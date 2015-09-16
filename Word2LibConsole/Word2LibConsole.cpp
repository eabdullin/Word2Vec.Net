// Word2LibConsole.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include <time.h>
//#include <pthread.h>
#include <Windows.h>
#include <cstdlib>
#define MAX_STRING 100
#define EXP_TABLE_SIZE 1000
#define MAX_EXP 6
#define MAX_SENTENCE_LENGTH 1000
#define MAX_CODE_LENGTH 40

const int vocab_hash_size = 30000000; // Maximum 30 * 0.7 = 21M words in the vocabulary

typedef float real; // Precision of float numbers

struct vocab_word
{
	long long cn;
	int *point, codelen;
	char *word, *code;
};

char train_file[MAX_STRING], output_file[MAX_STRING];
char save_vocab_file[MAX_STRING], read_vocab_file[MAX_STRING];
struct vocab_word* vocab;
int binary = 0, cbow = 1, debug_mode = 2, window = 5, min_count = 5, num_threads = 12, min_reduce = 1;
int* vocab_hash;
long long vocab_max_size = 1000, vocab_size = 0, layer1_size = 100;
long long train_words = 0, word_count_actual = 0, iter = 5, file_size = 0, classes = 0;
real alpha = 0.025, starting_alpha, sample = 1e-3;
real *syn0, *syn1, *syn1neg, *expTable;
clock_t start;

int hs = 0, negative = 5;
const int table_size = 1e8;
int* table;

void InitUnigramTable()
{
	int a, i;
	double train_words_pow = 0;
	double d1, power = 0.75;
	table = (int *)malloc(table_size * sizeof(int));
	for (a = 0; a < vocab_size; a++) train_words_pow += pow(vocab[a].cn, power);
	i = 0;
	d1 = pow(vocab[i].cn, power) / train_words_pow;
	for (a = 0; a < table_size; a++)
	{
		table[a] = i;
		if (a / (double)table_size > d1)
		{
			i++;
			d1 += pow(vocab[i].cn, power) / train_words_pow;
		}
		if (i >= vocab_size) i = vocab_size - 1;
	}
}

// Reads a single word from a file, assuming space + tab + EOL to be word boundaries
void ReadWord(char* word, FILE* fin)
{
	int a = 0, ch;
	while (!feof(fin))
	{
		ch = fgetc(fin);
		if (ch == 13) continue;
		if ((ch == ' ') || (ch == '\t') || (ch == '\n'))
		{
			if (a > 0)
			{
				if (ch == '\n') ungetc(ch, fin);
				break;
			}
			if (ch == '\n')
			{
				strcpy_s(word, MAX_STRING, "</s>");
				return;
			}
			else continue;
		}
		word[a] = ch;
		a++;
		if (a >= MAX_STRING - 1) a--; // Truncate too long words
	}
	word[a] = 0;
}

// Returns hash value of a word
int GetWordHash(char* word)
{
	unsigned long long a, hash = 0;
	for (a = 0; a < strlen(word); a++) hash = hash * 257 + word[a];
	hash = hash % vocab_hash_size;
	return hash;
}

// Returns position of a word in the vocabulary; if the word is not found, returns -1
int SearchVocab(char* word)
{
	unsigned int hash = GetWordHash(word);
	while (1)
	{
		if (vocab_hash[hash] == -1) return -1;
		int index = vocab_hash[hash];
		if (!strcmp(word, vocab[index].word))
			return vocab_hash[hash];
		hash = (hash + 1) % vocab_hash_size;
	}
	return -1;
}

// Reads a word and returns its index in the vocabulary
int ReadWordIndex(FILE* fin)
{
	char word[MAX_STRING];
	ReadWord(word, fin);
	if (feof(fin)) return -1;
	return SearchVocab(word);
}

// Adds a word to the vocabulary
int AddWordToVocab(char* word)
{
	unsigned int hash, length = strlen(word) + 1;
	if (length > MAX_STRING) length = MAX_STRING;
	vocab[vocab_size].word = (char *)calloc(length, sizeof(char));
	strcpy_s(vocab[vocab_size].word, length, word);
	vocab[vocab_size].cn = 0;
	vocab_size++;
	// Reallocate memory if needed
	if (vocab_size + 2 >= vocab_max_size)
	{
		vocab_max_size += 1000;
		vocab = (struct vocab_word *)realloc(vocab, vocab_max_size * sizeof(struct vocab_word));
	}
	hash = GetWordHash(word);
	while (vocab_hash[hash] != -1) hash = (hash + 1) % vocab_hash_size;
	vocab_hash[hash] = vocab_size - 1;
	return vocab_size - 1;
}

// Used later for sorting by word counts
int VocabCompare(const void* a, const void* b)
{
	return ((struct vocab_word *)b)->cn - ((struct vocab_word *)a)->cn;
}

