#include "Util.h"

/*Util::Util()
{
}*/

/*Util::~Util()
{
}*/

void Util::findMarkers(vector<Point2f> positions, vector<float> radiuses, float markerLocations[], int camera, float prevPos[][4])
{
	// Return previous positions if no marker was found
	if (0 == positions.size()) {
		for (int i = 0; i < 4; i++) {
			markerLocations[i] = prevPos[camera][i];
		}
		return;
	}

	// Match the given point to previous position and return one previous position additionally
	if (1 == positions.size()) {
		Point2f candidate = positions[0];
		float euclidianDist0 = sqrt(pow(prevPos[camera][0] - candidate.x, 2) + pow(prevPos[camera][1] - candidate.y, 2));
		float euclidianDist1 = sqrt(pow(prevPos[camera][2] - candidate.x, 2) + pow(prevPos[camera][3] - candidate.y, 2));
		if (euclidianDist0 < euclidianDist1) {
			markerLocations[0] = positions[0].x;
			markerLocations[1] = positions[0].y;
			markerLocations[2] = prevPos[camera][2];
			markerLocations[3] = prevPos[camera][3];
		}
		else {
			markerLocations[0] = prevPos[camera][0];
			markerLocations[1] = prevPos[camera][1];
			markerLocations[2] = positions[0].x;
			markerLocations[3] = positions[0].y;
		}
		// TODO: Instead of just delivering the old value, add the difference from existing new value to its old value to the other old value
	}

	if (2 == positions.size()) {
		markerLocations[0] = positions[0].x;
		markerLocations[1] = positions[0].y;
		markerLocations[2] = positions[1].x;
		markerLocations[3] = positions[1].y;
		return;
	}

	if (2 < positions.size()) {
		int closestPoint0 = findMostLikelyPoint(Point2f(prevPos[camera][0], prevPos[camera][1]), positions, radiuses);
		int closestPoint1 = findMostLikelyPoint(Point2f(prevPos[camera][2], prevPos[camera][3]), positions, radiuses);
		markerLocations[0] = positions[closestPoint0].x;
		markerLocations[1] = positions[closestPoint0].y;
		markerLocations[2] = positions[closestPoint1].x;
		markerLocations[3] = positions[closestPoint1].y;
		// TODO: Compare radius too
		// TODO: What if both points are the same?
	}
}

int Util::findMostLikelyPoint(Point2f point, vector<Point2f> positions, vector<float> radiuses) {
	float minDist = 0;
	int nearestPointIndex = 0;
	for (int i = 0; i < positions.size(); i++) {
		Point2f position = positions[i];
		float euclidianDist = sqrt(pow(point.x - position.x, 2) + pow(point.y - position.y, 2));
		if (euclidianDist < minDist) {
			minDist = euclidianDist;
			nearestPointIndex = i;
		}
	}
	return nearestPointIndex;
}