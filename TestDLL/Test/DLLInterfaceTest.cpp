#include "stdafx.h"
#include "CppUnitTest.h"
#include "..\TestDLL\TestDLLSort.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

/* NOTE: TEST_METHODS are not allowed to have the same name as any tested method. */
namespace Test
{
	TEST_CLASS(DLLInterfaceTest)
	{
	public:

		TEST_METHOD(TestSort)
		{

		}

		TEST_METHOD(TestShowImage2)
		{
			ShowImage();
		}

		TEST_METHOD(TestLeapImages) {
			//getLeapImages();
		}

	};
}