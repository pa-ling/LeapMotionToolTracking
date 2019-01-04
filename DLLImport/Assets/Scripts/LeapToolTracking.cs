using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Leap.Unity;

public class LeapToolTracking : LeapImageRetriever {

    private Texture2D processedWebcam0, processedWebcam1;

    private void Start()
    {
        processedWebcam0 = new Texture2D(640, 240);
        GameObject.Find("DisplayCamera0").GetComponentInChildren<MeshRenderer>().material.mainTexture = processedWebcam0;
        processedWebcam1 = new Texture2D(640, 240);
        GameObject.Find("DisplayCamera1").GetComponentInChildren<MeshRenderer>().material.mainTexture = processedWebcam1;
    }

    [DllImport("TestDLL", EntryPoint = "ProcessImageData")]
    public static extern void ProcessImageData(byte[] raw, byte[] processed, int width, int height);

    private void OnPreRender()
    {
        if (_currentImage != null)
        {
            byte[] rawImg0 = _currentImage.Data(Leap.Image.CameraType.LEFT);
            byte[] rawImg1 = _currentImage.Data(Leap.Image.CameraType.RIGHT);

            byte[] processedImg = new byte[rawImg0.Length];
            ProcessImageData(rawImg0, processedImg, _currentImage.Width, _currentImage.Height);
            //Debug.Log("Width: " + _currentImage.Width + ", Height: " + _currentImage.Height);

            Color32[] processedColors0 = new Color32[_currentImage.Width * _currentImage.Height];
            Color32[] processedColors1 = new Color32[_currentImage.Width * _currentImage.Height];
            for (int i = 0; i < _currentImage.Width; i++)
            {
                for (int j = 0; j < _currentImage.Height; j++)
                {
                    int index = j * _currentImage.Width + i;
                    processedColors0[index] = new Color32(rawImg0[index], rawImg0[index], rawImg0[index], 1);
                }
            }

            processedWebcam0.SetPixels32(processedColors0);
            processedWebcam0.Apply();
            processedWebcam1.SetPixels32(processedColors1);
            processedWebcam1.Apply();
        }
    }
}
