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

	void __declspec(dllexport) GetLeapImages(unsigned char* raw, unsigned char* img0, unsigned char* img1, int size)
	{
		memcpy(img0, raw, size);
		memcpy(img1, raw + size, size);
	}

	/* This function gets image data and processes it*/
	void __declspec(dllexport) GetDepthMap(unsigned char* img0, unsigned char* img1, unsigned char* disp, int width, int height)
	{
		unsigned char* raw0 = new unsigned char[width*height];
		unsigned char* raw1 = new unsigned char[width*height];
		memcpy(raw0, raw, width*height);
		memcpy(raw1, raw + width*height, width*height);

		Mat img0(height, width, CV_8UC1, raw0);
		Mat img1(height, width, CV_8UC1, raw1);
		//memcpy(out, frame.data, frame.total() * frame.elemSize());

		Mat disp, disp8;
		Ptr<StereoBM> sbm = StereoBM::create(16, 15);
		sbm->compute(img0, img1, disp);
		normalize(disp, disp8, 0, 255, NORM_MINMAX, CV_8U);

		imshow("Image 1", img0);
		imshow("Image 2", img1);
		imshow("Disparity", disp8);
	}

	void configureLogging()
	{
		el::Configurations conf("D:\\Development\\Git\\LeapMotionToolTracking\\TestDLL\\TestDLL\\logging.conf");
		el::Loggers::reconfigureAllLoggers(conf);
	}
}