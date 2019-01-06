// Test.cpp : Defines the entry point for the console application.

#include "..\TestDLL\TestDLLSort.h"
#include <stdio.h>
#include <iostream>
#include "opencv2/core/core.hpp"
#include "opencv2/calib3d/calib3d.hpp"
#include <opencv2/highgui/highgui.hpp>

using namespace cv;
using namespace std;

void sbm()
{
	Mat leftimg = imread("D:\\Tmp\\scene_l.jpg", CV_8UC1);
	Mat rightimg = imread("D:\\Tmp\\scene_r.jpg", CV_8UC1);

	Mat disp, disp8;
	Ptr<StereoBM> sbm = StereoBM::create();
	sbm->setNumDisparities(16);
	sbm->setBlockSize(35);
	sbm->setTextureThreshold(10);
	sbm->setSpeckleWindowSize(100);
	sbm->setSpeckleRange(32);
	sbm->setMinDisparity(0);
	sbm->setUniquenessRatio(0);
	sbm->compute(leftimg, rightimg, disp);
	normalize(disp, disp8, 0, 255, NORM_MINMAX, CV_8U);

	imshow("left", leftimg);
	imshow("right", rightimg);
	imshow("disp", disp8);
	waitKey(0);
}

void sgbm()
{
	Mat leftimg = imread("D:\\Tmp\\scene_l.jpg", CV_8UC1);
	Mat rightimg = imread("D:\\Tmp\\scene_r.jpg", CV_8UC1);

	int numberOfDisparities = 16;
	int sgbmWinSize = 11;

	Ptr<StereoSGBM> sgbm = StereoSGBM::create();
	sgbm->setPreFilterCap(63);
	sgbm->setBlockSize(sgbmWinSize);
	sgbm->setP1(8 * sgbmWinSize * sgbmWinSize);
	sgbm->setP2(32 * sgbmWinSize * sgbmWinSize);
	sgbm->setMinDisparity(0);
	sgbm->setNumDisparities(numberOfDisparities);
	sgbm->setUniquenessRatio(10);
	sgbm->setSpeckleWindowSize(150);
	sgbm->setSpeckleRange(50);
	sgbm->setDisp12MaxDiff(1);
	sgbm->setMode(StereoSGBM::MODE_HH);

	Mat disp, disp8;
	sgbm->compute(leftimg, rightimg, disp);
	normalize(disp, disp8, 0, 255, NORM_MINMAX, CV_8U);

	imshow("left", leftimg);
	imshow("right", rightimg);
	imshow("disp", disp8);
	waitKey(0);
}

int main()
{
	Mat image = imread("D:\\Tmp\\scene_l.jpg", CV_8UC1);

	Mat croppedImage(image, Rect(60, 100, 260, 140));
	imshow("Cropped", croppedImage);
	waitKey(0);
    return 0;
}
