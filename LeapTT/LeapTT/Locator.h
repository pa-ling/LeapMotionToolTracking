#pragma once

#include <opencv2/opencv.hpp>
#include "Marker.h"

class Locator
{
	static const int POSITION_MODIFIER = 70;
	static const int RADIUS_MODIFIER = 30;
	static float getEuclidianDistance(cv::Point2f a, cv::Point2f b);
public:
	//Locator();
	//~Locator();
	static Marker* findMarkers(std::vector<cv::Point2f> center, std::vector<float> radius, Marker prevData[]);
	static int Locator::findMostLikelyPoint(cv::Point2f point, float radius, std::vector<cv::Point2f> positions, std::vector<float> radiuses);
};
