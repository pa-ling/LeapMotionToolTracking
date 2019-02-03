#include "Util.h"

/*Util::Util()
{
}*/

/*Util::~Util()
{
}*/

void Util::findMarkers(vector<Point2f> center, vector<float> radius, float markerLocations[], int camera, float prevPos[][4])
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
		// TODO: Instead of just delivering the old value, add the difference from existing new value to its old value to the other old value
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

int Util::findClosestPointInVector(Point2f point, vector<Point2f> candidates) {
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