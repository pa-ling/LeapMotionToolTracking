
#include <opencv2/opencv.hpp>

using namespace std;
using namespace cv;

int main(int argc, char** argv)
{
	Point2f point1(1, 2);
	Point2f point2(3, 3);
	Point2f point3(0, 1);
	Point2f point4 = point1 + point2 - point3;
	cout << point4;
	getchar();
}