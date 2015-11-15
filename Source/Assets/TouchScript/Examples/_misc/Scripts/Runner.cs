using UnityEngine;

namespace TouchScript.Examples
{
	public class Runner : MonoBehaviour
	{
		private static Runner instance;

		public void LoadNextLevel()
		{
			Application.LoadLevel((Application.loadedLevel + 1)%Application.levelCount);
		}

	    private void Awake()
	    {
	        if (instance == null)
	        {
	            instance = this;
	            DontDestroyOnLoad(gameObject);
	        }

	        if (Application.loadedLevelName == "Examples" && Application.levelCount > 1)
	        {
				LoadNextLevel();
	        }
	    }
	}
}