// Sorts the vocabulary by frequency using word counts
void SortVocab()
{
	int a, size;
	unsigned int hash;
	// Sort the vocabulary and keep </s> at the first position
	//qsort(&vocab[1], vocab_size - 1, sizeof(struct vocab_word), VocabCompare);

	std::qsort(&vocab[1], vocab_size - 1, sizeof(struct vocab_word), VocabCompare);
	//for (int i = 0; i < vocab_size; i++)
	//{
	//	printf("%s %lld\n", vocab[i].word, vocab[i].cn);
	//}
	for (a = 0; a < vocab_hash_size; a++) vocab_hash[a] = -1;
	size = vocab_size;
	train_words = 0;
	for (a = 0; a < size; a++)
	{
		// Words occuring less than min_count times will be discarded from the vocab
		if ((vocab[a].cn < min_count) && (a != 0))
		{
			vocab_size--;
			free(vocab[a].word);
		}
		else
		{
			// Hash will be re-computed, as after the sorting it is not actual
			hash = GetWordHash(vocab[a].word);
			while (vocab_hash[hash] != -1) hash = (hash + 1) % vocab_hash_size;
			vocab_hash[hash] = a;
			train_words += vocab[a].cn;
		}
	}
	vocab = (struct vocab_word *)realloc(vocab, (vocab_size + 1) * sizeof(struct vocab_word));
	// Allocate memory for the binary tree construction
	for (a = 0; a < vocab_size; a++)
	{
		vocab[a].code = (char *)calloc(MAX_CODE_LENGTH, sizeof(char));
		vocab[a].point = (int *)calloc(MAX_CODE_LENGTH, sizeof(int));
	}
}

// Reduces the vocabulary by removing infrequent tokens
void ReduceVocab()
{
	int a, b = 0;
	unsigned int hash;
	for (a = 0; a < vocab_size; a++)
		if (vocab[a].cn > min_reduce)
		{
			vocab[b].cn = vocab[a].cn;
			vocab[b].word = vocab[a].word;
			b++;
		}
		else free(vocab[a].word);
		vocab_size = b;
		for (a = 0; a < vocab_hash_size; a++) vocab_hash[a] = -1;
		for (a = 0; a < vocab_size; a++)
		{
			// Hash will be re-computed, as it is not actual
			hash = GetWordHash(vocab[a].word);
			while (vocab_hash[hash] != -1) hash = (hash + 1) % vocab_hash_size;
			vocab_hash[hash] = a;
		}
		fflush(stdout);
		min_reduce++;
}

// Create binary Huffman tree using the word counts
// Frequent words will have short uniqe binary codes
void CreateBinaryTree()
{
	long long a, b, i, min1i, min2i, pos1, pos2, point[MAX_CODE_LENGTH];
	char code[MAX_CODE_LENGTH];
	long long* count = (long long *)calloc(vocab_size * 2 + 1, sizeof(long long));
	long long* binary = (long long *)calloc(vocab_size * 2 + 1, sizeof(long long));
	long long* parent_node = (long long *)calloc(vocab_size * 2 + 1, sizeof(long long));
	for (a = 0; a < vocab_size; a++) count[a] = vocab[a].cn;
	for (a = vocab_size; a < vocab_size * 2; a++) count[a] = 1e15;
	pos1 = vocab_size - 1;
	pos2 = vocab_size;
	// Following algorithm constructs the Huffman tree by adding one node at a time
	for (a = 0; a < vocab_size - 1; a++)
	{
		// First, find two smallest nodes 'min1, min2'
		if (pos1 >= 0)
		{
			if (count[pos1] < count[pos2])
			{
				min1i = pos1;
				pos1--;
			}
			else
			{
				min1i = pos2;
				pos2++;
			}
		}
		else
		{
			min1i = pos2;
			pos2++;
		}
		if (pos1 >= 0)
		{
			if (count[pos1] < count[pos2])
			{
				min2i = pos1;
				pos1--;
			}
			else
			{
				min2i = pos2;
				pos2++;
			}
		}
		else
		{
			min2i = pos2;
			pos2++;
		}
		count[vocab_size + a] = count[min1i] + count[min2i];
		parent_node[min1i] = vocab_size + a;
		parent_node[min2i] = vocab_size + a;
		binary[min2i] = 1;
	}
	// Now assign binary code to each vocabulary word
	for (a = 0; a < vocab_size; a++)
	{
		b = a;
		i = 0;
		while (1)
		{
			code[i] = binary[b];
			point[i] = b;
			i++;
			b = parent_node[b];
			if (b == vocab_size * 2 - 2) break;
		}
		vocab[a].codelen = i;
		vocab[a].point[0] = vocab_size - 2;
		for (b = 0; b < i; b++)
		{
			vocab[a].code[i - b - 1] = code[b];
			vocab[a].point[i - b] = point[b] - vocab_size;
		}
	}
	free(count);
	free(binary);
	free(parent_node);
}

