using System;
using System.Threading.Tasks;
using Microsoft.Azure.Kinect.Sensor;
using Unity.Collections;
using UnityEngine;

namespace Shoten11Sample
{
    public class GetCameraData : MonoBehaviour
    {
        private Device _kinect;
        private bool _isRunning = false;

        private byte[] _rawColorData = null;
        private byte[] _xyz = null;

        private readonly int MainTex = Shader.PropertyToID("_UnlitColorMap");

        private Texture2D _colorImageTexture = null;
        private Texture2D _xyzImageTexture = null;

        [SerializeField] private GameObject _testPlane;

        private Transformation _kinectTransformation = null;

        [SerializeField] private int w;
        [SerializeField] private int h;


        private void Start()
        {
            // camera settings

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

            // test plane settings

            var depthCalibration
                = _kinect.GetCalibration().DepthCameraCalibration;

            _kinectTransformation
                = _kinect.GetCalibration().CreateTransformation();

            _colorImageTexture = new Texture2D(
                depthCalibration.ResolutionWidth,
                depthCalibration.ResolutionHeight,
                TextureFormat.BGRA32, false
            );

            _xyzImageTexture = new Texture2D(
                depthCalibration.ResolutionWidth,
                depthCalibration.ResolutionHeight,
                TextureFormat.RGB48, false);
            _xyzImageTexture.wrapMode = TextureWrapMode.Repeat;

            if (_testPlane == null) return;

            _testPlane.transform.localScale
                = new Vector3(
                    1f, 1f,
                    (float) depthCalibration.ResolutionHeight
                    / depthCalibration.ResolutionWidth
                );

            // set Texture2D to unlit material
            _testPlane.GetComponent<MeshRenderer>()
                .material.SetTexture(MainTex, _xyzImageTexture);

            // start capturing loop
            _ = Task.Run(CaptureLoop);
        }

        /// <summary>
        /// Capture color image frame from azure kienct
        /// while running.
        /// </summary>
        private void CaptureLoop()
        {
            while (_isRunning)
            {
                using var capture = _kinect.GetCapture();
                Image colorImage =
                    _kinectTransformation
                        .ColorImageToDepthCamera(capture);
                _rawColorData = colorImage.Memory.ToArray();

                Image xyzImage = _kinectTransformation.DepthImageToPointCloud(capture.Depth);
                _xyz = xyzImage.Memory.ToArray();
            }
        }

        /// <summary>
        /// update color texture
        /// </summary>
        private void Update()
        {
            if (_rawColorData != null)
            {
                _colorImageTexture.LoadRawTextureData(_rawColorData);
                _colorImageTexture.Apply();
            }

            if (_xyz != null)
            {
                _xyzImageTexture.LoadRawTextureData(_xyz);
                _xyzImageTexture.Apply();

                Debug.Log(_xyzImageTexture.GetRawTextureData<Int16>().Length);
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