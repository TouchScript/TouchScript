/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Layers;
#if UNITY_5_3
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
#if UNITY_5_3
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

#if UNITY_5_3
            if (SceneManager.GetActiveScene().name == "Examples" && SceneManager.sceneCountInBuildSettings > 1)
#else
			if (Application.loadedLevelName == "Examples" && Application.levelCount > 1)
#endif
            {
                LoadNextLevel();
            }
        }

        private void OnLevelWasLoaded(int num)
        {
            TouchManager.Instance.AddLayer(layer, 0);
        }
    }
}