void LearnVocabFromTrainFile()
{
	char word[MAX_STRING];
	FILE* fin;
	long long a, i;
	for (a = 0; a < vocab_hash_size; a++) vocab_hash[a] = -1;
	int result = fopen_s(&fin, train_file, "rb");
	if (fin == NULL)
	{
		printf("ERROR: training data file not found!\n");
		exit(1);
	}
	vocab_size = 0;
	AddWordToVocab((char *)"</s>");
	while (1)
	{
		ReadWord(word, fin);
		if (feof(fin)) break;
		train_words++;
		if ((debug_mode > 1) && (train_words % 100000 == 0))
		{
			printf("%lldK%c", train_words / 1000, 13);
			fflush(stdout);
		}
		i = SearchVocab(word);
		if (i == -1)
		{
			a = AddWordToVocab(word);
			vocab[a].cn = 1;
		}
		else vocab[i].cn++;
		if (vocab_size > vocab_hash_size * 0.7)
			ReduceVocab();
	}
	SortVocab();
	if (debug_mode > 0)
	{
		printf("Vocab size: %lld\n", vocab_size);
		printf("Words in train file: %lld\n", train_words);
	}
	file_size = ftell(fin);
	fclose(fin);
}

void SaveVocab()
{
	long long i;
	FILE* fo;
	fopen_s(&fo, save_vocab_file, "wb");
	for (i = 0; i < vocab_size; i++) fprintf(fo, "%s %lld\n", vocab[i].word, vocab[i].cn);
	fclose(fo);
}

void ReadVocab()
{
	long long a, i = 0;
	char c;
	char word[MAX_STRING];
	FILE* fin;
	fopen_s(&fin, read_vocab_file, "rb");
	if (fin == NULL)
	{
		printf("Vocabulary file not found\n");
		exit(1);
	}
	for (a = 0; a < vocab_hash_size; a++) vocab_hash[a] = -1;
	vocab_size = 0;
	while (1)
	{
		ReadWord(word, fin);
		if (feof(fin)) break;
		a = AddWordToVocab(word);
		fscanf_s(fin, "%lld%c", &vocab[a].cn, &c);
		i++;
	}
	SortVocab();
	if (debug_mode > 0)
	{
		printf("Vocab size: %lld\n", vocab_size);
		printf("Words in train file: %lld\n", train_words);
	}
	fopen_s(&fin, train_file, "rb");
	if (fin == NULL)
	{
		printf("ERROR: training data file not found!\n");
		exit(1);
	}
	fseek(fin, 0, SEEK_END);
	file_size = ftell(fin);
	fclose(fin);
}

void InitNet()
{
	long long a, b;
	unsigned long long next_random = 1;
	//a = posix_memalign((void **)&syn0, 128, (long long)vocab_size * layer1_size * sizeof(real));
	syn0 = (real*)_aligned_malloc((long long)vocab_size * layer1_size * sizeof(real), 128);
	if (syn0 == NULL)
	{
		printf("Memory allocation failed\n");
		exit(1);
	}
	if (hs)
	{
		//a = posix_memalign((void **)&syn1, 128, (long long)vocab_size * layer1_size * sizeof(real));
		syn1 = (real*)_aligned_malloc((long long)vocab_size * layer1_size * sizeof(real), 128);
		if (syn1 == NULL)
		{
			printf("Memory allocation failed\n");
			exit(1);
		}
		for (a = 0; a < vocab_size; a++)
			for (b = 0; b < layer1_size; b++)
				syn1[a * layer1_size + b] = 0;
	}
	if (negative > 0)
	{
		//a = posix_memalign((void **)&syn1neg, 128, (long long)vocab_size * layer1_size * sizeof(real));
		syn1neg = (real*)_aligned_malloc((long long)vocab_size * layer1_size * sizeof(real), 128);
		if (syn1neg == NULL)
		{
			printf("Memory allocation failed\n");
			exit(1);
		}
		for (a = 0; a < vocab_size; a++)
			for (b = 0; b < layer1_size; b++)
				syn1neg[a * layer1_size + b] = 0;
	}
	for (a = 0; a < vocab_size; a++)
		for (b = 0; b < layer1_size; b++)
		{
			next_random = next_random * (unsigned long long)25214903917 + 11;
			syn0[a * layer1_size + b] = (((next_random & 0xFFFF) / (real)65536) - 0.5) / layer1_size;
		}
	CreateBinaryTree();
}

