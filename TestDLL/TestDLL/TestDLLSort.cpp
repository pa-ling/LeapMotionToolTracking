#include "TestDLLSort.h"
#include <algorithm>

extern "C" {

	void TESTDLLSORT_API TestSort(int a[], int length)
	{
		std::sort(a, a+length);
	}
}