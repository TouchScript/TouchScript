/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TouchScript.Editor
{

	[InitializeOnLoad]
    class TouchScriptSettingsWindow : EditorWindow
    {

        private const string SHOW_AT_STARTUP = "TouchScript.ShowSettingsOnStartup";

		private const string DEFINE_DEBUG = "TOUCHSCRIPT_DEBUG";
		private const string DEFINE_TUIO = "TOUCHSCRIPT_TUIO";

        private static bool showAtStartup = true;
		private static TouchScriptSettingsWindowSO so;

		private static bool initialized;
		private static int width = 500;
		private static int height = 500;
		private static GUIStyle header;
		private static GUIStyle bold;

		private static Dictionary<string, bool> enabledDefines = new Dictionary<string, bool>() 
		{
			{ DEFINE_DEBUG, false },
			{ DEFINE_TUIO, false },
		};

        [MenuItem("Window/TouchScript/Settings", false, 0)]
        static void createWindow()
        {
            EditorWindow window = GetWindow<TouchScriptSettingsWindow>(true, "TouchScript Settings", true);
            window.maxSize = new Vector2(width, height);
            window.minSize = new Vector2(width, height);

            window.Show();
        }

        static TouchScriptSettingsWindow()
        {
			EditorApplication.update += doShow;
        }

		private static void doShow() 
		{
			EditorApplication.update -= doShow;
			showAtStartup = EditorPrefs.GetBool(SHOW_AT_STARTUP, true);

			if (so == null) 
			{
				var sos = Resources.FindObjectsOfTypeAll<TouchScriptSettingsWindowSO>();
				if (sos.Length > 0) so = sos[0];
			}
			if (so == null)   
			{
				so = ScriptableObject.CreateInstance<TouchScriptSettingsWindowSO>();  
				if (showAtStartup) createWindow();
			}
		}

        private void OnEnable()
        {
			updateEnabledDefines();
        }

		private void OnDisable()
		{
		}

        private void OnGUI()
        {
			init();

			var headerRect = GUILayoutUtility.GetRect(width, 165);
            GUI.Box(headerRect, "v. " + TouchManager.VERSION 
				+ (string.IsNullOrEmpty(TouchManager.VERSION_SUFFIX) ? "" : " " + TouchManager.VERSION_SUFFIX), header);

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(10);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(10);

			EditorGUILayout.LabelField("Thank you for choosing TouchScript!", bold);

			EditorGUI.indentLevel++;
			drawListElement("FAQ", "Some of the questions have been already asked multiple times. \nCheck if yours is in the list.", 
				"https://github.com/TouchScript/TouchScript/wiki/FAQ");
			drawListElement("Documentation", "Complete up-to-date generated docs with all public API annotated.", 
				"http://touchscript.github.io/docs/");
			drawListElement("Official Forum", "Want to ask a question about TouchScript? Use the official Forum.",
				"http://touchprefab.com/index.php");
			drawListElement("Issues", "Found a bug? Feel free to post it in Github Issues.", 
				"https://github.com/TouchScript/TouchScript/issues");
			EditorGUI.indentLevel--;

			EditorGUILayout.LabelField("Options", bold);

			EditorGUI.indentLevel++;
			setDefine(DEFINE_DEBUG, EditorGUILayout.ToggleLeft("Enable Debug Mode", enabledDefines[DEFINE_DEBUG]));
			EditorGUILayout.LabelField("Enables " + DEFINE_DEBUG + " define to turn on some TouchScript debug features.", EditorStyles.miniLabel);
			setDefine(DEFINE_TUIO, EditorGUILayout.ToggleLeft("Enable TUIO", enabledDefines[DEFINE_TUIO]));
			EditorGUILayout.LabelField("Enables " + DEFINE_TUIO + " define, this adds TUIO protocol support.", EditorStyles.miniLabel);

			EditorGUILayout.EndVertical();
			GUILayout.Space(10);
			EditorGUILayout.EndHorizontal();

			drawShowAtStartup();
        }

		private void init()
		{
			if (initialized) return;
			initialized = true;

			header = new GUIStyle();
            header.normal.background = EditorResources.Load<Texture2D>("SettingsWindow/Header.png");
            header.normal.textColor = Color.white;
			header.padding = new RectOffset(0, 70, 102, 0);
			header.alignment = TextAnchor.UpperRight;

			bold = new GUIStyle(EditorStyles.largeLabel);
			bold.fontStyle = FontStyle.Bold;
			bold.fontSize = 18;
			bold.wordWrap = true;
		}

		private void drawListElement(string header, string content, string url)
		{
			GUILayout.BeginVertical();
			EditorGUILayout.LabelField("> " + header, EditorStyles.boldLabel);
			EditorGUILayout.LabelField(content, EditorStyles.wordWrappedLabel);
			GUILayout.EndVertical();

			if (!string.IsNullOrEmpty(url))
			{
				var rect = GUILayoutUtility.GetLastRect();
				EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
				if (Event.current.type == EventType.mouseDown && rect.Contains(Event.current.mousePosition))
					Application.OpenURL(url);
			}
		}

		private void drawShowAtStartup()
		{
			bool show = GUI.Toggle(new Rect(10, height - 24, 120, 30), showAtStartup, "Show at startup");
			if (show != showAtStartup)
			{
				showAtStartup = show;
				EditorPrefs.SetBool(SHOW_AT_STARTUP, showAtStartup);
			}
		}

		private void updateEnabledDefines()
		{
			var defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';'));
			var keys = new List<string>(enabledDefines.Keys);
			foreach (var define in keys)
			{
				if (defines.Contains(define)) enabledDefines[define] = true;
				else enabledDefines[define] = false;
			}
		}

		private void setDefine(string name, bool value)
		{
			if (!enabledDefines.ContainsKey(name)) return;
			if (enabledDefines[name] == value) return;

			enabledDefines[name] = value;
			var defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';'));
			if (value) defines.Add(name);
			else defines.Remove(name);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", defines.ToArray()));
		}

	}

	public class TouchScriptSettingsWindowSO : ScriptableObject
	{
		private void Awake()
		{
			hideFlags = HideFlags.HideAndDontSave;
		}
	}
}
