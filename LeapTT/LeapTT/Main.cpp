extern "C" {
#include "Main.h"
}
#include <opencv2/opencv.hpp>
#include "easylogging++.h"

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

	int findClosestPointInVector(Point2f point, vector<Point2f> candidates) {
		float minDist = 0;
		int nearestPointIndex = 0;
		for (int i = 0; i < candidates.size(); i++) {
			Point2f candidate = candidates[i];
			float euclidianDist = sqrt(pow(point.x - candidate.x, 2) + pow(point.y - candidate.y, 2));
			if (euclidianDist < minDist) {
				minDist = euclidianDist;
				nearestPointIndex = i;
			}
		}
		return nearestPointIndex;
	}

	void findMarkers(vector<Point2f> center, vector<float> radius, float markerLocations[], int camera)
	{
		// Return previous positions if no marker was found
		if (0 == center.size()) {
			for (int i = 0; i < 4; i++) {
				markerLocations[i] = prevPos[camera][i];
			}
			return;
		}

		// Match the given point to previous position and return one previous position additionally
		if (1 == center.size()) {
			Point2f candidate = center[0];
			float euclidianDist0 = sqrt(pow(prevPos[camera][0] - candidate.x, 2) + pow(prevPos[camera][1] - candidate.y, 2));
			float euclidianDist1 = sqrt(pow(prevPos[camera][2] - candidate.x, 2) + pow(prevPos[camera][3] - candidate.y, 2));
			if (euclidianDist0 < euclidianDist1) {
				markerLocations[0] = center[0].x;
				markerLocations[1] = center[0].y;
				markerLocations[2] = prevPos[camera][2];
				markerLocations[3] = prevPos[camera][3];
			}
			else {
				markerLocations[0] = prevPos[camera][0];
				markerLocations[1] = prevPos[camera][1];
				markerLocations[2] = center[0].x;
				markerLocations[3] = center[0].y;
			}
			// TODO: Compare radius too
		}

		if (2 == center.size()) {
			markerLocations[0] = center[0].x;
			markerLocations[1] = center[0].y;
			markerLocations[2] = center[1].x;
			markerLocations[3] = center[1].y;
			return;
		}

		if (2 < center.size()) {
			int closestPoint0 = findClosestPointInVector(Point2f(prevPos[camera][0], prevPos[camera][1]), center);
			int closestPoint1 = findClosestPointInVector(Point2f(prevPos[camera][2], prevPos[camera][3]), center);
			markerLocations[0] = center[closestPoint0].x;
			markerLocations[1] = center[closestPoint0].y;
			markerLocations[2] = center[closestPoint1].x;
			markerLocations[3] = center[closestPoint1].y;
			// TODO: Compare radius too
			// TODO: What if both points are the same?
		}
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
			circle(drawing, center[i], radius[i], color, 1);
		}

		LOG(INFO) << camera << "(" << prevPos[camera][0] << ", " << prevPos[camera][1] << ", " << prevPos[camera][2] << ", " << prevPos[camera][3] << ")";
		LOG(INFO) << camera << "*: " << center;

		findMarkers(center, radius, markerLocations, camera);
		circle(drawing, Point2f(markerLocations[0], markerLocations[1]), 2, Scalar(0, 255, 0), 2);
		circle(drawing, Point2f(markerLocations[2], markerLocations[3]), 2, Scalar(0, 255, 0), 2);

		//Save positions for next frame
		for (int i = 0; i < 4; i++) {
			prevPos[camera][i] = markerLocations[i];
		}

		imshow("Result", drawing);
	}

}