using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;

public class TestDLL : MonoBehaviour
{
    [DllImport("TestDLL", EntryPoint = "SortIntArray")]
    public static extern void TestSort(int[] a, int length);

    [DllImport("TestDLL", EntryPoint = "ShowImage")]
    public static extern void ShowImage(string path);

    [DllImport("TestDLL", EntryPoint = "ProcessImageData")]
    public static extern void ProcessImageData(Color32[] raw, Color32[] processed, int width, int height);

    [DllImport("TestDLL", EntryPoint = "GetLeapDimensions")]
    public static extern void GetLeapDimensions(int[] dim);

    [DllImport("TestDLL", EntryPoint = "GetLeapImage")]
    public static extern void GetLeapImage(Color32[] image, int index);

    private WebCamTexture webcam;
    private Texture2D processedWebcam; 

    void Start()
    {
        //#1
        /*int[] arrayOfInts = new int[] { 97, 92, 81, 60, 1, 104, 208, 56, 7, 1005 };
        Debug.Log(IntArrayToString(arrayOfInts, ";"));
        TestSort(arrayOfInts, arrayOfInts.Length);
        Debug.Log(IntArrayToString(arrayOfInts, ";"));*/

        //#2
        /*ShowImage("D:\\Development\\Git\\LeapMotionToolTracking\\TestDLL\\Test\\test_picture.png");
        Debug.Log("Done");*/

        //#3
        /*webcam = new WebCamTexture();
        webcam.Play();
        Debug.Log("width: " + webcam.width + ", height: " + webcam.height);
        processedWebcam = new Texture2D(webcam.width, webcam.height);
        GameObject.Find("DisplayCamera").GetComponentInChildren<MeshRenderer>().material.mainTexture = processedWebcam;*/

        //#4
        int[] dim = new int[2];
        GetLeapDimensions(dim);
        Debug.Log("width: " + dim[0] + ", height: " + dim[1]);

        //#5
        /*processedWebcam = new Texture2D(640, 240);
        GameObject.Find("DisplayCamera").GetComponentInChildren<MeshRenderer>().material.mainTexture = processedWebcam;
        Debug.Log("Create Image Buffer");
        Color32[] processedImg = new Color32[640 * 240];
        Debug.Log("Get Image from DLL");
        GetLeapImage(processedImg, 0);
        Debug.Log("Apply Image to texture");
        processedWebcam.SetPixels32(processedImg);
        processedWebcam.Apply();*/
    }

    void Update()
    {
        //#3
        /*if (webcam.isPlaying)
        {
            Color32[] rawImg = webcam.GetPixels32();
            Color32[] processedImg = new Color32[rawImg.Length];
            ProcessImageData(rawImg, processedImg, webcam.width, webcam.height);
            processedWebcam.SetPixels32(processedImg);
            processedWebcam.Apply();
        }*/

        //#5
        /*Debug.Log("Create Image Buffer");
        Color32[] processedImg = new Color32[640 * 240];
        Debug.Log("Get Image from DLL");
        //GetLeapImage(processedImg, 0);
        Debug.Log("Apply Image to texture");
        processedWebcam.SetPixels32(processedImg);
        processedWebcam.Apply();
        StartCoroutine(Example());*/
    }

    IEnumerator Example()
    {
        yield return new WaitForSeconds(2);
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