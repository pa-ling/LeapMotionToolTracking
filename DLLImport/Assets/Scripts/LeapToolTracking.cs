using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Leap.Unity;

public class LeapToolTracking : LeapImageRetriever
{
    [DllImport("LeapTT", EntryPoint = "GetLeapImages")]
    public static extern void GetLeapImages(byte[] raw, byte[] img0, byte[] img1, int size);

    [DllImport("LeapTT", EntryPoint = "CropImage")]
    public static extern void CropImage(byte[] imgData, byte[] croppedImgData, int width, int height, int startX, int startY, int cropWidth, int cropHeight);

    [DllImport("LeapTT", EntryPoint = "ConvertByteToColor")]
    public static extern void ConvertByteToColor(byte[] raw, Color32[] img0, int width, int height);

    [DllImport("LeapTT", EntryPoint = "GetMarkerLocations")]
    public static extern void GetMarkerLocations(byte[] imgData, float[] markerLocations, int width, int height, int camera);

    public GameObject marker1;
    public GameObject marker2;

    private static int TEX_WIDTH = 400;
    private static int TEX_HEIGHT = 400;
    private static int MAX_FOV = 8;

    private static int ROW_OFFSET = 100;
    private static int COL_OFFSET = 60;
    private static int WIDTH_WITH_OFFSET = TEX_WIDTH - 2 * COL_OFFSET;
    private static int HEIGHT_WITH_OFFSET = TEX_HEIGHT - 2 * ROW_OFFSET;

    //private static float DISTANCE_OF_CAMERAS = 3.75f;
    private static float DISTANCE_OF_CAMERAS = 4;
    //private static float CAMERA_ANGLE = 140;
    private static float CAMERA_ANGLE = 151.93f;

    private Texture2D leftCanvas, rightCanvas;

    private void Start()
    {
        leftCanvas = new Texture2D(WIDTH_WITH_OFFSET, HEIGHT_WITH_OFFSET);
        GameObject.Find("DisplayCamera0").GetComponentInChildren<MeshRenderer>().material.mainTexture = leftCanvas;

        rightCanvas = new Texture2D(WIDTH_WITH_OFFSET, HEIGHT_WITH_OFFSET);
        GameObject.Find("DisplayCamera1").GetComponentInChildren<MeshRenderer>().material.mainTexture = rightCanvas;
    }

