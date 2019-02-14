using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Leap.Unity;
using System.Collections.Generic;

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

    private const int TEX_WIDTH = 400;
    private const int TEX_HEIGHT = 400;
    private const int MAX_FOV = 8;
    private const int ROW_OFFSET = 100;
    private const int COL_OFFSET = 60;
    private const int WIDTH_WITH_OFFSET = TEX_WIDTH - 2 * COL_OFFSET;
    private const int HEIGHT_WITH_OFFSET = TEX_HEIGHT - 2 * ROW_OFFSET;

    private const float DISTANCE_OF_CAMERAS = 4.0f;
    private const float CAMERA_ANGLE = 151.93f;

    private const int VALUES_TO_KEEP = 5;

    private const float LEVEL_ACTUALITY_MODIFIER = 0.2f;
    private const float TREND_ACTUALITY_MODIFIER = 0.2f;

    private Vector3[] previousLevel;
    private Vector3[] previousTrend;

    public GameObject marker0;
    public GameObject marker1;
    public GameObject tool;
    public bool filterData;

    private void Start()
    {
        previousLevel = new Vector3[2];
        previousLevel[0] = Vector3.zero;
        previousLevel[1] = Vector3.zero;

        previousTrend = new Vector3[2];
        previousTrend[0] = Vector3.zero;
        previousTrend[1] = Vector3.zero;
    }

    private void OnPreRender()
    {
        if (_currentImage == null)
        {
            return;
        }

        int imageSize = _currentImage.Width * _currentImage.Height;
        byte[] raw = _currentImage.Data(Leap.Image.CameraType.LEFT);
        byte[] leftImgData = new byte[imageSize], rightImgData = new byte[imageSize];
        GetLeapImages(raw, leftImgData, rightImgData, imageSize);

        // Left image
        byte[] undistortedLeftImg = UndistortImage(leftImgData, Leap.Image.CameraType.LEFT);
        byte[] croppedUndistortedLeftImg = new byte[WIDTH_WITH_OFFSET * HEIGHT_WITH_OFFSET];
        CropImage(
            undistortedLeftImg,
            croppedUndistortedLeftImg,
            TEX_WIDTH, TEX_HEIGHT,
            COL_OFFSET, //COL_OFFSET and ROW_OFFSET have to be swapped here
            ROW_OFFSET,
            WIDTH_WITH_OFFSET,
            HEIGHT_WITH_OFFSET);
        float[] leftMarkerLocations = new float[4];
        GetMarkerLocations(croppedUndistortedLeftImg, leftMarkerLocations, WIDTH_WITH_OFFSET, HEIGHT_WITH_OFFSET, 0);

        // Right image
        byte[] undistortedRightImg = UndistortImage(rightImgData, Leap.Image.CameraType.RIGHT);
        byte[] croppedUndistortedRightImg = new byte[WIDTH_WITH_OFFSET * HEIGHT_WITH_OFFSET];
        CropImage(
            undistortedRightImg,
            croppedUndistortedRightImg,
            TEX_WIDTH, TEX_HEIGHT,
            COL_OFFSET, //COL_OFFSET and ROW_OFFSET have to be swapped here
            ROW_OFFSET,
            WIDTH_WITH_OFFSET,
            HEIGHT_WITH_OFFSET);
        float[] rightMarkerLocations = new float[4];
        GetMarkerLocations(croppedUndistortedRightImg, rightMarkerLocations, WIDTH_WITH_OFFSET, HEIGHT_WITH_OFFSET, 1);

        // Marker 0
        float x0 = -leftMarkerLocations[0] + WIDTH_WITH_OFFSET / 2;
        float y0 = GetDepth(leftMarkerLocations[0], rightMarkerLocations[0]) / 2;
        float z0 = -leftMarkerLocations[1] + HEIGHT_WITH_OFFSET - 100;
        Vector3 marker0Pos = new Vector3(x0, y0, z0) / 10;
        //Debug.Log(System.DateTime.Now + ": Marker0" + marker0Pos);
        if (filterData)
        {
            marker0Pos = HoltWinterDES(marker0Pos, 0);
        }
        //Debug.Log(System.DateTime.Now + ": Filtered Marker0" + marker0Pos);
        marker0.transform.localPosition = marker0Pos;

        // Marker 1
        float x1 = -leftMarkerLocations[2] + WIDTH_WITH_OFFSET / 2;
        float y1 = GetDepth(leftMarkerLocations[2], rightMarkerLocations[2]) / 2;
        float z1 = -leftMarkerLocations[3] + HEIGHT_WITH_OFFSET - 100;
        Vector3 marker1Pos = new Vector3(x1, y1, z1) / 10;
        //Debug.Log(System.DateTime.Now + ": Marker1" + marker1Pos);
        if (filterData)
        {
            marker1Pos = HoltWinterDES(marker1Pos, 1);
        }
        //Debug.Log(System.DateTime.Now + ": Filtered Marker1" + marker1Pos);
        marker1.transform.localPosition = marker1Pos;

        tool.transform.localPosition = marker0Pos;
        tool.transform.LookAt(marker1.transform.position);
    }

    private byte[] UndistortImage(byte[] imgData, Leap.Image.CameraType type)
    {
        byte[] undistortedImg = new byte[TEX_WIDTH * TEX_HEIGHT];
        for (float row = ROW_OFFSET; row < TEX_HEIGHT - ROW_OFFSET; row++)
        {
            for (float col = COL_OFFSET; col < TEX_WIDTH - COL_OFFSET; col++)
            {
                //Normalize from pixel xy to range [0..1]
                Leap.Vector input = new Leap.Vector();
                input.x = col / TEX_WIDTH;
                input.y = row / TEX_HEIGHT;

                //Convert from normalized [0..1] to ray slopes
                input.x = (input.x - 0.5f) * MAX_FOV;
                input.y = (input.y - 0.5f) * MAX_FOV;

                int dindex = (int)Mathf.Floor(row * TEX_WIDTH + col);

                //left image
                Leap.Vector pixel = _currentImage.RectilinearToPixel(type, input);
                int pindex = (int)Mathf.Floor(pixel.y) * _currentImage.Width + (int)Mathf.Floor(pixel.x);

                if (pixel.x >= 0 && pixel.x < _currentImage.Width && pixel.y >= 0 && pixel.y < _currentImage.Height)
                {
                    undistortedImg[dindex] = imgData[pindex];
                }
                else
                {
                    undistortedImg[dindex] = 0;
                }
            }
        }
        return undistortedImg;
    }

    private float GetDepth(float xL, float xR)
    {
       return (DISTANCE_OF_CAMERAS * WIDTH_WITH_OFFSET) / (float)(2 * Math.Tan(CAMERA_ANGLE / 2) * (xL - xR));
    }

    private void SaveVectorToQueue(Vector3 vector, Queue<Vector3> queue)
    {
        if (queue.Count > VALUES_TO_KEEP)
        {
            queue.Dequeue();
        }
        queue.Enqueue(vector);
    }

    private Vector3 HoltWinterDES(Vector3 input, int marker)
    {
        if (! isValid(input))
        {
            return previousLevel[marker] + previousTrend[marker];
        }

        Vector3 level = LEVEL_ACTUALITY_MODIFIER * input + (1 - LEVEL_ACTUALITY_MODIFIER) * (previousLevel[marker] + previousTrend[marker]);
        Vector3 trend = TREND_ACTUALITY_MODIFIER * (level - previousLevel[marker]) + (1 - TREND_ACTUALITY_MODIFIER) * previousTrend[marker];
        previousLevel[marker] = level;
        previousTrend[marker] = trend;

        return level + trend;
    }

    private bool isValid(Vector3 vector)
    {
        return 
            !float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z) &&
            !float.IsInfinity(vector.x) && !float.IsInfinity(vector.y) && !float.IsInfinity(vector.z);
    }

}
