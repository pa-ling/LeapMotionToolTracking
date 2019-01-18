extern "C" {
#include "Main.h"
}
#include <opencv2/opencv.hpp>
#include "easylogging++.h"

INITIALIZE_EASYLOGGINGPP

using namespace cv;
using namespace std;

extern "C" {

	int maskRadius = 20;

	/* This function tests data transport between DLL and caller */
	void __declspec(dllexport) SortIntArray(int a[], int length)
	{
		sort(a, a + length);
	}

	/* This function tests the availability of opencv */
	void __declspec(dllexport) ShowImage(char* path)
	{
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

	void __declspec(dllexport) CropImage(unsigned char* imgData, unsigned char* croppedImgData, int width, int height, int startX, int startY, int cropWidth, int cropHeight)
	{
		Mat image(height, width, CV_8UC1, imgData);
		Mat ROI(image, Rect(startX, startY, cropWidth, cropHeight));

		Mat croppedImage;
		ROI.copyTo(croppedImage);

		memcpy(croppedImgData, croppedImage.data, croppedImage.total() * croppedImage.elemSize());
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

		int sgbmWinSize = 15;

		Ptr<StereoBM> sbm = StereoBM::create();
		sbm->setNumDisparities(16);
		sbm->setBlockSize(15);
		/*sbm->setTextureThreshold(10);
		sbm->setSpeckleWindowSize(100);
		sbm->setSpeckleRange(32);
		sbm->setMinDisparity(0);
		sbm->setUniquenessRatio(0);*/

		Mat disp, disp8;
		sbm->compute(leftImg, rightImg, disp);
		normalize(disp, disp8, 0, 255, NORM_MINMAX, CV_8UC1);
		//memcpy(depthMap, disp8.data, disp8.total() * disp8.elemSize());

		imshow("Image 1", leftImg);
		imshow("Image 2", rightImg);
		imshow("Disparity", disp8);
	}

	/* This function gets image data and processes it*/
	void __declspec(dllexport) GetMarkerLocations(unsigned char* imgData, int markerLocations[], int width, int height)
	{
		Mat img(height, width, CV_8UC1, imgData);

		// Get brightest point in the picture = first marker
		double minVal; double maxVal; Point minLoc; Point maxLoc;
		minMaxLoc(img, &minVal, &maxVal, &minLoc, &maxLoc, Mat());
		threshold(img, img, maxVal-15, 255, THRESH_BINARY);

		// Create circular mask around the first marker
		Mat mask = Mat::zeros(height, width, CV_8UC1);
		circle(mask, maxLoc, maskRadius, Scalar(255, 255, 255), -1);

		Mat maskedImg = Mat::zeros(height, width, CV_8UC1);
		img.copyTo(maskedImg, mask); // input and output must not be the same!

		// Get contours
		vector<vector<Point>> contours;
		vector<Vec4i> hierarchy;
		findContours(maskedImg, contours, hierarchy, RETR_TREE, CHAIN_APPROX_SIMPLE, Point(0, 0));

		// Approximate contours to polygons + get bounding rects and circles
		vector<Point2f>center(contours.size());
		vector<float>radius(contours.size());

		for (int i = 0; i < contours.size(); i++)
		{
			minEnclosingCircle((Mat)contours[i], center[i], radius[i]);
		}



		// Draw polygonal contour + bonding rects + circles
		Mat drawing = Mat::zeros(maskedImg.size(), CV_8UC3);
		if (contours.size() >= 2) {
			markerLocations[0] = center[0].x;
			markerLocations[1] = center[0].y;
			markerLocations[2] = center[1].x;
			markerLocations[3] = center[1].y;
			circle(drawing, center[0], 5, Scalar(0, 0, 255), 1);
			circle(drawing, center[1], 5, Scalar(0, 0, 255), 1);
		}

		//circle(leftImg, maxLoc, maskRadius, Scalar(255, 255, 255), 1);
		LOG(INFO) << "Conturs " << contours.size();

		imshow("Contours" , drawing);
		imshow("Masked Image", maskedImg);
	}
}