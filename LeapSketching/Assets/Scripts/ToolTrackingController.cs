﻿using System;
using UnityEngine;
using Leap.Unity;

public class ToolTrackingController : LeapImageRetriever
{
    public GameObject marker0;
    public GameObject marker1;
    public GameObject tool;
    public bool filterData = true;
    public bool debug = true;

    private const int TEX_WIDTH = 500;
    private const int TEX_HEIGHT = 500;
    private const int ROW_OFFSET = 130;
    private const int COL_OFFSET = 0;
    private const int WIDTH_WITH_OFFSET = TEX_WIDTH - 2 * COL_OFFSET;
    private const int HEIGHT_WITH_OFFSET = TEX_HEIGHT - 2 * ROW_OFFSET;

    private const float H_FOV = 2.24603794f;
    private const float V_FOV = 1.5696862f;
    private const float FOV_MODIFIER = 3.4f;

    private const float DISTANCE_OF_CAMERAS = 4.0f;
    private const float CAMERA_ANGLE = 151.93f;

    private const float LEVEL_ACTUALITY_MODIFIER = 0.20f;
    private const float TREND_ACTUALITY_MODIFIER = 0.20f;

    private Vector3[] previousLevel;
    private Vector3[] previousTrend;
    private bool toolActive = true;
    private bool swapMarkers = false;
    private bool stopTracking = false;

    public void SwitchMarkers()
    {
        swapMarkers = !swapMarkers;
    }

    public void SetStopTracking(bool stopTracking)
    {
        this.stopTracking = stopTracking;
    }

    public bool ProviderConnected()
    {
        return _provider.IsConnected();
    }

    private void Start()
    {
        LeapToolTracking.Init(debug);
        previousLevel = new Vector3[2];
        previousLevel[0] = new Vector3(0, 5, 0);
        previousLevel[1] = previousLevel[0];

        previousTrend = new Vector3[2];
        previousTrend[0] = Vector3.zero;
        previousTrend[1] = Vector3.zero;

        marker0.SetActive(debug);
        marker1.SetActive(debug);

        if (debug)
        {
            marker0.GetComponent<MeshRenderer>().material.color = Color.green;
            marker1.GetComponent<MeshRenderer>().material.color = Color.blue;
        }
    }

    private void OnPreRender()
    {
        if (_currentImage == null || stopTracking)
        {
            return;
        }

        // Separate left and right image
        int imageSize = _currentImage.Width * _currentImage.Height;
        byte[] raw = _currentImage.Data(Leap.Image.CameraType.LEFT);
        byte[] leftImgData = new byte[imageSize], rightImgData = new byte[imageSize];
        LeapToolTracking.GetLeapImages(raw, leftImgData, rightImgData, _currentImage.Width, _currentImage.Height);

        // Get location of the markers in both pictures
        float[] leftMarkerLocations = ProcessImage(leftImgData, Leap.Image.CameraType.LEFT);
        float[] rightMarkerLocations = ProcessImage(rightImgData, Leap.Image.CameraType.RIGHT);

        // Sometimes Markers are not equally detected in both pictures. If this happens the left marker locations are swapped.
        if ((leftMarkerLocations[1] > leftMarkerLocations[3] && rightMarkerLocations[1] < rightMarkerLocations[3]) ||
            (leftMarkerLocations[1] < leftMarkerLocations[3] && rightMarkerLocations[1] > rightMarkerLocations[3]))
        {
            float tempX = leftMarkerLocations[0];
            float tempY = leftMarkerLocations[1];
            leftMarkerLocations[0] = leftMarkerLocations[2];
            leftMarkerLocations[1] = leftMarkerLocations[3];
            leftMarkerLocations[2] = tempX;
            leftMarkerLocations[3] = tempY;
        }
        //Debug.Log(System.DateTime.Now + ": RawMarkerL: (" + leftMarkerLocations[0] + "; " + leftMarkerLocations[1] + "); (" + leftMarkerLocations[2] + "; " + leftMarkerLocations[3] + ")");
        //Debug.Log(System.DateTime.Now + ": RawMarkerR: (" + rightMarkerLocations[0] + "; " + rightMarkerLocations[1] + "); (" + rightMarkerLocations[2] + "; " + rightMarkerLocations[3] + ")");

        if (ArrayIs(1000000, leftMarkerLocations) && ArrayIs(1000000, leftMarkerLocations))
        {
            toolActive = false;
            tool.SetActive(toolActive);
            return;
        }

        if (!toolActive)
        {
            toolActive = true;
            tool.SetActive(toolActive);
        }

        // Calculate marker position in 3D Space
        Vector3 marker0Pos = GetMarkerPosition(leftMarkerLocations[0], rightMarkerLocations[0], leftMarkerLocations[1], 0);
        Vector3 marker1Pos = GetMarkerPosition(leftMarkerLocations[2], rightMarkerLocations[2], leftMarkerLocations[3], 1);

        if (swapMarkers)
        {
            Vector3 temp = marker0Pos;
            marker0Pos = marker1Pos;
            marker1Pos = temp;
        }

        marker0.transform.localPosition = marker0Pos;
        marker1.transform.localPosition = marker1Pos;
        tool.transform.localPosition = marker0Pos;
        tool.transform.LookAt(marker1.transform.position);
    }

