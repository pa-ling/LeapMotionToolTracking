#ifndef TESTDLLSORT_H
#define TESTDLLSORT_H

extern "C" {
	__declspec(dllexport) void SortIntArray(int a[], int length);
	__declspec(dllexport) void ShowImage(char* path);
	__declspec(dllexport) void GetLeapImages(unsigned char* raw, unsigned char* img0, unsigned char* img1, int size);
	__declspec(dllexport) void GetDepthMap(unsigned char* img1, unsigned char* img2, unsigned char* disp, int width, int height);
}
void configureLogging();

#endif