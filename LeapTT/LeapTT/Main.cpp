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
		LOG(INFO) << "CropImage from (" <<  width << "," << height << ") to (" << startX << "," << startY << "," << cropWidth << "," << cropHeight << ")";

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
	void __declspec(dllexport) GetMarkerLocations(unsigned char* img0, unsigned char* img1, int markerLocations[], int width, int height)
	{
		Mat leftImg(height, width, CV_8UC1, img0);

		//Get brightest point in the picture = first marker
		double minVal; double maxVal; Point minLoc; Point maxLoc;
		minMaxLoc(leftImg, &minVal, &maxVal, &minLoc, &maxLoc, Mat());
		threshold(leftImg, leftImg, maxVal-20, 255, THRESH_BINARY);

		//Create circular mask around the first marker
		Mat mask = Mat::zeros(height, width, CV_8UC1);
		circle(mask, maxLoc, maskRadius, Scalar(255, 255, 255), -1);

		Mat maskedImage = Mat::zeros(height, width, CV_8UC1);
		leftImg.copyTo(maskedImage, mask); //input and output must not be the same!

		//circle(leftImg, maxLoc, maskRadius, Scalar(255, 255, 255), 1);

		imshow("Masked Image", maskedImage);
	}
}