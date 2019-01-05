using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Leap.Unity;

public class LeapToolTracking : LeapImageRetriever {

    private int TEX_WIDTH = 400;
    private int TEX_HEIGHT = 400;
    private int MAX_FOV = 8;

    private Texture2D processedWebcam0, processedWebcam1;

    private void Start()
    {
        Debug.Log("LeapToolTracking initalized.");
        processedWebcam0 = new Texture2D(TEX_WIDTH, TEX_HEIGHT);
        GameObject.Find("DisplayCamera0").GetComponentInChildren<MeshRenderer>().material.mainTexture = processedWebcam0;
        processedWebcam1 = new Texture2D(TEX_WIDTH, TEX_HEIGHT);
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
            //Debug.Log("left offset: " + _currentImage.ByteOffset(Leap.Image.CameraType.LEFT));
            //Debug.Log("right offset: " + _currentImage.ByteOffset(Leap.Image.CameraType.RIGHT));
            int imageSize = _currentImage.Width * _currentImage.Height;
            byte[] raw = _currentImage.Data(Leap.Image.CameraType.LEFT);
            byte[] leftImgData = new byte[imageSize];
            byte[] rightImgData = new byte[imageSize];
            byte[] depthMap = new byte[imageSize];

            GetLeapImages(raw, leftImgData, rightImgData, imageSize);

            //GetDepthMap(leftImgData, rightImgData, depthMap, _currentImage.Width, _currentImage.Height);
            //Debug.Log("Width: " + _currentImage.Width + ", Height: " + _currentImage.Height);

            Color32[] undistortedLeftImg = new Color32[TEX_WIDTH * TEX_HEIGHT];
            Color32[] undistortedRightImg = new Color32[TEX_WIDTH * TEX_HEIGHT];
            for (float row = 0; row < TEX_HEIGHT; row++)
            {
                for (float col = 0; col < TEX_WIDTH; col++)
                {
                    //Normalize from pixel xy to range [0..1]
                    Leap.Vector input = new Leap.Vector();
                    input.x = col / TEX_WIDTH;
                    input.y = row / TEX_HEIGHT;

                    //Convert from normalized [0..1] to ray slopes
                    input.x = (input.x - (float) .5) * MAX_FOV;
                    input.y = (input.y - (float) .5) * MAX_FOV;

                    int dindex = (int)Mathf.Floor(row * TEX_WIDTH + col);

                    Leap.Vector pixelLeft = _currentImage.RectilinearToPixel(Leap.Image.CameraType.LEFT, input);
                    int pindexLeft = (int) Mathf.Floor(pixelLeft.y) * _currentImage.Width + (int) Mathf.Floor(pixelLeft.x);

                    if (pixelLeft.x >= 0 && pixelLeft.x < _currentImage.Width && pixelLeft.y >= 0 && pixelLeft.y < _currentImage.Height)
                    {
                        undistortedLeftImg[dindex] = new Color32(leftImgData[pindexLeft], leftImgData[pindexLeft], leftImgData[pindexLeft], 1);
                    }
                    else
                    {
                        undistortedLeftImg[dindex] = new Color32(128, 128, 128, 1);
                    }

                    Leap.Vector pixelRight = _currentImage.RectilinearToPixel(Leap.Image.CameraType.RIGHT, input);
                    int pindexRight = (int)Mathf.Floor(pixelRight.y) * _currentImage.Width + (int)Mathf.Floor(pixelRight.x);

                    if (pixelRight.x >= 0 && pixelRight.x < _currentImage.Width && pixelRight.y >= 0 && pixelRight.y < _currentImage.Height)
                    {
                        undistortedRightImg[dindex] = new Color32(rightImgData[pindexRight], rightImgData[pindexRight], rightImgData[pindexRight], 1);
                    }
                    else
                    {
                        undistortedRightImg[dindex] = new Color32(128, 128, 128, 1);
                    }
                }
            }

            processedWebcam0.SetPixels32(undistortedLeftImg);
            processedWebcam0.Apply();
            processedWebcam1.SetPixels32(undistortedRightImg);
            processedWebcam1.Apply();
        }
    }
}
