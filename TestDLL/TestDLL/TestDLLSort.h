extern "C" {
	__declspec(dllexport) void TestSort(int a[], int length);
	__declspec(dllexport) int ShowImage();
	__declspec(dllexport) void processImage(unsigned char* raw, unsigned char* processed, int width, int height);
}