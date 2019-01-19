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

		TEST_METHOD(TestProcessImageData)
		{
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