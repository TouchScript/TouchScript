using UnityEngine;

public class Examples : MonoBehaviour
{

    private static Examples instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (Application.loadedLevelName == "_Examples" && Application.levelCount > 1)
        {
            loadNextLevel();
            Destroy(GetComponent<Example>());
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 120, 20, 100, 30), "Next >>"))
        {
            loadNextLevel();
        }
    }

    private void loadNextLevel()
    {
        Application.LoadLevel((Application.loadedLevel + 1)%Application.levelCount);
    }

}