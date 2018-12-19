#ifndef TESTDLLSORT_H
#define TESTDLLSORT_H

#include "ImageSample.h"

extern "C" {
	__declspec(dllexport) void SortIntArray(int a[], int length);
	__declspec(dllexport) void ShowImage(char path[]);
	__declspec(dllexport) void ProcessImageData(unsigned char* in, unsigned char* out, int width, int height);
	__declspec(dllexport) void GetLeapImages();
}

#endif