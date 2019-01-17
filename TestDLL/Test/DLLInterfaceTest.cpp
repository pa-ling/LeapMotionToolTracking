#include "stdafx.h"
#include "CppUnitTest.h"
#include "..\LeapTT\Main.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

/* NOTE: TEST_METHODS are not allowed to have the same name as any tested method. */
namespace Test
{
	TEST_CLASS(DLLInterfaceTest)
	{
	public:
		wchar_t message[200];

		TEST_METHOD(TestSortIntArray)
		{
			int length = 5;
			int actual[] = { 16, 2, 77, 40, 12071 };
			int expected[] = { 2, 16, 40, 77, 12071 };

			SortIntArray(actual, length);

			for (int i = 0; i < length; i++) {
				_swprintf(message, L"Values do not match at index %d", i);
				Assert::AreEqual(expected[i], actual[i], message);
			}
		}

		TEST_METHOD(TestShowImage)
		{
			char path[] = "..\\..\\Test\\test_picture.png";
			ShowImage(path);
		}

		TEST_METHOD(TestProcessImageData) {
			_swprintf(message, L"Test not implemented yet");
			Assert::Fail(message);
		}

		TEST_METHOD(TestLeapImages)
		{
			//getLeapImages();
			_swprintf(message, L"Test not implemented yet");
			Assert::Fail(message);
		}
		

	};
}