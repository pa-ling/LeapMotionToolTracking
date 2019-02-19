
#include <opencv2/opencv.hpp>

using namespace std;
using namespace cv;

int main(int argc, char** argv)
{
	Point2f point1(1, 2);
	Point2f point2(3, 3);
	Point2f point3(0, 1);
	Point2f point4 = point1 + point2 - point3;
	vector<Point2f> positions;
	positions.push_back(point1);
	positions.push_back(point2);
	positions.push_back(point3);
	positions.push_back(point4);
	cout << positions << endl;
	cout << "erase element" << endl;
	int closestPointIndex0 = 1;
	positions.erase(positions.begin() + closestPointIndex0);
	cout << positions << endl;

	getchar();
}