    private float[] ProcessImage(byte[] imgData, Leap.Image.CameraType type)
    {
        byte[] undistortedImg = UndistortImage(imgData, type);
        byte[] croppedUndistortedImg = new byte[WIDTH_WITH_OFFSET * HEIGHT_WITH_OFFSET];
        LeapToolTracking.CropImage(
            undistortedImg,
            croppedUndistortedImg,
            TEX_WIDTH, TEX_HEIGHT,
            COL_OFFSET, //COL_OFFSET and ROW_OFFSET have to be swapped here
            ROW_OFFSET,
            WIDTH_WITH_OFFSET,
            HEIGHT_WITH_OFFSET);

        float[] markerLocations = new float[] { 1000000, 1000000, 1000000, 1000000 };
        LeapToolTracking.GetMarkerLocations(croppedUndistortedImg, markerLocations, WIDTH_WITH_OFFSET, HEIGHT_WITH_OFFSET, (int) type);

        return markerLocations;
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
                input.x = (input.x - 0.5f) * V_FOV * FOV_MODIFIER;
                input.y = (input.y - 0.5f) * H_FOV * FOV_MODIFIER;

                
                Leap.Vector pixel = _currentImage.RectilinearToPixel(type, input);
                int dindex = (int) Mathf.Floor(row * TEX_WIDTH + col);
                int pindex = (int) Mathf.Floor(pixel.y) * _currentImage.Width + (int) Mathf.Floor(pixel.x);

                if (pixel.x >= 0 && pixel.x < _currentImage.Width && pixel.y >= 0 && pixel.y < _currentImage.Height)
                {
                    undistortedImg[dindex] = imgData[pindex];
                }
                else
                {
                    undistortedImg[dindex] = 100;
                }
            }
        }
        return undistortedImg;
    }

    private float GetDepth(float xL, float xR)
    {
       return (DISTANCE_OF_CAMERAS * WIDTH_WITH_OFFSET) / (float)(2 * Math.Tan(CAMERA_ANGLE / 2) * (xL - xR));
    }

    private Vector3 GetMarkerPosition(float xL, float xR, float y, int marker)
    {
        float y0 = GetDepth(xL, xR) / 2;
        float x0 = (-xL + (WIDTH_WITH_OFFSET / 2)) * y0 / 100;
        float z0 = (-y + (HEIGHT_WITH_OFFSET - 100)) * y0 / 100;
        Vector3 markerPos = new Vector3(x0, y0, z0) / 20;
        //Debug.Log(System.DateTime.Now + ": Marker" + marker + ":" + markerPos);
        if (filterData)
        {
            markerPos = HoltWinterDES(markerPos, marker);
        }
        //Debug.Log(System.DateTime.Now + ": Filtered Marker" + marker + ":" + markerPos);
        return markerPos;
    }

    private Vector3 HoltWinterDES(Vector3 input, int marker)
    {
        if (!IsValid(input))
        {
            return previousLevel[marker] + previousTrend[marker];
        }

        Vector3 level = LEVEL_ACTUALITY_MODIFIER * input + (1 - LEVEL_ACTUALITY_MODIFIER) * (previousLevel[marker] + previousTrend[marker]);
        Vector3 trend = TREND_ACTUALITY_MODIFIER * (level - previousLevel[marker]) + (1 - TREND_ACTUALITY_MODIFIER) * previousTrend[marker];
        previousLevel[marker] = level;
        previousTrend[marker] = trend;

        return level + trend;
    }

    private bool IsValid(Vector3 vector)
    {
        return 
            !(float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z) ||
            float.IsInfinity(vector.x) || float.IsInfinity(vector.y) || float.IsInfinity(vector.z));
    }

    private bool ArrayIs(float number, float[] array)
    {
        foreach (float element in array)
        {
            if (element != number)
            {
                return false;
            }
        }
        return true;
    }
}
