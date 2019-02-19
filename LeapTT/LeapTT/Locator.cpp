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
		Point2f point0 = Point2f(prevData[0].getX(), prevData[0].getY());
		Point2f point1 = Point2f(prevData[1].getX(), prevData[1].getY());
		float dist0 = getEuclidianDistance(point0, position);
		float dist1 = getEuclidianDistance(point1, position);
		if (dist0 < dist1) {
			newData[0] = Marker(position.x, position.y, radiuses[0]);
			newData[1] = Marker(point1 + position - point0, prevData[1].getR());
		}
		else {
			newData[0] = Marker(point0 + position - point1, prevData[0].getR());
			newData[1] = Marker(position.x, position.y, radiuses[0]);
		}
	}
	else
	if (2 <= positions.size())
	{
		Point2f prevPos0 = Point2f(prevData[0].getX(), prevData[0].getY());
		Point2f prevPos1 = Point2f(prevData[1].getX(), prevData[1].getY());

		int closestPointIndex0 = findMostLikelyPoint(prevPos0, prevData[0].getR(), positions, radiuses);
		int closestPointIndex1 = findMostLikelyPoint(prevPos1, prevData[1].getR(), positions, radiuses);

		if (closestPointIndex0 != closestPointIndex1)
		{
			newData[0] = Marker(positions[closestPointIndex0].x, positions[closestPointIndex0].y, radiuses[closestPointIndex0]);
			newData[1] = Marker(positions[closestPointIndex1].x, positions[closestPointIndex1].y, radiuses[closestPointIndex1]);
		}
		else //closestPointIndex0 == closestPointIndex1
		{
			float dist0 = getEuclidianDistance(prevPos0, positions[closestPointIndex0]);
			float dist1 = getEuclidianDistance(prevPos1, positions[closestPointIndex1]);
			if (dist0 < dist1)
			{
				newData[0] = Marker(positions[closestPointIndex0].x, positions[closestPointIndex0].y, radiuses[closestPointIndex0]);

				positions.erase(positions.begin() + closestPointIndex0);
				closestPointIndex1 = findMostLikelyPoint(prevPos1, prevData[1].getR(), positions, radiuses);
				newData[1] = Marker(positions[closestPointIndex1].x, positions[closestPointIndex1].y, radiuses[closestPointIndex1]);
			}
			else
			{
				newData[1] = Marker(positions[closestPointIndex1].x, positions[closestPointIndex1].y, radiuses[closestPointIndex1]);

				positions.erase(positions.begin() + closestPointIndex0);
				closestPointIndex0 = findMostLikelyPoint(prevPos0, prevData[0].getR(), positions, radiuses);
				newData[0] = Marker(positions[closestPointIndex0].x, positions[closestPointIndex0].y, radiuses[closestPointIndex0]);
			}
		}
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