DWORD WINAPI TrainModelThread(LPVOID id)
{
	long long a, b, d, cw, word, last_word, sentence_length = 0, sentence_position = 0;
	long long word_count = 0, last_word_count = 0, sen[MAX_SENTENCE_LENGTH + 1];
	long long l1, l2, c, target, label, local_iter = iter;
	unsigned long long next_random = (long long)id;
	real f, g;
	clock_t now;
	real* neu1 = (real *)calloc(layer1_size, sizeof(real));
	real* neu1e = (real *)calloc(layer1_size, sizeof(real));
	FILE* fi;
	fopen_s(&fi, train_file, "rb");
	fseek(fi, file_size / (long long)num_threads * (long long)id, SEEK_SET);
	while (1)
	{
		if (word_count - last_word_count > 10000)
		{
			word_count_actual += word_count - last_word_count;
			last_word_count = word_count;
			if ((debug_mode > 1))
			{
				now = clock();
				printf("%cAlpha: %f  Progress: %.2f%%  Words/thread/sec: %.2fk  ", 13, alpha,
					word_count_actual / (real)(iter * train_words + 1) * 100,
					word_count_actual / ((real)(now - start + 1) / (real)CLOCKS_PER_SEC * 1000));
				fflush(stdout);
			}
			alpha = starting_alpha * (1 - word_count_actual / (real)(iter * train_words + 1));
			if (alpha < starting_alpha * 0.0001) alpha = starting_alpha * 0.0001;
		}
		if (sentence_length == 0)
		{
			while (1)
			{
				word = ReadWordIndex(fi);
				if (feof(fi)) break;
				if (word == -1) continue;
				word_count++;
				if (word == 0) break;
				// The subsampling randomly discards frequent words while keeping the ranking same
				if (sample > 0)
				{
					real ran = (sqrt(vocab[word].cn / (sample * train_words)) + 1) * (sample * train_words) / vocab[word].cn;
					next_random = next_random * (unsigned long long)25214903917 + 11;
					if (ran < (next_random & 0xFFFF) / (real)65536) continue;
				}
				sen[sentence_length] = word;
				sentence_length++;
				if (sentence_length >= MAX_SENTENCE_LENGTH) break;
			}
			sentence_position = 0;
		}
		if (feof(fi) || (word_count > train_words / num_threads))
		{
			word_count_actual += word_count - last_word_count;
			local_iter--;
			if (local_iter == 0) break;
			word_count = 0;
			last_word_count = 0;
			sentence_length = 0;
			fseek(fi, file_size / (long long)num_threads * (long long)id, SEEK_SET);
			continue;
		}
		word = sen[sentence_position];
		if (word == -1) continue;
		for (c = 0; c < layer1_size; c++) neu1[c] = 0;
		for (c = 0; c < layer1_size; c++) neu1e[c] = 0;
		next_random = next_random * (unsigned long long)25214903917 + 11;
		b = next_random % window;
		if (cbow)
		{ //train the cbow architecture
			// in -> hidden
			cw = 0;
			for (a = b; a < window * 2 + 1 - b; a++)
				if (a != window)
				{
					c = sentence_position - window + a;
					if (c < 0) continue;
					if (c >= sentence_length) continue;
					last_word = sen[c];
					if (last_word == -1) continue;
					for (c = 0; c < layer1_size; c++) neu1[c] += syn0[c + last_word * layer1_size];
					cw++;
				}
			if (cw)
			{
				for (c = 0; c < layer1_size; c++) neu1[c] /= cw;
				if (hs)
					for (d = 0; d < vocab[word].codelen; d++)
					{
						f = 0;
						l2 = vocab[word].point[d] * layer1_size;
						// Propagate hidden -> output
						for (c = 0; c < layer1_size; c++) f += neu1[c] * syn1[c + l2];
						if (f <= -MAX_EXP) continue;
						else if (f >= MAX_EXP) continue;
						else f = expTable[(int)((f + MAX_EXP) * (EXP_TABLE_SIZE / MAX_EXP / 2))];
						// 'g' is the gradient multiplied by the learning rate
						g = (1 - vocab[word].code[d] - f) * alpha;
						// Propagate errors output -> hidden
						for (c = 0; c < layer1_size; c++) neu1e[c] += g * syn1[c + l2];
						// Learn weights hidden -> output
						for (c = 0; c < layer1_size; c++) syn1[c + l2] += g * neu1[c];
					}
				// NEGATIVE SAMPLING
				if (negative > 0)
					for (d = 0; d < negative + 1; d++)
					{
						if (d == 0)
						{
							target = word;
							label = 1;
						}
						else
						{
							next_random = next_random * (unsigned long long)25214903917 + 11;
							target = table[(next_random >> 16) % table_size];
							if (target == 0) target = next_random % (vocab_size - 1) + 1;
							if (target == word) continue;
							label = 0;
						}
						l2 = target * layer1_size;
						f = 0;
						for (c = 0; c < layer1_size; c++) f += neu1[c] * syn1neg[c + l2];
						if (f > MAX_EXP) g = (label - 1) * alpha;
						else if (f < -MAX_EXP) g = (label - 0) * alpha;
						else g = (label - expTable[(int)((f + MAX_EXP) * (EXP_TABLE_SIZE / MAX_EXP / 2))]) * alpha;
						for (c = 0; c < layer1_size; c++) neu1e[c] += g * syn1neg[c + l2];
						for (c = 0; c < layer1_size; c++) syn1neg[c + l2] += g * neu1[c];
					}
				// hidden -> in
				for (a = b; a < window * 2 + 1 - b; a++)
					if (a != window)
					{
						c = sentence_position - window + a;
						if (c < 0) continue;
						if (c >= sentence_length) continue;
						last_word = sen[c];
						if (last_word == -1) continue;
						for (c = 0; c < layer1_size; c++) syn0[c + last_word * layer1_size] += neu1e[c];
					}
			}
		}
		else
		{ //train skip-gram
			for (a = b; a < window * 2 + 1 - b; a++)
				if (a != window)
				{
					c = sentence_position - window + a;
					if (c < 0) continue;
					if (c >= sentence_length) continue;
					last_word = sen[c];
					if (last_word == -1) continue;
					l1 = last_word * layer1_size;
					for (c = 0; c < layer1_size; c++) neu1e[c] = 0;
					// HIERARCHICAL SOFTMAX
					if (hs)
						for (d = 0; d < vocab[word].codelen; d++)
						{
							f = 0;
							l2 = vocab[word].point[d] * layer1_size;
							// Propagate hidden -> output
							for (c = 0; c < layer1_size; c++) f += syn0[c + l1] * syn1[c + l2];
							if (f <= -MAX_EXP) continue;
							else if (f >= MAX_EXP) continue;
							else f = expTable[(int)((f + MAX_EXP) * (EXP_TABLE_SIZE / MAX_EXP / 2))];
							// 'g' is the gradient multiplied by the learning rate
							g = (1 - vocab[word].code[d] - f) * alpha;
							// Propagate errors output -> hidden
							for (c = 0; c < layer1_size; c++) neu1e[c] += g * syn1[c + l2];
							// Learn weights hidden -> output
							for (c = 0; c < layer1_size; c++) syn1[c + l2] += g * syn0[c + l1];
						}
					// NEGATIVE SAMPLING
					if (negative > 0)
						for (d = 0; d < negative + 1; d++)
						{
							if (d == 0)
							{
								target = word;
								label = 1;
							}
							else
							{
								next_random = next_random * (unsigned long long)25214903917 + 11;
								target = table[(next_random >> 16) % table_size];
								if (target == 0) target = next_random % (vocab_size - 1) + 1;
								if (target == word) continue;
								label = 0;
							}
							l2 = target * layer1_size;
							f = 0;
							for (c = 0; c < layer1_size; c++) f += syn0[c + l1] * syn1neg[c + l2];
							if (f > MAX_EXP) g = (label - 1) * alpha;
							else if (f < -MAX_EXP) g = (label - 0) * alpha;
							else g = (label - expTable[(int)((f + MAX_EXP) * (EXP_TABLE_SIZE / MAX_EXP / 2))]) * alpha;
							for (c = 0; c < layer1_size; c++) neu1e[c] += g * syn1neg[c + l2];
							for (c = 0; c < layer1_size; c++) syn1neg[c + l2] += g * syn0[c + l1];
						}
					// Learn weights input -> hidden
					for (c = 0; c < layer1_size; c++) syn0[c + l1] += neu1e[c];
				}
		}
		sentence_position++;
		if (sentence_position >= sentence_length)
		{
			sentence_length = 0;
			continue;
		}
	}
	fclose(fi);
	free(neu1);
	free(neu1e);
	//pthread_exit(NULL);
	return 0;
}

