using UnityEngine;
using System.Runtime.InteropServices;

public class TestDLL : MonoBehaviour
{
    // The imported function
    [DllImport("TestDLL", EntryPoint = "TestSort")]
    public static extern void TestSort(int[] a, int length);

    public int[] arrayOfInts;

    void Start()
    {
        arrayOfInts = new int[5];
        arrayOfInts[0] = 10;
        arrayOfInts[1] = 74;
        arrayOfInts[2] = 1;
        arrayOfInts[3] = 245;
        arrayOfInts[4] = 7;
        Debug.Log(arrayOfInts[0]);
        TestSort(arrayOfInts, arrayOfInts.Length);
        Debug.Log(arrayOfInts[0]);
    }
}