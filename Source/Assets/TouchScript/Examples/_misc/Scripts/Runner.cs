/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Layers;
using System.Collections;


#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace TouchScript.Examples
{
    /// <summary>
    /// This component loads demo scenes in a loop.
    /// </summary>
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

#if UNITY_5_3_OR_NEWER
            if (SceneManager.GetActiveScene().name == "Examples" && SceneManager.sceneCountInBuildSettings > 1)
#else
			if (Application.loadedLevelName == "Examples" && Application.levelCount > 1)
#endif
            {
                LoadNextLevel();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        }

        private void OnLevelWasLoaded(int num)
        {
			StartCoroutine(resetUILayer());
        }

		private IEnumerator resetUILayer()
		{
			yield return new WaitForEndOfFrame();
			TouchManager.Instance.AddLayer(layer, 0);
		}
    }
}