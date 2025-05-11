using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFusion.SF_Stealth.Scripts {
    /// <summary>
    /// Simple script that allows you to modify some of the Stealth shader values via the UI on runtime
    /// </summary>
    public class StealthShaderConfigurator : MonoBehaviour {

        public GameObject stealthObject;

        [SerializeField]
        private Slider stealthAmount;

        [SerializeField]
        private TextMeshProUGUI stealthAmontTxt;

        [Header("Stealth Mode")]
        [SerializeField]
        private Slider distortion;

        [SerializeField]
        private TextMeshProUGUI distortionTxt;

        [SerializeField]
        private Button activateFresnel;

        [SerializeField]
        private TextMeshProUGUI activateFresnelTxt;

        [SerializeField]
        private Slider fresnelPower;

        [SerializeField]
        private TextMeshProUGUI fresnelPowerTxt;

        [SerializeField]
        private Slider stealthTwirl;

        [SerializeField]
        private TextMeshProUGUI stealthTwirlTxt;

        [SerializeField]
        private Slider stealthRotation;

        [SerializeField]
        private TextMeshProUGUI stealthRotationTxt;

        [SerializeField]
        [Header("Dissolve Noise")]
        private Slider edgeThickness;

        [SerializeField]
        private TextMeshProUGUI edgeThicknessTxt;

        [SerializeField]
        private Slider noiseTwirl;

        [SerializeField]
        private TextMeshProUGUI noiseTwirlTxt;
        
        [SerializeField]
        private Slider noiseRotation;

        [SerializeField]
        private TextMeshProUGUI noiseRotationTxt;

        [SerializeField]
        private Button activateCustomTexture;

        [SerializeField]
        private TextMeshProUGUI activateCustomTextureTxt;

        [SerializeField]
        private Slider simpleNoiseScale;

        [SerializeField]
        private TextMeshProUGUI simpleNoiseScaleTxt;


        private bool isFresnelActive;
        private bool isCustomTextureActive;
        private readonly int _decimals = 3;
        private static readonly int StealthAmount = Shader.PropertyToID("_Stealth_Amount");
        private static readonly int Distortion = Shader.PropertyToID("_Distortion");
        private static readonly int FresnelActivated = Shader.PropertyToID("_Fresnel_Activated");
        private static readonly int FresnelPower = Shader.PropertyToID("_Fresnel_Power");
        private static readonly int StealthTwirl = Shader.PropertyToID("_Stealth_Twirl");
        private static readonly int StealthRotation = Shader.PropertyToID("_Stealth_Rotation");
        private static readonly int EdgeThickness = Shader.PropertyToID("_Edge_Thickness");
        private static readonly int NoiseTwirl = Shader.PropertyToID("_Noise_Twirl");
        private static readonly int NoiseRotation = Shader.PropertyToID("_Noise_Rotation");
        private static readonly int SimpleNoiseScale = Shader.PropertyToID("_Simple_Noise_Scale");
        private static readonly int UseTexture = Shader.PropertyToID("_Use_Texture");


        private void SliderHandler(MeshRenderer mesh, Slider slider, TextMeshProUGUI sliderValue, int shaderID) {
            var val = mesh.material.GetFloat(shaderID);
            slider.value = val;
            sliderValue.text = $"{Math.Round(val, _decimals)}";
            slider.onValueChanged.AddListener(v => {
                sliderValue.text = $"{Math.Round(v, _decimals)}";
                foreach (var mat in mesh.materials) {
                    mat.SetFloat(shaderID, v);
                }
            });
        }

        private void Start() {
            var mesh = stealthObject.GetComponent<MeshRenderer>();

            SliderHandler(mesh, stealthAmount, stealthAmontTxt, StealthAmount);
            SliderHandler(mesh, distortion, distortionTxt, Distortion);
            {
                var val = mesh.material.GetFloat(FresnelActivated);
                isFresnelActive = val > 0f;
                activateFresnelTxt.text = val > 0 ? "Enabled" : "Disabled";
                activateFresnel.onClick.AddListener(() => {
                    isFresnelActive = !isFresnelActive;
                    var v = isFresnelActive ? 1f : 0f;
                    foreach (var mat in mesh.materials) {
                        mat.SetFloat(FresnelActivated, v);
                    }
                    activateFresnelTxt.text = v > 0 ? "Enabled" : "Disabled";
                });
            }
            SliderHandler(mesh, fresnelPower, fresnelPowerTxt, FresnelPower);
            SliderHandler(mesh, stealthTwirl, stealthTwirlTxt, StealthTwirl);
            SliderHandler(mesh, stealthRotation, stealthRotationTxt, StealthRotation);
            SliderHandler(mesh, edgeThickness, edgeThicknessTxt, EdgeThickness);
            SliderHandler(mesh, noiseTwirl, noiseTwirlTxt, NoiseTwirl);
            SliderHandler(mesh, noiseRotation, noiseRotationTxt, NoiseRotation);

            {
                var val = mesh.material.GetFloat(UseTexture);
                isCustomTextureActive = val > 0f;
                activateCustomTextureTxt.text = val > 0 ? "Enabled" : "Disabled";
                activateCustomTexture.onClick.AddListener(() => {
                    isCustomTextureActive = !isCustomTextureActive;
                    var v = isCustomTextureActive ? 1f : 0f;
                    foreach (var mat in mesh.materials) {
                        mat.SetFloat(UseTexture, v);
                    }
                    activateCustomTextureTxt.text = v > 0 ? "Enabled" : "Disabled";
                });
            }

            SliderHandler(mesh, simpleNoiseScale, simpleNoiseScaleTxt, SimpleNoiseScale);


        }

    }
}