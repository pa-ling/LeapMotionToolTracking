#include "stdafx.h"
#include "CppUnitTest.h"
#include <opencv2/opencv.hpp>
#include "..\LeapTT\Main.h"
#include "..\LeapTT\Util.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

using namespace std;
using namespace cv;

/* NOTE: TEST_METHODS are not allowed to have the same name as any tested method. */
namespace Test
{
	TEST_CLASS(DLLInterfaceTest)
	{
	public:
		wchar_t message[200];

		TEST_METHOD(TestFindClosestPointInVector)
		{
			Point2f point(0, 0);
			vector<Point2f> candidates;
			candidates.push_back(Point2f(0, 0));
			candidates.push_back(Point2f(1, 5));
			candidates.push_back(Point2f(7, 3));
			candidates.push_back(Point2f(10, 1));
			candidates.push_back(Point2f(1, 1));

			int index = Util::findMostLikelyPoint(point, candidates);
			//_swprintf(message, L"" + index);
			Assert::Fail(message);
		}	

	};
}