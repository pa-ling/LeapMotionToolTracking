#ifndef TESTDLLSORT_H
#define TESTDLLSORT_H

extern "C" {
	__declspec(dllexport) void SortIntArray(int a[], int length);
	__declspec(dllexport) void ShowImage(char* path);
	__declspec(dllexport) void ConvertByteToColor(unsigned char* img8uc1, unsigned char* img8uc3, int width, int height);
	__declspec(dllexport) void CropImage(unsigned char* imgData, unsigned char* croppedImgData, int width, int height, int startX, int startY, int cropWidth, int cropHeight);
	__declspec(dllexport) void GetLeapImages(unsigned char* raw, unsigned char* img0, unsigned char* img1, int size);
	__declspec(dllexport) void GetDepthMap(unsigned char* img0, unsigned char* img1, unsigned char* depthMap, int width, int height);
}
void configureLogging();

#endif