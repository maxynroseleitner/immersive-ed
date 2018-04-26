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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerGenerateNormalTexture2DWindow : EditorWindow
    {
        private string inputFile;
        private string outputFile;

        private Texture2D inputFileTexture;
        private Texture2D outputFileTexture;
        private Vector2 scrollPosition;
        private Material material;
        private bool texturesDirty = true;

        private void LoadTextures()
        {
            if (!texturesDirty)
            {
                return;
            }
            texturesDirty = false;

            try
            {
                if (inputFileTexture == null)
                {
                    inputFileTexture = new Texture2D(1, 1);
                }
                inputFileTexture.LoadImage(File.ReadAllBytes(inputFile));
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Unable to load input texture: {0}", ex.Message);
            }
            try
            {
                if (outputFileTexture == null)
                {
                    outputFileTexture = new Texture2D(1, 1);
                }
                outputFileTexture.LoadImage(File.ReadAllBytes(outputFile));
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Unable to load output texture: {0}", ex.Message);
            }
        }

        private void OnEnable()
        {
            OnFocus();    
        }

        private void OnDisable()
        {
            OnLostFocus();
        }

        private void OnFocus()
        {
            inputFile = EditorPrefs.GetString("WeatherMaker_GenerateNormalTexture2DWindow_inputFile");
            outputFile = EditorPrefs.GetString("WeatherMaker_GenerateNormalTexture2DWindow_outputFile");
            LoadTextures();
        }

        private void OnLostFocus()
        {
            EditorPrefs.SetString("WeatherMaker_GenerateNormalTexture2DWindow_inputFile", inputFile);
            EditorPrefs.SetString("WeatherMaker_GenerateNormalTexture2DWindow_outputFile", outputFile);
            texturesDirty = true;
        }

        private void OnDestroy()
        {
            OnLostFocus();
        }

        private void OnGUI()
        {
            const float textHeight = 20.0f;
            const float textButtonWidth = 100.0f;
            const float submitButtonWidth = 150.0f;
            EditorGUIUtility.labelWidth = 80.0f;
            EditorGUI.BeginChangeCheck();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Input Image", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            inputFile = EditorGUILayout.TextField(new GUIContent(string.Empty, inputFile), inputFile, GUILayout.ExpandWidth(true), GUILayout.Height(textHeight));
            if (GUILayout.Button("Input...", GUILayout.MaxWidth(textButtonWidth)))
            {
                string newFile = EditorUtility.OpenFilePanelWithFilters("Select input image", string.Empty, WeatherMakerEditorCommands.ImageFilesFilter);
                if (!string.IsNullOrEmpty(newFile))
                {
                    inputFile = newFile;
                    LoadTextures();
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Output Image", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            outputFile = EditorGUILayout.TextField(new GUIContent(string.Empty, outputFile), outputFile, GUILayout.ExpandWidth(true), GUILayout.Height(textHeight));
            if (GUILayout.Button("Output...", GUILayout.MaxWidth(textButtonWidth)))
            {
                string newFile = EditorUtility.SaveFilePanel("Select output image", string.Empty, "Normals.png", "png");
                if (!string.IsNullOrEmpty(newFile))
                {
                    outputFile = newFile;
                    LoadTextures();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(inputFileTexture, GUILayout.MaxWidth(256.0f), GUILayout.MaxHeight(256.0f));
            GUILayout.Label(outputFileTexture, GUILayout.MaxWidth(256.0f), GUILayout.MaxHeight(256.0f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Generate Normals", GUILayout.MaxWidth(submitButtonWidth)))
            {
                // do normals
                if (material == null)
                {
                    Shader shader = Shader.Find("WeatherMaker/WeatherMakerNormalGenerator2DShader");
                    material = new Material(shader);
                }
                Texture2D input = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                input.wrapMode = TextureWrapMode.Repeat;
                input.filterMode = FilterMode.Bilinear;
                input.LoadImage(File.ReadAllBytes(inputFile));
                material.SetTexture("_MainTex", input);
                Texture2D output = new Texture2D(input.width, input.height, TextureFormat.ARGB32, false);
                output.wrapMode = TextureWrapMode.Repeat;
                RenderTexture outputRT = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                RenderTexture active = RenderTexture.active;
                RenderTexture.active = outputRT;
                Graphics.Blit(input, material);
                output.ReadPixels(new Rect(0, 0, input.width, input.height), 0, 0);
                output.Apply();
                RenderTexture.active = active;
                GameObject.DestroyImmediate(material);
                if (Path.GetExtension(outputFile).Equals(".png", StringComparison.OrdinalIgnoreCase))
                {
                    File.WriteAllBytes(outputFile, output.EncodeToPNG());
                }
                else
                {
                    File.WriteAllBytes(outputFile, output.EncodeToJPG(92));
                }
                UnityEngine.Object.DestroyImmediate(input);
                UnityEngine.Object.DestroyImmediate(output);
            }

            if (EditorGUI.EndChangeCheck())
            {
                texturesDirty = true;
                OnFocus();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    public static class WeatherMakerEditorCommands
    {
        public static readonly string[] ImageFilesFilter = new string[] { "Image Files", "png,jpg,jpeg", "All Files", "*" };

        [MenuItem("Window/Weather Maker/Generate Normal Texture (2D)", priority = 0)]
        public static void GenerateNormalTexture2D()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(WeatherMakerGenerateNormalTexture2DWindow));
            window.titleContent = new GUIContent("Normals");
        }

        [MenuItem("Window/Weather Maker/Generate 3D Texture", priority = 1)]
        public static void Generate3DTexture()
        {
            string inputFolder = EditorUtility.OpenFolderPanel("Select input assets folder of images, i.e. myproject/assets/subfolder/images/", string.Empty, string.Empty);
            if (string.IsNullOrEmpty(inputFolder))
            {
                return;
            }

            Texture3D texture = null;
            string[] allFiles = Directory.GetFiles(inputFolder);
            List<string> files = new List<string>();
            foreach (string file in allFiles)
            {
                if (Path.GetExtension(file).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(file).Equals(".png", StringComparison.OrdinalIgnoreCase))
                {
                    files.Add(file);
                }
            }
            List<Color32> allPixels = new List<Color32>();
            foreach (string file in files)
            {
                int pos = file.IndexOf("/Assets/");
                string assetFile = file.Substring(pos + 1, file.Length - pos - 1);
                Texture2D tex2D = AssetDatabase.LoadAssetAtPath(assetFile, typeof(Texture2D)) as Texture2D;
                Color32[] pixels = tex2D.GetPixels32();
                if (texture == null)
                {
                    texture = new Texture3D(tex2D.width, tex2D.height, files.Count, tex2D.format, true);
                    texture.filterMode = tex2D.filterMode;
                    texture.wrapMode = tex2D.wrapMode;
                    texture.name = "Texture3D (Weather Maker)";
                }
                allPixels.AddRange(pixels);
            }
            texture.SetPixels32(allPixels.ToArray());
            texture.Apply();
            UnityEditor.AssetDatabase.CreateAsset(texture, "Assets/My3DTexture.asset");
            EditorUtility.DisplayDialog("3D texture saved", "New texture asset created as 'Assets/My3DTexture.asset", "OK");
        }

        [MenuItem("Window/Weather Maker/Combine Normal and Alpha Textures", priority = 1)]
        public static void CombineNormalAndAlphaTextures()
        {
            string normalMapFile = EditorUtility.OpenFilePanelWithFilters("Select normal map texture", string.Empty, ImageFilesFilter);
            if (string.IsNullOrEmpty(normalMapFile))
            {
                return;
            }
            string alphaFile = EditorUtility.OpenFilePanelWithFilters("Select alpha texture", string.Empty, ImageFilesFilter);
            if (string.IsNullOrEmpty(alphaFile))
            {
                return;
            }
            try
            {
                Texture2D normalMapTexture = new Texture2D(1, 1);
                normalMapTexture.LoadImage(File.ReadAllBytes(normalMapFile));
                Texture2D alphaTexture = new Texture2D(1, 1);
                alphaTexture.LoadImage(File.ReadAllBytes(alphaFile));
                if (normalMapTexture.width != alphaTexture.width || normalMapTexture.height != alphaTexture.height)
                {
                    throw new InvalidOperationException("Normal map and alpha image must be same size");
                }
                Texture2D final = new Texture2D(normalMapTexture.width, normalMapTexture.height, TextureFormat.ARGB32, false, false);
                string combinedFile = EditorUtility.SaveFilePanel("Save final texture", string.Empty, "Combined.png", "png");
                if (string.IsNullOrEmpty(combinedFile))
                {
                    return;
                }

                Color32[] normalMapPixels = normalMapTexture.GetPixels32();
                Color32[] alphaPixels = alphaTexture.GetPixels32();
                Color32[] finalPixels = new Color32[normalMapPixels.Length];
                for (int i = 0; i < normalMapPixels.Length; i++)
                {
                    Vector3 normal = new Vector3(normalMapPixels[i].r / 255.0f, normalMapPixels[i].g / 255.0f, normalMapPixels[i].b / 255.0f).normalized;
                    finalPixels[i] = new Color32((byte)(normal.x * 255.0f), (byte)(normal.y * 255.0f), (byte)(normal.z * 255.0f), alphaPixels[i].a);
                }
                final.SetPixels32(finalPixels);
                final.Apply();
                if (Path.GetExtension(combinedFile).Equals(".png", StringComparison.OrdinalIgnoreCase))
                {
                    File.WriteAllBytes(combinedFile, final.EncodeToPNG());
                }
                else
                {
                    File.WriteAllBytes(combinedFile, final.EncodeToJPG(92));
                }
                EditorUtility.DisplayDialog("Success", "Combined texture saved at " + combinedFile, "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", ex.Message, "OK");
            }
        }
    }
}
