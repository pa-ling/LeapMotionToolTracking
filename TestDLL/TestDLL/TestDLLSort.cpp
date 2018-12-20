extern "C" {
#include "TestDLLSort.h"
}
#include <opencv2/opencv.hpp>

using namespace cv;
using namespace std;

extern "C" {

	/* This function tests data transport between DLL and caller */
	void __declspec(dllexport) SortIntArray(int a[], int length)
	{
		sort(a, a + length);
	}

	/* This function tests the availability of opencv */
	void __declspec(dllexport) ShowImage(char path[])
	{
		Mat image = imread(path, IMREAD_COLOR);
		imshow("Test Image", image);
		waitKey(1000); //Wait for 1 second before closing the window
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

	void __declspec(dllexport) GetLeapImages() {
		int width, height, index = 0;
		getDimensions(&width, &height);
		char* image0 = new char[width * height];
		char* image1 = new char[width * height];
		
		for (int i = 0; i < 1000; i++) {
			getImage(image0, 0);
			getImage(image1, 1);
			Mat frame0(height, width, CV_8UC1, image0);
			Mat frame1(height, width, CV_8UC1, image1);
			imshow("Leap1", frame0);
			imshow("Leap2", frame1);
			waitKey(100);
		}
	}
}