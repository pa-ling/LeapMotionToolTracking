#include "TestDLLSort.h"
#include <algorithm>
#include <opencv2/core.hpp>
#include <opencv2/imgcodecs.hpp>
#include <opencv2/highgui.hpp>
#include <opencv2/opencv.hpp>
#include <iostream>

using namespace cv;
using namespace std;

extern "C" {

	void __declspec(dllexport) TestSort(int a[], int length)
	{
		sort(a, a + length);
	}

	int __declspec(dllexport) ShowImage()
	{
		char path[] = "C:\\Users\\paling\\Downloads\\eagle.png";
		Mat image;
		image = imread(path, IMREAD_COLOR); // Read the file

		namedWindow("Display window", WINDOW_AUTOSIZE); // Create a window for display.
		imshow("Display window", image); // Show our image inside it.
		waitKey(0); // Wait for a keystroke in the window

		return 23;
	}

	void __declspec(dllexport) processImage(unsigned char* raw, unsigned char* processed, int width, int height) {
		Mat frame(height, width, CV_8UC4, raw);

		// Process frame here �
		cvtColor(frame, frame, COLOR_BGR2GRAY);
		//cvtColor(frame, frame, COLOR_BGR2RGB);
		memcpy(processed, frame.data, width * height);
		imshow("frame", frame);
	}
}