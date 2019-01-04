#ifndef TESTDLLSORT_H
#define TESTDLLSORT_H

extern "C" {
	__declspec(dllexport) void SortIntArray(int a[], int length);
	__declspec(dllexport) void ShowImage(char* path);
	__declspec(dllexport) void ProcessImageData(unsigned char* in, unsigned char* out, int width, int height);
	//__declspec(dllexport) void GetLeapDimensions(int dim[]);
	//__declspec(dllexport) void GetLeapImage(unsigned char* out, int index);
}
void configureLogging();

#endif