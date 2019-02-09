#include "Locator.h"

using namespace std;
using namespace cv;

/*Locator::Locator()
{
}*/

/*Locator::~Locator()
{
}*/

Marker* Locator::findMarkers(vector<Point2f> positions, vector<float> radiuses, Marker prevData[])
{
	Marker* newData = new Marker[2];

	if (0 == positions.size())
	{
		// Return previous positions if no marker was found
		newData[0] = Marker(prevData[0]);
		newData[1] = Marker(prevData[1]);
	} 
	else
	if (1 == positions.size())
	{
		// Match the given point to previous position and return one previous position additionally
		Point2f position = positions[0];
		float dist0 = getEuclidianDistance(Point2f(prevData[0].getX(), prevData[0].getY()), position);
		float dist1 = getEuclidianDistance(Point2f(prevData[1].getX(), prevData[1].getY()), position);
		if (dist0 < dist1) {
			newData[0] = Marker(position.x, position.y, radiuses[0]);
			newData[1] = Marker(prevData[1]);
		}
		else {
			newData[0] = Marker(prevData[0]);
			newData[1] = Marker(position.x, position.y, radiuses[0]);
		}
		// TODO: Instead of just delivering the old value, add the difference from existing new value to its old value to the other old value
	}
	else
	if (2 == positions.size())
	{
		newData[0] = Marker(positions[0].x, positions[0].y, radiuses[0]);
		newData[1] = Marker(positions[1].x, positions[1].y, radiuses[1]);
	}
	else
	if (2 < positions.size())
	{
		int closestPoint0 = findMostLikelyPoint(Point2f(prevData[0].getX(), prevData[0].getY()), prevData[0].getR(), positions, radiuses);
		int closestPoint1 = findMostLikelyPoint(Point2f(prevData[1].getX(), prevData[1].getY()), prevData[1].getR(), positions, radiuses);
		newData[0] = Marker(positions[closestPoint0].x, positions[closestPoint0].y, radiuses[closestPoint0]);
		newData[1] = Marker(positions[closestPoint1].x, positions[closestPoint1].y, radiuses[closestPoint1]);
		// TODO: What if both points are the same?
	}

	return newData;
}

int Locator::findMostLikelyPoint(Point2f point, float radius, vector<Point2f> positions, vector<float> radiuses) {
	float minScore = numeric_limits<float>::max();
	int mostLikelyPointIndex = 0;
	for (int i = 0; i < positions.size(); i++) {
		Point2f position = positions[i];
		float dist = getEuclidianDistance(point, position);
		float radiusDiff = abs(radiuses[i] - radius);
		float score = dist * POSITION_MODIFIER + radiusDiff * RADIUS_MODIFIER;
		if (score < minScore) {
			minScore = score;
			mostLikelyPointIndex = i;
		}
	}
	return mostLikelyPointIndex;
}

float Locator::getEuclidianDistance(Point2f a, Point2f b)
{
	return sqrt(pow(a.x - b.x, 2) + pow(a.y - b.y, 2));
}