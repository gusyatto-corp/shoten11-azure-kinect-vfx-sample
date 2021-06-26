﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.Kinect.Sensor;
using UnityEngine;

namespace Shoten11Sample
{
    public class GetCameraData : MonoBehaviour
    {
        private Device _kinect;
        private bool _isRunning = false;

        private byte[] _rawColorData = null;

        private readonly int MainTex = Shader.PropertyToID("_UnlitColorMap");

        private Texture2D _colorImageTexture = null;

        [SerializeField] private GameObject _testPlane;

        private void Start()
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

            var colorCalibration = _kinect.GetCalibration().ColorCameraCalibration;
            _colorImageTexture = new Texture2D(colorCalibration.ResolutionWidth, colorCalibration.ResolutionHeight,
                TextureFormat.BGRA32, false);

            if (_testPlane == null) return;

            _testPlane.transform.localScale = new Vector3(1f, 1f,
                (float) colorCalibration.ResolutionHeight / colorCalibration.ResolutionWidth);
            _testPlane.GetComponent<MeshRenderer>().material.SetTexture(MainTex, _colorImageTexture);

            _ = Task.Run(CaptureLoop);
        }

        private void CaptureLoop()
        {
            while (_isRunning)
            {
                using var capture = _kinect.GetCapture();
                Image colorImage = capture.Color;
                _rawColorData = colorImage.Memory.ToArray();
            }
        }

        private void Update()
        {
            if (_rawColorData != null)
            {
                _colorImageTexture.LoadRawTextureData(_rawColorData);
                _colorImageTexture.Apply();
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
}