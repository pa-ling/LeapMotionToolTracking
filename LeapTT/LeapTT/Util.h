#pragma once

#include <opencv2/opencv.hpp>
#include "Marker.h"

using namespace std;
using namespace cv;

class Util
{
public:
	//Util();
	//~Util();
	static Marker* findMarkers(vector<Point2f> center, vector<float> radius, Marker prevData[]);
	static int findMostLikelyPoint(Point2f point, vector<Point2f> candidates, vector<float> radiuses);
};
