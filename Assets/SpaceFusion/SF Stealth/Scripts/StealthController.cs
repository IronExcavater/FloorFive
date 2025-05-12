using System;
using System.Collections.Generic;
using System.Linq;
using SpaceFusion.Shared.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

namespace SpaceFusion.SF_Stealth.Scripts {

    /// <summary>
    /// Handles activating and deactivating the stealth mode
    /// </summary>
    public class StealthController : ValueUpdater {


        [Header("Clone object with stealth shader applied")]
        public GameObject stealthObject;

        [Header("Direction from where the stealth effect starts")]
        public Vector3 activationDirection;

        [FormerlySerializedAs("sizeAddValue")]
        [Header("Will be added additionally to the object size")]
        public float sizeOffset;


        [Header("specify durations in seconds")]
        public float activationDuration;

        public float deactivationDuration;


        private MeshRenderer mesh;
        private static readonly int ObjectSize = Shader.PropertyToID("_Object_Size");
        private static readonly int StealthAmount = Shader.PropertyToID("_Stealth_Amount");
        private static readonly int ActivationDirection = Shader.PropertyToID("_Activation_Direction");


        private List<MeshInfo> meshInfoList;

        private void Awake() {
            meshInfoList = GetMeshInfo(stealthObject);
            SetStartStealthConfiguration(meshInfoList);
        }

        private void Update() {
            UpdateStealthAmount(meshInfoList, GetUpdatedValue());
            if (elapsedTime >= duration) {
                inProgress = false;
            }
        }

        /// <summary>
        /// if stealth is fully active it deactivates the stealth again, other wise if already deactivated it activates it again
        /// </summary>
        private void ChangeStealthVisibility() {
            if (Math.Abs(currentValue - startValue) < 0.001f) {
                Activate(activationDuration);
            } else {
                Deactivate(deactivationDuration);
            }
        }

        /// <summary>
        /// checks the stealth direction and checks in which direction the object has the largest size.
        /// returns the largest size based on the direction + sizeAddValue
        /// </summary>
        private float GetMaxObjectSize(MeshRenderer meshRenderer) {
            var bounds = meshRenderer.bounds.size;
            var scale = gameObject.transform.localScale;
            // calculate the real object size
            var realSize = new Vector3(bounds.x / scale.x, bounds.y / scale.y, bounds.z / scale.z);

            // Since the center of the object is handled as 0 in the shader, half is going in positive direction and half in negative direction
            var adaptedSize = (realSize / 2);
            // Initialize max value to a very small number
            var maxConsideredValue = 0f;

            // Check each component and update maxConsideredValue if the direction is set ( not zero)
            if (activationDirection.x != 0) {
                maxConsideredValue = Math.Max(maxConsideredValue, adaptedSize.x);
            }

            if (activationDirection.y != 0) {
                maxConsideredValue = Math.Max(maxConsideredValue, adaptedSize.y);
            }

            if (activationDirection.z != 0) {
                maxConsideredValue = Math.Max(maxConsideredValue, adaptedSize.z);
            }


            if (maxConsideredValue <= 0f) {
                throw new InvalidOperationException("No stealth direction is set.");
            }

            return maxConsideredValue + sizeOffset;
        }


        /// <summary>
        /// extracts all renderers and calculates the object size for each renderer.
        /// Returns a MeshInfo helper class that stores this information
        /// </summary>
        private List<MeshInfo> GetMeshInfo(GameObject g) {
            var renderers = g.GetComponentsInChildren<MeshRenderer>();
            return renderers.Select(meshRenderer => new MeshInfo {
                mesh = meshRenderer, objectSize = GetMaxObjectSize(meshRenderer)
            }).ToList();
        }

        /// <summary>
        /// Sets the starting ShaderGraph values for Materials that belong to the stealthObject
        /// </summary>
        private void SetStartStealthConfiguration(List<MeshInfo> meshInfo) {
            foreach (var info in meshInfo) {
                foreach (var meshMaterial in info.mesh.materials) {
                    meshMaterial.SetFloat(ObjectSize, info.objectSize);
                    meshMaterial.SetVector(ActivationDirection, activationDirection);
                    meshMaterial.SetFloat(StealthAmount, startValue);
                }
            }
        }

        /// <summary>
        /// Updates the stealth a mount of all Materials that belong to the stealthObject
        /// </summary>
        private static void UpdateStealthAmount(List<MeshInfo> meshInfo, float stealthAmount) {
            foreach (var meshMaterial in meshInfo.SelectMany(info => info.mesh.materials)) {
                meshMaterial.SetFloat(StealthAmount, stealthAmount);
            }
        }


    }

    /// <summary>
    /// Helper class to store the actual object size for each mesh
    /// </summary>
    public class MeshInfo {

        public MeshRenderer mesh;
        public float objectSize;

    }
}