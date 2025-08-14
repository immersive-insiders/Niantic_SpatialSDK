// Copyright 2022-2025 Niantic.

using Niantic.Lightship.AR.Utilities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace Niantic.Lightship.MetaQuest.InternalSamples
{
    /// <summary>
    /// As of AR Foundation 6.1, the MetaOpenXROcclusionSubsystem does not implement
    /// acquiring cpu images for depth. This sample shows how to use the Lightship
    /// extension method to acquire a cpu image from the occlusion subsystem anyway.
    /// </summary>
    public sealed class CpuImageExample : MonoBehaviour
    {
        [SerializeField]
        private AROcclusionManager _occlusionManager;

        [SerializeField]
        private RawImage _rawImage;

        [SerializeField]
        private Text _imageInfoText;

        // Resources
        private Texture2D _tempTexture;
        private Texture2D _depthTexture2D;

        private void Start()
        {
            // Reset the UI elements
            _rawImage.texture = null;
            _imageInfoText.text = "No image available";
        }

        private void Update()
        {
            // Get the occlusion subsystem
            var subsystem = _occlusionManager.subsystem;
            if (subsystem == null)
            {
                return;
            }

            /*
             * Acquire the depth XRCpuImage using the Lightship extension
             *
             * The TryAcquireEnvironmentDepthCpuImageExt API blits the GPU depth to an
             * internal texture with a cpu allocation and then copies the texture data
             * to an XRCpuImage. Do not overuse this API as it is expensive due to its
             * indirect nature.
             */
            if (subsystem.TryAcquireEnvironmentDepthCpuImageExt(ref _tempTexture, out var cpuImage))
            {
                /*
                 * At this point, the XRCpuImage is ready to use. Here, we create a texture from it
                 * to be able to display it on the UI as proof. In real applications, you should never
                 * create a texture from a depth XRCpuImage. You should get a depth texture from the
                 * occlusion manager directly.
                 */
                if (cpuImage.CreateOrUpdateTexture(ref _depthTexture2D))
                {
                    _rawImage.texture = _depthTexture2D;
                    _imageInfoText.text = $"Depth Image: {cpuImage.width}x{cpuImage.height} - {cpuImage.format}";
                }

                // Release the cpu image
                cpuImage.Dispose();
            }
        }

        private void OnDestroy()
        {
            if (_tempTexture!= null)
            {
                Destroy(_tempTexture);
            }

            if (_depthTexture2D != null)
            {
                Destroy(_depthTexture2D);
            }
        }
    }
}
