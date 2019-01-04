extern "C" {
#include "TestDLLSort.h"
}
#include <opencv2/opencv.hpp>
#include "easylogging++.h"

INITIALIZE_EASYLOGGINGPP

using namespace cv;
using namespace std;

extern "C" {

	/* This function tests data transport between DLL and caller */
	void __declspec(dllexport) SortIntArray(int a[], int length)
	{
		sort(a, a + length);
	}

	/* This function tests the availability of opencv */
	void __declspec(dllexport) ShowImage(char* path)
	{
		configureLogging();

		int length = strlen(path) + 1;
		char* res = (char*)malloc(length);
		strcpy_s(res, length, path);

		LOG(INFO) << "ShowImage > path: " << res;
		free(res);
		Mat image = imread(path, IMREAD_COLOR);
		imshow("Test Image", image);
		waitKey(1000); //Wait for 1 second before closing the window
		destroyWindow("Test Image");
	}

	/* This function gets image data and processes it*/
	void __declspec(dllexport) ProcessImageData(unsigned char* raw, unsigned char* processed, int width, int height) {
		unsigned char* image0 = new unsigned char[width*height];
		unsigned char* image1 = new unsigned char[width*height];
		memcpy(image0, raw, width*height);
		memcpy(image1, raw + width*height, width*height);

		Mat img1(height, width, CV_8UC1, image0);
		Mat img2(height, width, CV_8UC1, image1);
		//memcpy(out, frame.data, frame.total() * frame.elemSize());
		imshow("Image 1", img1);
		imshow("Image 2", img2);
	}

	/*void __declspec(dllexport) GetLeapDimensions(int dim[]) {
		configureLogging();
		LOG(INFO) << "GetLeapDimensions";
		int width, height;
		getDimensions(&width, &height);
		dim[0] = width;
		dim[1] = height;
	}*/

	/*void __declspec(dllexport) GetLeapImage(unsigned char* out, int index) {
		int width, height;
		getDimensions(&width, &height);
		unsigned char* image = new unsigned char[width * height];
		getImage(image, index);
		Mat frame(height, width, CV_8UC1, image);
		memcpy(out, frame.data, frame.total() * frame.elemSize());
	}*/

	void configureLogging() {
		el::Configurations conf("D:\\Development\\Git\\LeapMotionToolTracking\\TestDLL\\TestDLL\\logging.conf");
		el::Loggers::reconfigureAllLoggers(conf);
	}
}