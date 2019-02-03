#pragma once

#include <opencv2/opencv.hpp>

using namespace std;
using namespace cv;

class Util
{
public:
	//Util();
	//~Util();
	static int findMostLikelyPoint(Point2f point, vector<Point2f> candidates, vector<float> radiuses);
	static void findMarkers(vector<Point2f> center, vector<float> radius, float markerLocations[], int camera, float prevPos[][4]);
};
