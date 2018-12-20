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
	void __declspec(dllexport) ShowImage(char* string)
	{
		el::Configurations conf("D:\\Development\\Git\\LeapMotionToolTracking\\TestDLL\\TestDLL\\logging.conf");
		el::Loggers::reconfigureAllLoggers(conf);

		int length = strlen(string) + 1;
		char* res = (char*)malloc(length);
		strcpy_s(res, length, string);

		LOG(INFO) << "ShowImage > path: " << res;
		free(res);
		Mat image = imread(string, IMREAD_COLOR);
		imshow("Test Image", image);
		waitKey(1000); //Wait for 1 second before closing the window
		destroyWindow("Test Image");
	}

	/* This function gets image data and processes it*/
	void __declspec(dllexport) ProcessImageData(unsigned char* in, unsigned char* out, int width, int height) {
		Mat frame(height, width, CV_8UC4, in);

		// Process frame here …
		//cvtColor(frame, frame, COLOR_BGR2GRAY);
		cvtColor(frame, frame, COLOR_BGR2BGRA);
		memcpy(out, frame.data, frame.total() * frame.elemSize());
		//this shows the picture but with different colors
		//this because unity and opencv are using different ways to store colors
		//i think one uses BGRA and the other RGBA
		imshow("frame", frame);
	}

	void __declspec(dllexport) GetLeapDimensions(int dim[]) {
		int width, height;
		getDimensions(&width, &height);
		dim[0] = width;
		dim[1] = height;
	}

	void __declspec(dllexport) GetLeapImage(unsigned char* out, int index) {
		int width, height;
		getDimensions(&width, &height);
		unsigned char* image = new unsigned char[width * height];
		getImage(image, index);
		Mat frame(height, width, CV_8UC1, image);
		memcpy(out, frame.data, frame.total() * frame.elemSize());
	}
}