using System.Runtime.InteropServices;
using UnityEngine;

public class LeapToolTracking {

    [DllImport("LeapTT", EntryPoint = "Init")]
    public static extern void Init(bool debug);

    [DllImport("LeapTT", EntryPoint = "GetLeapImages")]
    public static extern void GetLeapImages(byte[] raw, byte[] img0, byte[] img1, int width, int height);

    [DllImport("LeapTT", EntryPoint = "CropImage")]
    public static extern void CropImage(byte[] imgData, byte[] croppedImgData, int width, int height, int startX, int startY, int cropWidth, int cropHeight);

    [DllImport("LeapTT", EntryPoint = "ConvertByteToColor")]
    public static extern void ConvertByteToColor(byte[] raw, Color32[] img0, int width, int height);

    [DllImport("LeapTT", EntryPoint = "GetMarkerLocations")]
    public static extern void GetMarkerLocations(byte[] imgData, float[] markerLocations, int width, int height, int camera);

}
