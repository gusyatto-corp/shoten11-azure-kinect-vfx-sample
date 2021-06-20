using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.Azure.Kinect.Sensor;
using TMPro;

public class NewBehaviourScript : MonoBehaviour
{
    private Device _kinect;
    private bool _isRunning = false;

    [SerializeField]
    private TextMeshPro _text;

    void Start()
    {
        _kinect = Device.Open();
        _kinect.StartCameras(new DeviceConfiguration
        {
            ColorFormat = ImageFormat.ColorBGRA32,
            ColorResolution = ColorResolution.R1080p,
            DepthMode = DepthMode.NFOV_2x2Binned,
            SynchronizedImagesOnly = true,
            CameraFPS = FPS.FPS30
        });

        _isRunning = true;

        if(_kinect != null && _text !=null)
        {
            _text.text = _kinect.Version.Depth.ToString();
        }


    }

    private void OnApplicationQuit()
    {
        _isRunning = false;
        if (_kinect != null)
        {
            _kinect.StopCameras();
            _kinect.Dispose();
            _kinect = null;
        }
    }
}
