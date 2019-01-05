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
	void __declspec(dllexport) GetDepthMap(unsigned char* img0, unsigned char* img1, unsigned char* depthMap, int width, int height)
	{
		Mat leftImg(height, width, CV_8UC1, img0);
		Mat rightImg(height, width, CV_8UC1, img1);

		Mat disp, disp8;
		Ptr<StereoBM> sbm = StereoBM::create(16, 15);
		sbm->compute(leftImg, rightImg, disp);
		normalize(disp, disp8, 0, 255, NORM_MINMAX, CV_8U);
		//memcpy(depthMap, disp8.data, disp8.total() * disp8.elemSize());

		imshow("Image 1", leftImg);
		imshow("Image 2", rightImg);
		imshow("Disparity", disp8);
	}

	void configureLogging()
	{
		el::Configurations conf("D:\\Development\\Git\\LeapMotionToolTracking\\TestDLL\\TestDLL\\logging.conf");
		el::Loggers::reconfigureAllLoggers(conf);
	}
}