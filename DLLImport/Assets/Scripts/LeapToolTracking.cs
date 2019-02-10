using System;
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

    private const int TEX_WIDTH = 400;
    private const int TEX_HEIGHT = 400;
    private const int MAX_FOV = 8;
    private const int ROW_OFFSET = 100;
    private const int COL_OFFSET = 60;
    private const int WIDTH_WITH_OFFSET = TEX_WIDTH - 2 * COL_OFFSET;
    private const int HEIGHT_WITH_OFFSET = TEX_HEIGHT - 2 * ROW_OFFSET;

    private const float DISTANCE_OF_CAMERAS = 4.0f;
    private const float CAMERA_ANGLE = 151.93f;

    public GameObject marker1;
    public GameObject marker2;

    private void OnPreRender()
    {
        if (_currentImage == null)
        {
            Debug.Log("No image data avaiable.");
            return;
        }

        int imageSize = _currentImage.Width * _currentImage.Height;
        byte[] raw = _currentImage.Data(Leap.Image.CameraType.LEFT);
        byte[] leftImgData = new byte[imageSize], rightImgData = new byte[imageSize];
        GetLeapImages(raw, leftImgData, rightImgData, imageSize);

        byte[] undistortedLeftImg = new byte[TEX_WIDTH * TEX_HEIGHT], undistortedRightImg = new byte[TEX_WIDTH * TEX_HEIGHT];
        UndistortImages(leftImgData, rightImgData, undistortedLeftImg, undistortedRightImg);

        byte[] croppedUndistortedLeftImg = new byte[WIDTH_WITH_OFFSET * HEIGHT_WITH_OFFSET], croppedUndistortedRightImg = new byte[WIDTH_WITH_OFFSET * HEIGHT_WITH_OFFSET];
        CropImage(
            undistortedLeftImg,
            croppedUndistortedLeftImg,
            TEX_WIDTH, TEX_HEIGHT,
            COL_OFFSET, //COL_OFFSET and ROW_OFFSET have to be swapped here
            ROW_OFFSET,
            WIDTH_WITH_OFFSET,
            HEIGHT_WITH_OFFSET);
        CropImage(
            undistortedRightImg,
            croppedUndistortedRightImg,
            TEX_WIDTH, TEX_HEIGHT,
            COL_OFFSET, //COL_OFFSET and ROW_OFFSET have to be swapped here
            ROW_OFFSET,
            WIDTH_WITH_OFFSET,
            HEIGHT_WITH_OFFSET);

        float[] leftMarkerLocations = new float[4], rightMarkerLocations = new float[4];
        GetMarkerLocations(croppedUndistortedLeftImg, leftMarkerLocations, WIDTH_WITH_OFFSET, HEIGHT_WITH_OFFSET, 0);
        GetMarkerLocations(croppedUndistortedRightImg, rightMarkerLocations, WIDTH_WITH_OFFSET, HEIGHT_WITH_OFFSET, 1);

        Debug.Log(System.DateTime.Now + ": Left(" + leftMarkerLocations[0] + ", " + leftMarkerLocations[1] + ", " + leftMarkerLocations[2] + ", " + leftMarkerLocations[3] + ")");
        Debug.Log(System.DateTime.Now + ": Right(" + rightMarkerLocations[0] + ", " + rightMarkerLocations[1] + ", " + rightMarkerLocations[2] + ", " + rightMarkerLocations[3] + ")");

        float dist0 = GetDepth(leftMarkerLocations[0], rightMarkerLocations[0]);
        float dist1 = GetDepth(leftMarkerLocations[2], rightMarkerLocations[2]);

        Vector3 marker0Pos = new Vector3(-leftMarkerLocations[0] + WIDTH_WITH_OFFSET / 2, dist0, -leftMarkerLocations[1] + HEIGHT_WITH_OFFSET - 100) / 10.0f;
        Vector3 marker1Pos = new Vector3(-leftMarkerLocations[2] + WIDTH_WITH_OFFSET / 2, dist1, -leftMarkerLocations[3] + HEIGHT_WITH_OFFSET - 100) / 10.0f;

        Debug.Log(System.DateTime.Now + ": Marker1" + marker0Pos);
        Debug.Log(System.DateTime.Now + ": Marker2" + marker1Pos);
        marker1.transform.position = marker0Pos;
        marker2.transform.position = marker1Pos;
    }

    private void UndistortImages(byte[] leftImgData, byte[] rightImgData, byte[] undistortedLeftImg, byte[] undistortedRightImg)
    {
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
    }

    private float GetDepth(float xL, float xR)
    {
       return (DISTANCE_OF_CAMERAS * WIDTH_WITH_OFFSET) / (float)(2 * Math.Tan(CAMERA_ANGLE / 2) * (xL - xR));
    }

    private Vector3 FilterData(float x, float y, float z)
    {
        return new Vector3();
    }
}
