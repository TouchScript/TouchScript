/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Layers;
using UnityEngine.SceneManagement;

namespace TouchScript.Examples
{
    public class Runner : MonoBehaviour
    {
        private static Runner instance;
        private UILayer layer;

        public void LoadNextLevel()
        {
            SceneManager.LoadScene((SceneManager.GetActiveScene ().buildIndex + 1) % SceneManager.sceneCountInBuildSettings);
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            layer = GetComponent<UILayer>();

            if (SceneManager.GetActiveScene ().name == "Examples" && SceneManager.sceneCountInBuildSettings > 1)
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