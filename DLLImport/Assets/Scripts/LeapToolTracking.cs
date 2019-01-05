using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Leap.Unity;

public class LeapToolTracking : LeapImageRetriever {

    private Texture2D processedWebcam0, processedWebcam1;

    private void Start()
    {
        Debug.Log("LeapToolTracking initalized.");
        processedWebcam0 = new Texture2D(640, 240);
        GameObject.Find("DisplayCamera0").GetComponentInChildren<MeshRenderer>().material.mainTexture = processedWebcam0;
        processedWebcam1 = new Texture2D(640, 240);
        GameObject.Find("DisplayCamera1").GetComponentInChildren<MeshRenderer>().material.mainTexture = processedWebcam1;
    }

    [DllImport("TestDLL", EntryPoint = "GetLeapImages")]
    public static extern void GetLeapImages(byte[] raw, byte[] img0, byte[] img1, int size);

    [DllImport("TestDLL", EntryPoint = "GetDepthMap")]
    public static extern void GetDepthMap(byte[] img0, byte[] img1, byte[] disp, int width, int height);

    private void OnPreRender()
    {
        if (_currentImage != null)
        {
            int imageSize = _currentImage.Width * _currentImage.Height;
            byte[] raw = _currentImage.Data(Leap.Image.CameraType.LEFT);
            byte[] leftImgData = new byte[imageSize];
            byte[] rightImgData = new byte[imageSize];
            byte[] depthMap = new byte[imageSize];

            GetLeapImages(raw, leftImgData, rightImgData, imageSize);

            GetDepthMap(leftImgData, rightImgData, depthMap, _currentImage.Width, _currentImage.Height);
            //Debug.Log("Width: " + _currentImage.Width + ", Height: " + _currentImage.Height);

            Color32[] leftImg = new Color32[imageSize];
            Color32[] rightImg = new Color32[imageSize];
            for (int i = 0; i < _currentImage.Width; i++)
            {
                for (int j = 0; j < _currentImage.Height; j++)
                {
                    int index = j * _currentImage.Width + i;
                    leftImg[index] = new Color32(leftImgData[index], leftImgData[index], leftImgData[index], 1);
                    rightImg[index] = new Color32(rightImgData[index], rightImgData[index], rightImgData[index], 1);
                }
            }

            processedWebcam0.SetPixels32(leftImg);
            processedWebcam0.Apply();
            processedWebcam1.SetPixels32(rightImg);
            processedWebcam1.Apply();
        }
    }
}