void TrainModel()
{
	long a, b, c, d;
	FILE* fo;
	//std::thread *pt = (std::thread *)malloc(num_threads * sizeof(std::thread));
	HANDLE* pt = (HANDLE *)malloc(num_threads * sizeof(HANDLE));
	printf("Starting training using file %s\n", train_file);
	starting_alpha = alpha;
	if (read_vocab_file[0] != 0)
		ReadVocab();
	else
		LearnVocabFromTrainFile();
	if (save_vocab_file[0] != 0)
		SaveVocab();
	if (output_file[0] == 0) return;
	InitNet();
	if (negative > 0) InitUnigramTable();
	start = clock();
	//TrainModelThread(0);
	for (a = 0; a < num_threads; a++)
	{
		pt[a] = CreateThread(NULL, 0, TrainModelThread, (void *)a, 0, 0);
		//pt[a] = std::thread(TrainModelThread, a);
		//pthread_create(&pt[a], NULL, TrainModelThread, (void *)a);
	}
	for (a = 0; a < num_threads; a++)
	{
		WaitForSingleObject(pt[a], INFINITE);
		//pt[a].join();
		//pthread_join(pt[a], NULL);
	}
	fopen_s(&fo, output_file, "wb");
	if (classes == 0)
	{
		// Save the word vectors
		fprintf(fo, "%lld %lld\n", vocab_size, layer1_size);
		for (a = 0; a < vocab_size; a++)
		{
			fprintf(fo, "%s ", vocab[a].word);
			if (binary) for (b = 0; b < layer1_size; b++) fwrite(&syn0[a * layer1_size + b], sizeof(real), 1, fo);
			else for (b = 0; b < layer1_size; b++) fprintf(fo, "%lf ", syn0[a * layer1_size + b]);
			fprintf(fo, "\n");
		}
	}
	else
	{
		// Run K-means on the word vectors
		int clcn = classes, iter = 10, closeid;
		int* centcn = (int *)malloc(classes * sizeof(int));
		int* cl = (int *)calloc(vocab_size, sizeof(int));
		real closev, x;
		real* cent = (real *)calloc(classes * layer1_size, sizeof(real));
		for (a = 0; a < vocab_size; a++) cl[a] = a % clcn;
		for (a = 0; a < iter; a++)
		{
			for (b = 0; b < clcn * layer1_size; b++) cent[b] = 0;
			for (b = 0; b < clcn; b++) centcn[b] = 1;
			for (c = 0; c < vocab_size; c++)
			{
				for (d = 0; d < layer1_size; d++) cent[layer1_size * cl[c] + d] += syn0[c * layer1_size + d];
				centcn[cl[c]]++;
			}
			for (b = 0; b < clcn; b++)
			{
				closev = 0;
				for (c = 0; c < layer1_size; c++)
				{
					cent[layer1_size * b + c] /= centcn[b];
					closev += cent[layer1_size * b + c] * cent[layer1_size * b + c];
				}
				closev = sqrt(closev);
				for (c = 0; c < layer1_size; c++) cent[layer1_size * b + c] /= closev;
			}
			for (c = 0; c < vocab_size; c++)
			{
				closev = -10;
				closeid = 0;
				for (d = 0; d < clcn; d++)
				{
					x = 0;
					for (b = 0; b < layer1_size; b++) x += cent[layer1_size * d + b] * syn0[c * layer1_size + b];
					if (x > closev)
					{
						closev = x;
						closeid = d;
					}
				}
				cl[c] = closeid;
			}
		}
		// Save the K-means classes
		for (a = 0; a < vocab_size; a++) fprintf(fo, "%s %d\n", vocab[a].word, cl[a]);
		free(centcn);
		free(cent);
		free(cl);
	}
	free(pt);
	fclose(fo);
}

