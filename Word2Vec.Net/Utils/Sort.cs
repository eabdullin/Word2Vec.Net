// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Sort.cs" company="Ingenius Systems">
//   Copyright (c) Ingenius Systems
//   Create on 20:40:29 by Еламан Абдуллин
// </copyright>
// <summary>
//   Defines the Sort type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace Word2Vec.Net.Utils
{
    public static class Sort
    {
        private static int Partition<T>(T[] m, int a, int b, IComparer<T> comparer) 
        {
            int i = a;
            for (int j = a; j <= b; j++)         // просматриваем с a по b
            {
                if (comparer.Compare(m[j], m[b]) < 0)  // если элемент m[j] не превосходит m[b],
                {
                    T t = m[i];                  // меняем местами m[j] и m[a], m[a+1], m[a+2] и так далее...
                    m[i] = m[j];                 // то есть переносим элементы меньшие m[b] в начало,
                    m[j] = t;                    // а затем и сам m[b] «сверху»
                    i++;                         // таким образом последний обмен: m[b] и m[i], после чего i++
                }
            }
            return i - 1;                        // в индексе i хранится <новая позиция элемента m[b]> + 1
        }

        public static void QSort<T>(T[] m, int a, int b, IComparer<T> comparer)// a - начало подмножества, b - конец
        {                                        // для первого вызова: a = 0, b = <элементов в массиве> - 1
            if (a >= b) return;
            int c = Partition(m, a, b, comparer);
            QSort(m, a, c - 1, comparer);
            QSort(m, c + 1, b, comparer);
        }
    }
}