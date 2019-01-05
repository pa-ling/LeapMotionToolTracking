// Test.cpp : Defines the entry point for the console application.

#include "..\TestDLL\TestDLLSort.h"
#include <stdio.h>
#include <iostream>
#include "opencv2/core/core.hpp"
#include "opencv2/calib3d/calib3d.hpp"
#include <opencv2/highgui/highgui.hpp>

using namespace cv;
using namespace std;

int main()
{
	Mat leftimg = imread("D:\\Tmp\\scene_l.jpg", CV_8UC1);
	Mat rightimg = imread("D:\\Tmp\\scene_r.jpg", CV_8UC1);

	cout << "left width: " << leftimg.rows << ", left height: " << leftimg.cols << endl;
	cout << "right width: " << rightimg.rows << ", right height: " << rightimg.cols << endl;

	Mat disp, disp8;

	Ptr<StereoBM> sbm = StereoBM::create(16, 15);
	sbm->compute(leftimg, rightimg, disp);
	normalize(disp, disp8, 0, 255, NORM_MINMAX , CV_8U);

	imshow("left", leftimg);
	imshow("right", rightimg);
	imshow("disp", disp8);
	waitKey(0);
    return 0;
}

