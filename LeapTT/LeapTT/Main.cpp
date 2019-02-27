extern "C" {
#include "Main.h"
}
#include <opencv2/opencv.hpp>
#include "easylogging++.h"
#include "Locator.h"
#include "Marker.h"

INITIALIZE_EASYLOGGINGPP

using namespace cv;
using namespace std;

extern "C" {

	bool DEBUG = true;
	int MASK_RADIUS = 20;

	Marker prevData[2][2];

	void __declspec(dllexport) Init(bool debug)
	{
		LOG(INFO) << "Initalizing LeapTT.";
		LOG(INFO) << "Debug: " << debug;
		prevData[0][0] = Marker(200, 200 , 1);
		prevData[0][1] = Marker(200, 0, 1);
		prevData[1][0] = Marker(200, 200, 1);
		prevData[1][1] = Marker(200, 0, 1);
		//TODO: Initialize marker0 at the top of the picture and marker1 at the bottom?
		DEBUG = debug;
	}

	void __declspec(dllexport) GetLeapImages(unsigned char* raw, unsigned char* img0Data, unsigned char* img1Data, int width, int height)
	{
		int size = width * height;
		memcpy(img0Data, raw, size);
		memcpy(img1Data, raw + size, size);

		if (DEBUG) {
			Mat img0(height, width, CV_8UC1, img0Data);
			Mat img1(height, width, CV_8UC1, img1Data);
			imshow("Raw Img 0", img0);
			imshow("Raw Img 1", img1);
		}
	}

	void __declspec(dllexport) CropImage(unsigned char* imgData, unsigned char* croppedImgData, int width, int height, int startX, int startY, int cropWidth, int cropHeight)
	{
		Mat image(height, width, CV_8UC1, imgData);
		Mat ROI(image, Rect(startX, startY, cropWidth, cropHeight));

		Mat croppedImage;
		ROI.copyTo(croppedImage);

		memcpy(croppedImgData, croppedImage.data, croppedImage.total() * croppedImage.elemSize());
	}

	void __declspec(dllexport) ConvertByteToColor(unsigned char* img8uc1, unsigned char* img8uc3, int width, int height)
	{
		Mat singleChannelImage(height, width, CV_8UC1, img8uc1);

		// copy channel 0 from the first image to all channels of the second image
		int from_to[] = { 0,0, 0,1, 0,2 };
		Mat threeChannelImage(singleChannelImage.size(), CV_8UC3);
		mixChannels(&singleChannelImage, 1, &threeChannelImage, 1, from_to, 3);
		cvtColor(threeChannelImage, threeChannelImage, COLOR_RGB2RGBA);

		memcpy(img8uc3, threeChannelImage.data, threeChannelImage.total() * threeChannelImage.elemSize());
	}

	void __declspec(dllexport) GetMarkerLocations(unsigned char* imgData, float markerLocations[], int width, int height, int camera)
	{
		Mat img(height, width, CV_8UC1, imgData);
		Mat iThreshImg = Mat(img.size(), CV_8UC3);
		Mat aThreshImg = Mat(img.size(), CV_8UC3);

		// Prefilter so that environment will not be recognized as marker when no marker is present
		threshold(img, iThreshImg, 125, 255, THRESH_TOZERO);

		// Get brightest point in the picture = first marker
		double minVal; double maxVal; Point minLoc; Point maxLoc;
		minMaxLoc(iThreshImg, &minVal, &maxVal, &minLoc, &maxLoc, Mat());

		// limit brightness range where both markers are located
		threshold(iThreshImg, aThreshImg, maxVal - 25, 255, THRESH_BINARY);

		// check if markers are visible (white threshold image)
		minMaxLoc(aThreshImg, &minVal, &maxVal, &minLoc, &maxLoc, Mat());
		if (255 == minVal) {
			Init(DEBUG);
			return;
		}

		// Get contours
		vector<vector<Point>> contours;
		vector<Vec4i> hierarchy;
		findContours(aThreshImg, contours, hierarchy, RETR_TREE, CHAIN_APPROX_SIMPLE, Point(0, 0));

		// Get bounding circles of contours
		vector<Point2f>center(contours.size());
		vector<float>radius(contours.size());
		for (int i = 0; i < contours.size(); i++) {
			minEnclosingCircle((Mat)contours[i], center[i], radius[i]);
		}

		LOG(INFO) << camera << "(x0:" 
			<< prevData[camera][0].getX() << ", y0:"
			<< prevData[camera][0].getY() << ", r0:"
			<< prevData[camera][0].getR() << ", x1:"
			<< prevData[camera][1].getX() << ", y1:"
			<< prevData[camera][1].getY() << ", r1:"
			<< prevData[camera][1].getR() << ")";
		LOG(INFO) << camera << "*: " << center;

		Marker *newData = Locator::findMarkers(center, radius, prevData[camera]);

		markerLocations[0] = newData[0].getX();
		markerLocations[1] = newData[0].getY();
		markerLocations[2] = newData[1].getX();
		markerLocations[3] = newData[1].getY();

		// Show several debug windows
		if (DEBUG) {
			imshow("Undist Img " + to_string(camera), img);
			//imshow("Init Thresh Img " + to_string(camera), iThreshImg);
			imshow("Adapt Thresh Img " + to_string(camera), aThreshImg);
			Mat drawing = Mat::zeros(img.size(), CV_8UC3);
			for (int i = 0; i < contours.size(); i++) {
				Scalar color = Scalar(0, 0, 255);
				circle(drawing, center[i], (int)radius[i], color, 1);
			}
			circle(drawing, Point2f(markerLocations[0], markerLocations[1]), 2, Scalar(30, 147, 56), 2); // green = marker0
			circle(drawing, Point2f(markerLocations[2], markerLocations[3]), 2, Scalar(255, 151, 0), 2); // blue = marker1
			circle(drawing, Point2f(0, 0), 2, Scalar(0, 0, 255), 10); // red = top left
			circle(drawing, Point2f(400, 0), 2, Scalar(0, 255, 255), 10); // yellow = top right
			circle(drawing, Point2f(0, 200), 2, Scalar(255, 0, 255), 10); // pink = bottom left
			circle(drawing, Point2f(400, 200), 2, Scalar(155, 155, 155), 10); // grey = bottom right
			imshow("Result " + to_string(camera), drawing);
		}

		//Save positions for next frame
		prevData[camera][0] = newData[0];
		prevData[camera][1] = newData[1];
		delete [] newData;
	}

}