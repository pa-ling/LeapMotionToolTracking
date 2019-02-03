extern "C" {
#include "Main.h"
}
#include <opencv2/opencv.hpp>
#include "easylogging++.h"
#include "Util.h"

INITIALIZE_EASYLOGGINGPP

using namespace cv;
using namespace std;

extern "C" {

	int MASK_RADIUS = 20;

	float prevPos[2][4] = { { -1, -1, -1, -1 }, { -1, -1, -1, -1 } };

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

	void __declspec(dllexport) GetMarkerLocations(unsigned char* imgData, float markerLocations[], int width, int height, int camera)
	{
		Mat img(height, width, CV_8UC1, imgData);
		imshow("Image", img);

		// Prefilter so that environment will not be recognized as marker when no marker is present
		threshold(img, img, 125, 255, THRESH_TOZERO);

		// Get brightest point in the picture = first marker
		double minVal; double maxVal; Point minLoc; Point maxLoc;
		minMaxLoc(img, &minVal, &maxVal, &minLoc, &maxLoc, Mat());
		threshold(img, img, maxVal - 25, 255, THRESH_BINARY);
		imshow("Adaptive Threshold Image", img);

		// Get contours
		vector<vector<Point>> contours;
		vector<Vec4i> hierarchy;
		findContours(img, contours, hierarchy, RETR_TREE, CHAIN_APPROX_SIMPLE, Point(0, 0));

		// Get bounding circles of contours
		vector<Point2f>center(contours.size());
		vector<float>radius(contours.size());
		Mat drawing = Mat::zeros(img.size(), CV_8UC3);
		for (int i = 0; i < contours.size(); i++) {
			minEnclosingCircle((Mat)contours[i], center[i], radius[i]);
			Scalar color = Scalar(0, 0, 255);
			circle(drawing, center[i], (int) radius[i], color, 1);
		}

		LOG(INFO) << camera << "(" << prevPos[camera][0] << ", " << prevPos[camera][1] << ", " << prevPos[camera][2] << ", " << prevPos[camera][3] << ")";
		LOG(INFO) << camera << "*: " << center;

		Util::findMarkers(center, radius, markerLocations, camera, prevPos);
		circle(drawing, Point2f(markerLocations[0], markerLocations[1]), 2, Scalar(0, 255, 0), 2);
		circle(drawing, Point2f(markerLocations[2], markerLocations[3]), 2, Scalar(0, 255, 0), 2);

		//Save positions for next frame
		for (int i = 0; i < 4; i++) {
			prevPos[camera][i] = markerLocations[i];
		}

		imshow("Result", drawing);
	}

}