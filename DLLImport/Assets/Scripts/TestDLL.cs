using UnityEngine;
using System.Runtime.InteropServices;

public class TestDLL : MonoBehaviour
{
    // The imported function
    [DllImport("TestDLL", EntryPoint = "TestSort")]
    public static extern void TestSort(int[] a, int length);

    private int[] arrayOfInts = new int[] { 97, 92, 81, 60, 1, 104, 208, 56, 7, 1005 };

    void Start()
    {
        Debug.Log(ArrayToString(arrayOfInts, ";"));
        TestSort(arrayOfInts, arrayOfInts.Length);
        Debug.Log(ArrayToString(arrayOfInts, ";"));
    }

    private string ArrayToString(int[] array, string delimiter)
    {
        string result = "";

        for (int i = 0; i < array.Length; i++)
        {
            result += array[i];
            if (i != array.Length - 1)
            {
                result += ";";
            }
        }

        return result;
    }
}