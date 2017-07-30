/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Layers;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using System;
#endif
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;

#endif

namespace TouchScript.Examples
{
    /// <exclude />
    public class Runner : MonoBehaviour
    {
        private static Runner instance;
        private TouchLayer layer;

        public void LoadLevel(string name)
        {
#if UNITY_5_3_OR_NEWER
            SceneManager.LoadScene(name);
#else
			Application.LoadLevel(name);
#endif
        }

        public void LoadNextLevel()
        {
#if UNITY_5_3_OR_NEWER
            SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings);
#else
			Application.LoadLevel((Application.loadedLevel + 1)%Application.levelCount);
#endif
        }

        public void LoadPreviousLevel()
        {
#if UNITY_5_3_OR_NEWER
            var newLevel = SceneManager.GetActiveScene().buildIndex - 1;
            if (newLevel == 0) newLevel = SceneManager.sceneCountInBuildSettings - 1;
            SceneManager.LoadScene(newLevel);
#else
			var newLevel = Application.loadedLevel - 1;
			if (newLevel == 0) newLevel = Application.levelCount - 1;
			Application.LoadLevel(newLevel);
#endif
        }

        private void Start()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            layer = GetComponent<TouchLayer>();

#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets("t:Scene", new string[] {"Assets/TouchScript/Examples"});
            if (EditorBuildSettings.scenes.Length != guids.Length)
            {
                if (EditorUtility.DisplayDialog("Add Example Scenes to Build Settings?",
                    "You are running Examples scene but example scenes are not added to Build Settings. Do you want to add them now?", "Yes", "No"))
                {
                    var importers = Array.ConvertAll(guids, (string guid) => AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
                    Array.Sort(importers, (AssetImporter a, AssetImporter b) =>
                    {
                        var i1 = string.IsNullOrEmpty(a.userData) ? 42 : Convert.ToInt32(a.userData);
                        var i2 = string.IsNullOrEmpty(b.userData) ? 42 : Convert.ToInt32(b.userData);
                        if (i1 == i2) return 0;
                        return i1 - i2;
                    });
                    EditorBuildSettings.scenes = Array.ConvertAll(importers, (AssetImporter i) => new EditorBuildSettingsScene(i.assetPath, true));
                    EditorUtility.DisplayDialog("Success", "Example scenes were added to Build Settings. Please restart Play Mode.", "OK");
                }
            }
#endif

#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded += sceneLoadedHandler;
#endif

#if UNITY_5_3_OR_NEWER
            if (SceneManager.GetActiveScene().name == "Examples" && SceneManager.sceneCountInBuildSettings > 1)
#else
			if (Application.loadedLevelName == "Examples" && Application.levelCount > 1)
#endif
            {
                LoadNextLevel();
            }
        }

        private void OnDestroy()
        {
#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded -= sceneLoadedHandler;
#endif
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        }

#if UNITY_5_4_OR_NEWER
        private void sceneLoadedHandler(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(resetUILayer());
        }
#else
        private void OnLevelWasLoaded(int num)
        {
			StartCoroutine(resetUILayer());
        }
#endif

        private IEnumerator resetUILayer()
        {
            yield return new WaitForEndOfFrame();
            LayerManager.Instance.AddLayer(layer, 0);
        }
    }
}