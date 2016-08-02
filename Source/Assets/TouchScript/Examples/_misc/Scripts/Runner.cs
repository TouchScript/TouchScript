/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Layers;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace TouchScript.Examples
{
    public class Runner : MonoBehaviour
    {
        private static Runner instance;
        private UILayer layer;

        public void LoadNextLevel()
        {
#if UNITY_5_3_OR_NEWER
            SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings);
#else
            Application.LoadLevel((Application.loadedLevel + 1)%Application.levelCount);
#endif
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            layer = GetComponent<UILayer>();

#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded += LevelWasLoaded;
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

#if UNITY_5_4_OR_NEWER
        private void LevelWasLoaded(Scene scene, LoadSceneMode mode)
        {
            TouchManager.Instance.AddLayer(layer, 0);
        }
#else
        private void OnLevelWasLoaded(int value)
        {
            TouchManager.Instance.AddLayer(layer, 0);
        }
#endif
    }
}
