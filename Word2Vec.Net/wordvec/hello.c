#include <stdio.h>
#include "hello.h"

__stdcall void hello(char *s)
{
        printf("Hello %s\n", s);
}