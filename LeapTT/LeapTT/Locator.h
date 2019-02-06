#pragma once

#include <opencv2/opencv.hpp>
#include "Marker.h"

using namespace std;
using namespace cv;

class Locator
{
	static const int POSITION_MODIFIER = 50;
	static const int RADIUS_MODIFIER = 50;
public:
	//Locator();
	//~Locator();
	static Marker* findMarkers(vector<Point2f> center, vector<float> radius, Marker prevData[]);
	static int Locator::findMostLikelyPoint(Point2f point, float radius, vector<Point2f> positions, vector<float> radiuses);
};