    private void OnPreRender()
    {
        if (_currentImage == null)
        {
            return;
        }

        int imageSize = _currentImage.Width * _currentImage.Height;
        byte[] raw = _currentImage.Data(Leap.Image.CameraType.LEFT);
        byte[] leftImgData = new byte[imageSize];
        byte[] rightImgData = new byte[imageSize];
        GetLeapImages(raw, leftImgData, rightImgData, imageSize);

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
                input.x = (input.x - (float).5) * MAX_FOV;
                input.y = (input.y - (float).5) * MAX_FOV;

                int dindex = (int)Mathf.Floor(row * TEX_WIDTH + col);

                //left image
                Leap.Vector pixelLeft = _currentImage.RectilinearToPixel(Leap.Image.CameraType.LEFT, input);
                int pindexLeft = (int)Mathf.Floor(pixelLeft.y) * _currentImage.Width + (int)Mathf.Floor(pixelLeft.x);

                if (pixelLeft.x >= 0 && pixelLeft.x < _currentImage.Width && pixelLeft.y >= 0 && pixelLeft.y < _currentImage.Height)
                {
                    undistortedLeftImg[dindex] = leftImgData[pindexLeft];
                }
                else
                {
                    undistortedLeftImg[dindex] = 0;
                }

                //right image
                Leap.Vector pixelRight = _currentImage.RectilinearToPixel(Leap.Image.CameraType.RIGHT, input);
                int pindexRight = (int)Mathf.Floor(pixelRight.y) * _currentImage.Width + (int)Mathf.Floor(pixelRight.x);

                if (pixelRight.x >= 0 && pixelRight.x < _currentImage.Width && pixelRight.y >= 0 && pixelRight.y < _currentImage.Height)
                {
                    undistortedRightImg[dindex] = rightImgData[pindexRight];
                }
                else
                {
                    undistortedRightImg[dindex] = 0;
                }
            }
        }

        byte[] croppedUndistortedLeftImg = new byte[WIDTH_WITH_OFFSET * HEIGHT_WITH_OFFSET];
        CropImage(
            undistortedLeftImg,
            croppedUndistortedLeftImg,
            TEX_WIDTH, TEX_HEIGHT,
            COL_OFFSET, //COL_OFFSET and ROW_OFFSET have to be swapped here
            ROW_OFFSET,
            WIDTH_WITH_OFFSET,
            HEIGHT_WITH_OFFSET);
        Color32[] undistortedLeftImgColors = new Color32[croppedUndistortedLeftImg.Length];
        ConvertByteToColor(croppedUndistortedLeftImg, undistortedLeftImgColors, WIDTH_WITH_OFFSET, HEIGHT_WITH_OFFSET);

        byte[] croppedUndistortedRightImg = new byte[WIDTH_WITH_OFFSET * HEIGHT_WITH_OFFSET];
        CropImage(
            undistortedRightImg,
            croppedUndistortedRightImg,
            TEX_WIDTH, TEX_HEIGHT,
            COL_OFFSET, //COL_OFFSET and ROW_OFFSET have to be swapped here
            ROW_OFFSET,
            WIDTH_WITH_OFFSET,
            HEIGHT_WITH_OFFSET);
        Color32[] undistortedRightImgColors = new Color32[croppedUndistortedRightImg.Length];
        ConvertByteToColor(croppedUndistortedRightImg, undistortedRightImgColors, WIDTH_WITH_OFFSET, HEIGHT_WITH_OFFSET);

        float[] leftMarkerLocations = new float[4];
        GetMarkerLocations(croppedUndistortedLeftImg, leftMarkerLocations, WIDTH_WITH_OFFSET, HEIGHT_WITH_OFFSET, 0);
        Debug.Log(System.DateTime.Now + ": Left(" + leftMarkerLocations[0] + ", " + leftMarkerLocations[1] + ", " + leftMarkerLocations[2] + ", " + leftMarkerLocations[3] + ")");

        float[] rightMarkerLocations = new float[4];
        GetMarkerLocations(croppedUndistortedRightImg, rightMarkerLocations, WIDTH_WITH_OFFSET, HEIGHT_WITH_OFFSET, 1);
        Debug.Log(System.DateTime.Now + ": Right(" + rightMarkerLocations[0] + ", " + rightMarkerLocations[1] + ", " + rightMarkerLocations[2] + ", " + rightMarkerLocations[3] + ")");

        float x_1L = leftMarkerLocations[0];
        float x_2L = leftMarkerLocations[2];
        float x_1R = rightMarkerLocations[0];
        float x_2R = rightMarkerLocations[2];
        float distance1 = (DISTANCE_OF_CAMERAS * WIDTH_WITH_OFFSET) / (float)(2 * Math.Tan(CAMERA_ANGLE / 2) * (x_1L - x_1R));
        float distance2 = (DISTANCE_OF_CAMERAS * WIDTH_WITH_OFFSET) / (float)(2 * Math.Tan(CAMERA_ANGLE / 2) * (x_2L - x_2R));
        //Debug.Log("(" + x_1L + ", " + leftMarkerLocations[1] + ", " + distance1 + ")");

        Vector3 marker1_pos = new Vector3(-x_1L + WIDTH_WITH_OFFSET / 2, distance1, -leftMarkerLocations[1] + HEIGHT_WITH_OFFSET) / 10.0f;
        Vector3 marker2_pos = new Vector3(-x_2L + WIDTH_WITH_OFFSET / 2, distance2, -leftMarkerLocations[3] + HEIGHT_WITH_OFFSET) / 10.0f;

        Debug.Log(System.DateTime.Now + ": Marker1" + marker1_pos);
        marker1.transform.position = marker1_pos;
        Debug.Log(System.DateTime.Now + ": Marker2" + marker2_pos);
        marker2.transform.position = marker2_pos;

        leftCanvas.SetPixels32(undistortedLeftImgColors);
        leftCanvas.Apply();
        rightCanvas.SetPixels32(undistortedRightImgColors);
        rightCanvas.Apply();
    }
}
