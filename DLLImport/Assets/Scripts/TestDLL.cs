using UnityEngine;
using System.Runtime.InteropServices;

public class TestDLL : MonoBehaviour
{
    // The imported functions
    [DllImport("TestDLL", EntryPoint = "TestSort")]
    public static extern void TestSort(int[] a, int length);

    [DllImport("TestDLL", EntryPoint = "ShowImage")]
    public static extern int ShowImage();

    [DllImport("TestDLL", EntryPoint = "processImage")]
    public static extern void processImage(Color32[] raw, int width, int height);

    private int[] arrayOfInts = new int[] { 97, 92, 81, 60, 1, 104, 208, 56, 7, 1005 };

    private WebCamTexture webcam;
    private Texture2D processedWebcam;

    void Start()
    {
        Debug.Log(IntArrayToString(arrayOfInts, ";"));
        TestSort(arrayOfInts, arrayOfInts.Length);
        Debug.Log(IntArrayToString(arrayOfInts, ";"));

        //Debug.Log(ShowImage());

        webcam = new WebCamTexture();
        webcam.Play();

        Debug.Log("width: " + webcam.width + ", height: " + webcam.height);
        processedWebcam = new Texture2D(webcam.width, webcam.height);
        GameObject.Find("DisplayCamera").GetComponentInChildren<MeshRenderer>().material.mainTexture = processedWebcam;
    }

    void Update()
    {
        if (webcam.isPlaying)
        {
            Color32[] rawImg = webcam.GetPixels32();
            processImage(rawImg, webcam.width, webcam.height);
            processedWebcam.SetPixels32(rawImg);
            processedWebcam.Apply();
        }
    }

    private string IntArrayToString(int[] array, string delimiter)
    {
        string result = "";

        for (int i = 0; i < array.Length; i++)
        {
            result += array[i];
            if (i != array.Length - 1)
            {
                result += delimiter;
            }
        }

        return result;
    }

}