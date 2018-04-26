//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset off of leak forums or any other horrible evil pirate site, please consider buying it from the Unity asset store at https ://www.assetstore.unity3d.com/en/#!/content/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many others) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Represents a command buffer
    /// </summary>
    public class WeatherMakerCommandBuffer
    {
        /// <summary>
        /// Camera the command buffer is attached to
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// Render queue for the command buffer
        /// </summary>
        public CameraEvent RenderQueue;

        /// <summary>
        /// The command buffer
        /// </summary>
        public CommandBuffer CommandBuffer;

        /// <summary>
        /// A copy of the original material to render with, will be destroyed when command buffer is removed
        /// </summary>
        public Material Material;

        /// <summary>
        /// Whether the command buffer is a reflection
        /// </summary>
        public bool IsReflection { get; set; }

        /// <summary>
        /// Optional action to update material properties
        /// </summary>
        public System.Action<WeatherMakerCommandBuffer> UpdateMaterial;
    }

    public class WeatherMakerCommandBufferManagerScript : MonoBehaviour
    {
        /// <summary>
        /// Set this in OnWillRenderObject for the current reflection Vector
        /// </summary>
        public static Vector3? CurrentReflectionPlane;

        private readonly List<WeatherMakerCommandBuffer> commandBuffers = new List<WeatherMakerCommandBuffer>();
        private readonly Vector4[] frustumCorners = new Vector4[8];
        private readonly Vector3[] frustumCorners2 = new Vector3[4];
        private readonly Rect viewport = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        private Camera lastNonReflectionCamera;
        private readonly Matrix4x4[] inverseView = new Matrix4x4[2];

        private void UpdateDeferredShadingKeyword(Camera camera, Material material)
        {
            if (camera.actualRenderingPath == RenderingPath.DeferredShading)
            {
                if (!material.IsKeywordEnabled("WEATHER_MAKER_DEFERRED_SHADING"))
                {
                    material.EnableKeyword("WEATHER_MAKER_DEFERRED_SHADING");
                }
            }
            else if (material.IsKeywordEnabled("WEATHER_MAKER_DEFERRED_SHADING"))
            {
                material.DisableKeyword("WEATHER_MAKER_DEFERRED_SHADING");
            }
        }

        private void UpdateCommandBuffersForCamera(Camera camera)
        {
            if (camera == null)
            {
                return;
            }
            foreach (WeatherMakerCommandBuffer commandBuffer in commandBuffers)
            {
                if (camera != commandBuffer.Camera)
                {
                    continue;
                }
                UpdateDeferredShadingKeyword(camera, commandBuffer.Material);
                if (commandBuffer.IsReflection)
                {
                    camera = lastNonReflectionCamera == null ? camera : lastNonReflectionCamera;
                }
                else
                {
                    lastNonReflectionCamera = camera;
                }

                Transform ct = camera.transform;
                camera.CalculateFrustumCorners(viewport, camera.farClipPlane, camera.stereoEnabled ? Camera.MonoOrStereoscopicEye.Left : Camera.MonoOrStereoscopicEye.Mono, frustumCorners2);
                // bottom left, top left, bottom right, top right
                frustumCorners[0] = ct.TransformDirection(frustumCorners2[0]);
                frustumCorners[1] = ct.TransformDirection(frustumCorners2[1]);
                frustumCorners[2] = ct.TransformDirection(frustumCorners2[3]);
                frustumCorners[3] = ct.TransformDirection(frustumCorners2[2]);
                camera.CalculateFrustumCorners(viewport, camera.farClipPlane, Camera.MonoOrStereoscopicEye.Right, frustumCorners2);
                // bottom left, top left, bottom right, top right
                frustumCorners[4] = ct.TransformDirection(frustumCorners2[0]);
                frustumCorners[5] = ct.TransformDirection(frustumCorners2[1]);
                frustumCorners[6] = ct.TransformDirection(frustumCorners2[3]);
                frustumCorners[7] = ct.TransformDirection(frustumCorners2[2]);
                if (commandBuffer.IsReflection)
                {
                    Vector3 plane = (CurrentReflectionPlane == null ? Vector3.up : CurrentReflectionPlane.Value);
                    frustumCorners[0] = Vector3.Reflect(frustumCorners[0], plane);
                    frustumCorners[1] = Vector3.Reflect(frustumCorners[1], plane);
                    frustumCorners[2] = Vector3.Reflect(frustumCorners[2], plane);
                    frustumCorners[3] = Vector3.Reflect(frustumCorners[3], plane);
                    frustumCorners[4] = Vector3.Reflect(frustumCorners[4], plane);
                    frustumCorners[5] = Vector3.Reflect(frustumCorners[5], plane);
                    frustumCorners[6] = Vector3.Reflect(frustumCorners[6], plane);
                    frustumCorners[7] = Vector3.Reflect(frustumCorners[7], plane);
                }
                camera = commandBuffer.Camera;
                commandBuffer.Material.SetVectorArray("_WeatherMakerCameraFrustumRays", frustumCorners);
                if (camera.stereoEnabled)
                {
                    inverseView[0] = camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
                    inverseView[1] = camera.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
                }
                else
                {
                    inverseView[0] = inverseView[1] = camera.cameraToWorldMatrix;
                }
                commandBuffer.Material.SetMatrixArray("_WeatherMakerInverseView", inverseView);
                if (commandBuffer.UpdateMaterial != null)
                {
                    commandBuffer.UpdateMaterial(commandBuffer);
                }
            }
        }

        private void CleanupCommandBuffer(WeatherMakerCommandBuffer commandBuffer)
        {
            if (commandBuffer == null)
            {
                return;
            }
            else if (commandBuffer.Material != null)
            {
                GameObject.Destroy(commandBuffer.Material);
            }
            if (commandBuffer.Camera != null && commandBuffer.CommandBuffer != null)
            {
                commandBuffer.Camera.RemoveCommandBuffer(commandBuffer.RenderQueue, commandBuffer.CommandBuffer);
                commandBuffer.CommandBuffer.Release();
            }
        }

        private void CleanupCameras()
        {
            // remove destroyed camera command buffers
            for (int i = commandBuffers.Count - 1; i >= 0; i--)
            {
                if (commandBuffers[i].Camera == null)
                {
                    CleanupCommandBuffer(commandBuffers[i]);
                    commandBuffers.RemoveAt(i);
                }
            }
        }

        private void RemoveAllCommandBuffers()
        {
            for (int i = commandBuffers.Count - 1; i >= 0; i--)
            {
                CleanupCommandBuffer(commandBuffers[i]);
            }
            commandBuffers.Clear();
        }

        private void SceneManagerSceneLoaded(Scene newScene, LoadSceneMode mode)
        {
            RemoveAllCommandBuffers();
        }

        private void Start()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManagerSceneLoaded;
        }

        private void LateUpdate()
        {
            CleanupCameras();
        }

        /// <summary>
        /// Add a command buffer
        /// </summary>
        /// <param name="commandBuffer">Command buffer to add, the CommandBuffer property must have a unique name assigned</param>
        /// <returns>True if added, false if not</returns>
        public bool AddCommandBuffer(WeatherMakerCommandBuffer commandBuffer)
        {
            if (commandBuffer == null || string.IsNullOrEmpty(commandBuffer.CommandBuffer.name))
            {
                return false;
            }
            RemoveCommandBuffer(commandBuffer.Camera, commandBuffer.CommandBuffer.name);
            commandBuffers.Add(commandBuffer);
            commandBuffer.Camera.AddCommandBuffer(commandBuffer.RenderQueue, commandBuffer.CommandBuffer);
            return true;
        }

        /// <summary>
        /// Remove a command buffer
        /// </summary>
        /// <param name="commandBuffer">Command buffer to remove</param>
        /// <returns>True if removed, false if not</returns>
        public bool RemoveCommandBuffer(WeatherMakerCommandBuffer commandBuffer)
        {
            if (commandBuffer == null)
            {
                return false;
            }
            int index = commandBuffers.IndexOf(commandBuffer);
            if (index >= 0)
            {
                CleanupCommandBuffer(commandBuffers[index]);
                commandBuffers.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove a command buffer
        /// </summary>
        /// <param name="camera">Camera to remove command buffer on</param>
        /// <param name="name">Name of the command buffer to remove</param>
        /// <returns>True if removed, false if not</returns>
        public bool RemoveCommandBuffer(Camera camera, string name)
        {
            if (camera == null || string.IsNullOrEmpty(name))
            {
                return false;
            }
            for (int i = 0; i < commandBuffers.Count; i++)
            {
                if (commandBuffers[i].Camera == camera && commandBuffers[i].CommandBuffer.name == name)
                {
                    CleanupCommandBuffer(commandBuffers[i]);
                    commandBuffers.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Remove all command buffers with a specified name
        /// </summary>
        /// <param name="name">Name of the command buffers to remove</param>
        /// <returns>True if at least one command buffer removed, false otherwise</returns>
        public bool RemoveCommandBuffers(string name)
        {
            bool foundOne = false;
            for (int i = commandBuffers.Count - 1; i >= 0; i--)
            {
                if (commandBuffers[i].CommandBuffer.name == name)
                {
                    CleanupCommandBuffer(commandBuffers[i]);
                    commandBuffers.RemoveAt(i);
                    foundOne = true;
                }
            }
            return foundOne;
        }

        /// <summary>
        /// Checks for existance of a command buffer
        /// </summary>
        /// <param name="commandBuffer">Command buffer to check for</param>
        /// <returns>True if exists, false if not</returns>
        public bool ContainsCommandBuffer(WeatherMakerCommandBuffer commandBuffer)
        {
            if (commandBuffer == null || commandBuffer.Camera == null)
            {
                return false;
            }
            foreach (CommandBuffer cameraCommandBuffer in commandBuffer.Camera.GetCommandBuffers(commandBuffer.RenderQueue))
            {
                if (commandBuffer.CommandBuffer == cameraCommandBuffer)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks for existance of a command buffer by camera and name
        /// </summary>
        /// <param name="camera">Camera to check for</param>
        /// <param name="renderQueue">Camera event to check for</param>
        /// <param name="name">Name to check for</param>
        /// <returns>True if exists, false if not</returns>
        public bool ContainsCommandBuffer(Camera camera, CameraEvent renderQueue, string name)
        {
            if (camera == null || string.IsNullOrEmpty(name))
            {
                return false;
            }
            foreach (CommandBuffer cameraCommandBuffer in camera.GetCommandBuffers(renderQueue))
            {
                if (cameraCommandBuffer.name == name)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Called right before a camera is culled
        /// </summary>
        /// <param name="camera">Camera to pre-cull</param>
        public void PreCullCamera(Camera camera)
        {
            UpdateCommandBuffersForCamera(camera);
        }

        /// <summary>
        /// Called right before a camera is rendered
        /// </summary>
        /// <param name="camera">Camera to pre-render</param>
        public void PreRenderCamera(Camera camera)
        {

        }
    }
}