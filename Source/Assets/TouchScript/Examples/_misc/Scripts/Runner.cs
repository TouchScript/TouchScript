/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Layers;
#if UNITY_5_3 || UNITY_5_4
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
#if UNITY_5_3 || UNITY_5_4
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

#if UNITY_5_4
            SceneManager.sceneLoaded += LevelWasLoaded;
#endif

#if UNITY_5_3 || UNITY_5_4
            if (SceneManager.GetActiveScene().name == "Examples" && SceneManager.sceneCountInBuildSettings > 1)
#else
			if (Application.loadedLevelName == "Examples" && Application.levelCount > 1)
#endif
            {
                LoadNextLevel();
            }
        }


#if !UNITY_5_4
        private void OnLevelWasLoaded(int value)
        {
            TouchManager.Instance.AddLayer(layer, 0);
        }
#else
        private void LevelWasLoaded(Scene scene, LoadSceneMode mode)
        {
            TouchManager.Instance.AddLayer(layer, 0);
        }
#endif
    }
}