char* TO_CHAR(wchar_t* pWCBuffer)
{
	size_t i;
	char pMBBuffer[MAX_STRING];
	wcstombs_s(&i, pMBBuffer, (size_t)MAX_STRING, pWCBuffer, (size_t)MAX_STRING);
	return pMBBuffer;
}

int ArgPos(char* str, int argc, wchar_t** argv)
{
	int a;
	for (a = 1; a < argc; a++)
		if (!strcmp(str, TO_CHAR(argv[a])))
		{
			if (a == argc - 1)
			{
				printf("Argument missing for %s\n", str);
				exit(1);
			}
			return a;
		}
	return -1;
}

void TrainModelWithParameters(
	char* trainFileName,
	char* outPutfileName,
	char* saveVocabFileName,
	char* readVocubFileName,
	long long size,
	int debugMode,
	int binaryMode,
	int cbowMode,
	real alphaValue,
	real sampleValue,
	int hsMode,
	int negativeMode,
	int threads,
	long long iterValue,
	int minCount,
	long long classesValue,
	int windowMode)
{
	strcpy_s(train_file, strlen(trainFileName), trainFileName);
	strcpy_s(output_file, strlen(outPutfileName), outPutfileName);
	strcpy_s(save_vocab_file, strlen(saveVocabFileName), saveVocabFileName);
	strcpy_s(read_vocab_file, strlen(readVocubFileName), readVocubFileName);
	layer1_size = size;
	debug_mode = debugMode;
	binary = binaryMode,
		cbow = cbowMode;
	alpha = alphaValue;
	sample = sampleValue;
	hs = hsMode;
	negative = negativeMode;
	num_threads = threads;
	iter = iterValue;
	min_count = minCount;
	classes = classesValue;
	window = windowMode;

	vocab = (struct vocab_word *)calloc(vocab_max_size, sizeof(struct vocab_word));
	vocab_hash = (int *)calloc(vocab_hash_size, sizeof(int));
	expTable = (real *)malloc((EXP_TABLE_SIZE + 1) * sizeof(real));
	for (int i = 0; i < EXP_TABLE_SIZE; i++)
	{
		expTable[i] = exp((i / (real)EXP_TABLE_SIZE * 2 - 1) * MAX_EXP); // Precompute the exp() table
		expTable[i] = expTable[i] / (expTable[i] + 1); // Precompute f(x) = x / (x + 1)
	}
	TrainModel();
}
const long long max_size = 2000;         // max length of strings
const long long N = 40;                  // number of closest words that will be shown
const long long max_w = 50;
int Distance(char* file_name){
	FILE *f;
	char st1[max_size];
	char *bestw[N];
	char  st[100][max_size]; //file_name[max_size],
	float dist, len, bestd[N], vec[max_size];
	long long words, size, a, b, c, d, cn, bi[100];
	char ch;
	float *M;
	char *vocab;
	//if (argc < 2) {
	//	printf("Usage: ./distance <FILE>\nwhere FILE contains word projections in the BINARY FORMAT\n");
	//	return 0;
	//}
	//strcpy(file_name, argv[1]);
	//f = fopen(file_name, "rb");
	fopen_s(&f, file_name, "rb");
	if (f == NULL) {
		printf("Input file not found\n");
		return -1;
	}

	fscanf_s(f, "%lld", &words);
	fscanf_s(f, "%lld", &size);
	vocab = (char *)malloc((long long)words * max_w * sizeof(char));
	for (a = 0; a < N; a++) bestw[a] = (char *)malloc(max_size * sizeof(char));
	M = (float *)malloc((long long)words * (long long)size * sizeof(float));
	if (M == NULL) {
		printf("Cannot allocate memory: %lld MB    %lld  %lld\n", (long long)words * size * sizeof(float) / 1048576, words, size);
		return -1;
	}
	for (b = 0; b < words; b++) {
		a = 0;
		while (1) {
			vocab[b * max_w + a] = fgetc(f);
			//printf("%c", vocab[b * max_w + a]);
			if (feof(f) || (vocab[b * max_w + a] == ' ')) break;
			if ((a < max_w) && (vocab[b * max_w + a] != '\n')) a++;
		}
		vocab[b * max_w + a] = 0;
		for (a = 0; a < size; a++) fread(&M[a + b * size], sizeof(float), 1, f);
		len = 0;
		for (a = 0; a < size; a++) len += M[a + b * size] * M[a + b * size];
		len = sqrt(len);
		for (a = 0; a < size; a++) M[a + b * size] /= len;
	}

	fclose(f);
	while (1) {
		for (a = 0; a < N; a++) bestd[a] = 0;
		for (a = 0; a < N; a++) bestw[a][0] = 0;
		printf("Enter word or sentence (EXIT to break): ");
		a = 0;
		//while (1) {
		//	st1[a] = fgetc(stdin);
		//	if ((st1[a] == '\n') || (a >= max_size - 1)) {
		//		st1[a] = 0;
		//		break;
		//	}
		//	a++;
		//}
		strcpy_s(st1,N, "дейін");
		if (!strcmp(st1, "EXIT")) break;
		cn = 0;
		b = 0;
		c = 0;
		while (1) {
			st[cn][b] = st1[c];
			b++;
			c++;
			st[cn][b] = 0;
			if (st1[c] == 0) break;
			if (st1[c] == ' ') {
				cn++;
				b = 0;
				c++;
			}
		}
		cn++;
		int result = strcmp("dark", "result");
		result = strcmp("dark", "dark");
		for (a = 0; a < cn; a++) {
			for (b = 0; b < words; b++) if (!strcmp(&vocab[b * max_w], st[a])) break;
			if (b == words) b = -1;
			bi[a] = b;
			printf("\nWord: %s  Position in vocabulary: %lld\n", st[a], bi[a]);
			if (b == -1) {
				printf("Out of dictionary word!\n");
				break;
			}
		}
		if (b == -1) continue;
		printf("\n                                              Word       Cosine distance\n------------------------------------------------------------------------\n");
		for (a = 0; a < size; a++) vec[a] = 0;
		for (b = 0; b < cn; b++) {
			if (bi[b] == -1) continue;
			for (a = 0; a < size; a++) vec[a] += M[a + bi[b] * size];
		}
		len = 0;
		for (a = 0; a < size; a++) len += vec[a] * vec[a];
		len = sqrt(len);
		for (a = 0; a < size; a++) vec[a] /= len;
		for (a = 0; a < N; a++) bestd[a] = -1;
		for (a = 0; a < N; a++) bestw[a][0] = 0;
		for (c = 0; c < words; c++) {
			a = 0;
			for (b = 0; b < cn; b++) if (bi[b] == c) a = 1;
			if (a == 1) continue;
			dist = 0;
			for (a = 0; a < size; a++) dist += vec[a] * M[a + c * size];
			for (a = 0; a < N; a++) {
				if (dist > bestd[a]) {
					for (d = N - 1; d > a; d--) {
						bestd[d] = bestd[d - 1];
						//strcpy(bestw[d], bestw[d - 1]);
						strcpy_s(bestw[d], N, bestw[d - 1]);
					}
					bestd[a] = dist;
					strcpy_s(bestw[a], N, &vocab[c * max_w]);
					break;
				}
			}
		}
		for (a = 0; a < N; a++) printf("%50s\t\t%f\n", bestw[a], bestd[a]);
	}
	return 0;
}
int _tmain(int argc, _TCHAR* argv[])
{
	int i;
	if (argc == 1)
	{
		printf("WORD VECTOR estimation toolkit v 0.1c\n\n");
		printf("Options:\n");
		printf("Parameters for training:\n");
		printf("\t-train <file>\n");
		printf("\t\tUse text data from <file> to train the model\n");
		printf("\t-output <file>\n");
		printf("\t\tUse <file> to save the resulting word vectors / word clusters\n");
		printf("\t-size <int>\n");
		printf("\t\tSet size of word vectors; default is 100\n");
		printf("\t-window <int>\n");
		printf("\t\tSet max skip length between words; default is 5\n");
		printf("\t-sample <float>\n");
		printf("\t\tSet threshold for occurrence of words. Those that appear with higher frequency in the training data\n");
		printf("\t\twill be randomly down-sampled; default is 1e-3, useful range is (0, 1e-5)\n");
		printf("\t-hs <int>\n");
		printf("\t\tUse Hierarchical Softmax; default is 0 (not used)\n");
		printf("\t-negative <int>\n");
		printf("\t\tNumber of negative examples; default is 5, common values are 3 - 10 (0 = not used)\n");
		printf("\t-threads <int>\n");
		printf("\t\tUse <int> threads (default 12)\n");
		printf("\t-iter <int>\n");
		printf("\t\tRun more training iterations (default 5)\n");
		printf("\t-min-count <int>\n");
		printf("\t\tThis will discard words that appear less than <int> times; default is 5\n");
		printf("\t-alpha <float>\n");
		printf("\t\tSet the starting learning rate; default is 0.025 for skip-gram and 0.05 for CBOW\n");
		printf("\t-classes <int>\n");
		printf("\t\tOutput word classes rather than word vectors; default number of classes is 0 (vectors are written)\n");
		printf("\t-debug <int>\n");
		printf("\t\tSet the debug mode (default = 2 = more info during training)\n");
		printf("\t-binary <int>\n");
		printf("\t\tSave the resulting vectors in binary moded; default is 0 (off)\n");
		printf("\t-save-vocab <file>\n");
		printf("\t\tThe vocabulary will be saved to <file>\n");
		printf("\t-read-vocab <file>\n");
		printf("\t\tThe vocabulary will be read from <file>, not constructed from the training data\n");
		printf("\t-cbow <int>\n");
		printf("\t\tUse the continuous bag of words model; default is 1 (use 0 for skip-gram model)\n");
		printf("\nExamples:\n");
		printf("./word2vec -train data.txt -output vec.txt -size 200 -window 5 -sample 1e-4 -negative 5 -hs 0 -binary 0 -cbow 1 -iter 3\n\n");
		return 0;
	}
	output_file[0] = 0;
	save_vocab_file[0] = 0;
	read_vocab_file[0] = 0;
	if ((i = ArgPos((char *)"-size", argc, argv)) > 0) layer1_size = atoi(TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-train", argc, argv)) > 0) strcpy_s(train_file, MAX_STRING, TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-save-vocab", argc, argv)) > 0) strcpy_s(save_vocab_file, MAX_STRING, TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-read-vocab", argc, argv)) > 0) strcpy_s(read_vocab_file, MAX_STRING, TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-debug", argc, argv)) > 0) debug_mode = atoi(TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-binary", argc, argv)) > 0) binary = atoi(TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-cbow", argc, argv)) > 0) cbow = atoi(TO_CHAR(argv[i + 1]));
	if (cbow) alpha = 0.05;
	if ((i = ArgPos((char *)"-alpha", argc, argv)) > 0) alpha = atof(TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-output", argc, argv)) > 0) strcpy_s(output_file, MAX_STRING, TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-window", argc, argv)) > 0) window = atoi(TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-sample", argc, argv)) > 0) sample = atof(TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-hs", argc, argv)) > 0) hs = atoi(TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-negative", argc, argv)) > 0) negative = atoi(TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-threads", argc, argv)) > 0) num_threads = atoi(TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-iter", argc, argv)) > 0) iter = atoi(TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-min-count", argc, argv)) > 0) min_count = atoi(TO_CHAR(argv[i + 1]));
	if ((i = ArgPos((char *)"-classes", argc, argv)) > 0) classes = atoi(TO_CHAR(argv[i + 1]));
	vocab = (struct vocab_word *)calloc(vocab_max_size, sizeof(struct vocab_word));
	vocab_hash = (int *)calloc(vocab_hash_size, sizeof(int));
	expTable = (real *)malloc((EXP_TABLE_SIZE + 1) * sizeof(real));
	for (i = 0; i < EXP_TABLE_SIZE; i++)
	{
		expTable[i] = exp((i / (real)EXP_TABLE_SIZE * 2 - 1) * MAX_EXP); // Precompute the exp() table
		expTable[i] = expTable[i] / (expTable[i] + 1); // Precompute f(x) = x / (x + 1)
	}
	TrainModel();
	return 0;
}


