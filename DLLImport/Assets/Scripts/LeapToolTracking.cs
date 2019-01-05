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

    private int ROW_OFFSET = 100;
    private int COL_OFFSET = 60;

    private Texture2D processedWebcam0, processedWebcam1;

    private void Start()
    {
        Debug.Log("LeapToolTracking initalized.");
        processedWebcam0 = new Texture2D(TEX_WIDTH - 2 * COL_OFFSET, TEX_HEIGHT - 2 * ROW_OFFSET);
        GameObject.Find("DisplayCamera0").GetComponentInChildren<MeshRenderer>().material.mainTexture = processedWebcam0;
        processedWebcam1 = new Texture2D(TEX_WIDTH, TEX_HEIGHT);
        GameObject.Find("DisplayCamera1").GetComponentInChildren<MeshRenderer>().material.mainTexture = processedWebcam1;
    }

    [DllImport("TestDLL", EntryPoint = "ConvertByteToColor")]
    public static extern void ConvertByteToColor(byte[] raw, Color32[] img0, int width, int height);

    [DllImport("TestDLL", EntryPoint = "CropImage")]
    public static extern void CropImage(byte[] imgData, byte[] croppedImgData, int width, int height, int startX, int startY, int cropWidth, int cropHeight);

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

            //Debug.Log("Width: " + _currentImage.Width + ", Height: " + _currentImage.Height);

            byte[] undistortedLeftImg = new byte[TEX_WIDTH * TEX_HEIGHT];
            byte[] undistortedRightImg = new byte[TEX_WIDTH * TEX_HEIGHT];
            for (float row = ROW_OFFSET; row < TEX_HEIGHT - ROW_OFFSET; row++)
            {
                for (float col = COL_OFFSET; col < TEX_WIDTH - COL_OFFSET; col++)
                {
                    //Normalize from pixel xy to range [0..1]
                    Leap.Vector input = new Leap.Vector();
                    input.x = col / TEX_WIDTH;
                    input.y = row / TEX_HEIGHT;

                    //Convert from normalized [0..1] to ray slopes
                    input.x = (input.x - (float) .5) * MAX_FOV;
                    input.y = (input.y - (float) .5) * MAX_FOV;

                    int dindex = (int)Mathf.Floor((row - ROW_OFFSET) * TEX_WIDTH + col - COL_OFFSET);

                    Leap.Vector pixelLeft = _currentImage.RectilinearToPixel(Leap.Image.CameraType.LEFT, input);
                    int pindexLeft = (int) Mathf.Floor(pixelLeft.y) * _currentImage.Width + (int) Mathf.Floor(pixelLeft.x);

                    if (pixelLeft.x >= 0 && pixelLeft.x < _currentImage.Width && pixelLeft.y >= 0 && pixelLeft.y < _currentImage.Height)
                    {
                        undistortedLeftImg[dindex] = leftImgData[pindexLeft];
                    }
                    else
                    {
                        undistortedLeftImg[dindex] = 128;
                    }

                    Leap.Vector pixelRight = _currentImage.RectilinearToPixel(Leap.Image.CameraType.RIGHT, input);
                    int pindexRight = (int)Mathf.Floor(pixelRight.y) * _currentImage.Width + (int)Mathf.Floor(pixelRight.x);

                    if (pixelRight.x >= 0 && pixelRight.x < _currentImage.Width && pixelRight.y >= 0 && pixelRight.y < _currentImage.Height)
                    {
                        undistortedRightImg[dindex] = rightImgData[pindexRight];
                    }
                    else
                    {
                        undistortedRightImg[dindex] = 128;
                    }
                }
            }

            byte[] croppedUndistortedLeftImg = new byte[(TEX_WIDTH - 2*COL_OFFSET) * (TEX_HEIGHT - 2*ROW_OFFSET)];
            CropImage(undistortedLeftImg, croppedUndistortedLeftImg, TEX_WIDTH, TEX_HEIGHT, 0, 0, TEX_WIDTH - 2*COL_OFFSET, TEX_HEIGHT - 2*ROW_OFFSET);

            Color32[] undistortedLeftImgColors = new Color32[croppedUndistortedLeftImg.Length];
            Color32[] undistortedRightImgColors = new Color32[undistortedRightImg.Length];
            ConvertByteToColor(croppedUndistortedLeftImg, undistortedLeftImgColors, TEX_WIDTH- 2*COL_OFFSET, TEX_HEIGHT - 2 * ROW_OFFSET);
            ConvertByteToColor(undistortedRightImg, undistortedRightImgColors, TEX_WIDTH, TEX_HEIGHT);
            //GetDepthMap(undistortedLeftImg, undistortedRightImg, depthMap, TEX_WIDTH, TEX_HEIGHT);

            processedWebcam0.SetPixels32(undistortedLeftImgColors);
            processedWebcam0.Apply();
            processedWebcam1.SetPixels32(undistortedRightImgColors);
            processedWebcam1.Apply();
        }
    }
}
