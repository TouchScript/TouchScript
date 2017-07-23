/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using TouchScript.Debugging;
using TouchScript.Debugging.Filters;
using TouchScript.Debugging.GL;
using TouchScript.Debugging.Loggers;
using TouchScript.Editor.EditorUI;
using TouchScript.Utils;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Debugging
{
    public class PointerDebuggerWindow : EditorWindow
    {
        private class Styles : IDisposable
        {
            public Texture2D BG;

            public int Padding = 5;
            public int GlobalPadding = 10;

            public int TabHeight = 20;
            public int TabWidth = 80;
            public int TabPadding = 10;

            public int TopWindowHeight = 240;
            public int RefreshHeight = 30;

            public int PointerItemHeight = 22;
            public Color PointerItemSelected = new Color(.86f, .86f, .86f, 1f);
            public Color PointerItemEmpty = new Color(.7f, .7f, .7f, .2f);

            public GUIStyle PointerItemStyle;
            public GUIStyle EnterPlayModeText;
            public GUIStyle SmallText;
            public GUIStyle SmallButton;
            public GUIStyle FilterToggle;

            public Styles()
            {
                BG = CreateColorTexture(new Color(0, 0, 0, 0.05f));

                PointerItemStyle = new GUIStyle("ShurikenModuleTitle")
                {
                    font = (new GUIStyle("Label")).font,
                    border = new RectOffset(15, 7, 4, 4),
                    fixedHeight = 22,
                    contentOffset = new Vector2(20f, -2f),
                };

                EnterPlayModeText = new GUIStyle("miniLabel")
                {
                    alignment = TextAnchor.MiddleCenter,
                };

                SmallText = new GUIStyle("miniLabel")
                {
                    alignment = TextAnchor.UpperLeft,
                };

                SmallButton = new GUIStyle("Button")
                {
                    fontSize = SmallText.fontSize,
                    fontStyle = SmallText.fontStyle,
                    font = SmallText.font,
                };

                FilterToggle = new GUIStyle("ShurikenToggle")
                {
                    fontSize = SmallText.fontSize,
                    fontStyle = SmallText.fontStyle,
                    font = SmallText.font,
                };
                FilterToggle.normal.textColor = SmallText.normal.textColor;
                FilterToggle.onNormal.textColor = SmallText.normal.textColor;
            }

            public void Dispose()
            {
                DestroyImmediate(BG);
            }

            public static Texture2D CreateColorTexture(Color color)
            {
                var texture = new Texture2D(1, 1);
                texture.hideFlags = HideFlags.HideAndDontSave;
                texture.name = "Color " + color;
                texture.SetPixel(0, 0, color);
                texture.Apply();
                return texture;
            }
        }

        public enum LogType
        {
            Editor,
            File
        }

        // sec
        private const float UPDATE_INTERVAL = 1f;

        private enum Tab
        {
            Pointers,
            Event,
            Filters
        }

        [MenuItem("Window/TouchScript/Debug", false, 0)]
        static void createWindow()
        {
            EditorWindow window = GetWindow<PointerDebuggerWindow>(false, "TSDebugger", true);
            window.minSize = new Vector2(300, 600);

            window.Show();
        }

        private Styles styles;

        private LogType logType;
        private IPointerLogger pLogger;
        private PointerVisualizer pointerVisualizer;
        private PagedList pointerList;
        private PagedList eventList;

        [NonSerialized]
        private Tab activeTab;

        [NonSerialized]
        private int pointerDataCount = 0;

        [NonSerialized]
        private List<PointerData> pointerData = new List<PointerData>();

        [NonSerialized]
        private List<string> pointerStrings = new List<string>();

        [NonSerialized]
        private List<PointerLog> pointerEvents = new List<PointerLog>();

        [NonSerialized]
        private PointerLog selectedEvent;

        [NonSerialized]
        private int selectedEventId = -1;

        [NonSerialized]
        private Dictionary<int, string> pointerEventStrings = new Dictionary<int, string>();

        [NonSerialized]
        private PointerLogFilter logFilter;

        private FilterState filterState;
        //private Vector2 filterScroll;

        private bool autoRefresh = true;

        [NonSerialized]
        private float refreshTime;

        private void OnEnable()
        {
            setupLogging();
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                setupPlaymodeLogging();

            if (filterState == null)
            {
                filterState = new FilterState();
                filterState.Load();
            }

            EditorApplication.update += updateHandler;
        }

        private void OnDisable()
        {
            if (styles != null) styles.Dispose();

            EditorApplication.update -= updateHandler;
        }

        private void updateHandler()
        {
            if (pLogger == null) return;

            if (pLogger.PointerCount != pointerDataCount)
            {
                updatePointers();
            }
            if (autoRefresh)
            {
                var time = Time.unscaledTime;
                if (time > refreshTime)
                {
                    refreshTime = time + UPDATE_INTERVAL;
                    updateEventList();
                }
            }
        }

        #region Init

        private void setupPlaymodeLogging()
        {
            TouchScriptDebugger.Instance.PointerLogger = pLogger = new PointerLogger();
        }

        private void setupLogging()
        {
            pointerVisualizer = new PointerVisualizer();
            pointerList = new PagedList(22, drawPointerItem, pointerSelectionChangeHandler);
            eventList = new PagedList(22, drawEventItem, eventSelectionChangeHandler);
            logFilter = new PointerLogFilter();
        }

        private void loadLogFile()
        {
            var path = EditorUtility.OpenFilePanel("Load log data", Application.dataPath, "bin");
            if (string.IsNullOrEmpty(path)) return;
            pLogger = new FileReaderLogger(path);
            updatePointers();
        }

        private void updateLogType(LogType type)
        {
            logType = type;

            if (type == LogType.Editor)
            {
                if (pLogger != null) pLogger.Dispose();
                if (EditorApplication.isPlayingOrWillChangePlaymode) setupPlaymodeLogging();
            }
            else
            {
                TouchScriptDebugger.Instance.ClearPointerLogger();
            }
        }

        #endregion

        #region Update

        private void updatePointers()
        {
            pointerData = pLogger.GetFilteredPointerData();
            pointerList.Count = pointerData.Count;
            pointerDataCount = pointerData.Count;

            pointerStrings.Clear();
            foreach (var data in pointerData)
            {
                pointerStrings.Add(string.Format("{0} (id: {1})", data.Type, data.Id));
            }

            Repaint();
        }

        private void updateEventList()
        {
            if (pointerList.SelectedId == -1)
            {
                pointerEvents.Clear();
                eventList.Count = 0;
            }
            else
            {
                var id = pointerData[pointerList.SelectedId].Id;
                syncFilter();
                pointerEvents = pLogger.GetFilteredLogsForPointer(id, logFilter);
                eventList.Count = pointerEvents.Count;
            }

            Repaint();
        }

        private void selectPointer()
        {
            updateEventList();
            pointerVisualizer.Hide();
        }

        private void selectEvent()
        {
            if (eventList.SelectedId == -1)
            {
                pointerVisualizer.Hide();
                selectedEventId = -1;
                return;
            }

            selectedEventId = eventList.SelectedId;
            selectedEvent = pointerEvents[selectedEventId];
            pointerVisualizer.Show(selectedEvent.State.Position);
            switchTab(Tab.Event);
        }

        private void syncFilter()
        {
            logFilter.EventMask = filterState.PointerEventMask;
        }

        private string getEventString(int id)
        {
            var evt = pointerEvents[id];
            string str = null;
            if (!pointerEventStrings.TryGetValue(evt.Id, out str))
            {
                DateTime time = new DateTime(evt.Tick);
                str = string.Format("{0} > {1}", time.ToString("HH:mm:ss.ffffff"), evt.Event);
                pointerEventStrings.Add(evt.Id, str);
            }
            return str;
        }

        #endregion

        #region Misc

        private void switchTab(Tab newTab)
        {
            activeTab = newTab;

            //if (activeTab == Tab.Filters)
            //{
            //    filterScroll = Vector2.zero;
            //}

            Repaint();
        }

        #endregion

        #region Drawing

        private void OnGUI()
        {
            if (styles == null) styles = new Styles();

            int height = styles.TopWindowHeight;
            //int height = pointerList.FitHeight(10);

            var rect = GUIUtils.GetPaddedRect(height + styles.GlobalPadding * 2, styles.Padding);

            GUI.DrawTexture(rect, styles.BG);
            GUIUtils.ContractRect(ref rect, styles.GlobalPadding);

            switch (activeTab)
            {
                case Tab.Pointers:
                    if (pointerData.Count == 0)
                        drawNoData(rect);
                    else
                        pointerList.Draw(rect);
                    break;
                case Tab.Event:
                    if (selectedEventId == -1)
                        drawNoData(rect);
                    else
                        drawSelectedEvent(rect);
                    break;
                case Tab.Filters:
                    drawFilters(rect);
                    break;
            }

            drawTabs();
            drawRefresh();

            //eventList.Count = 100;
            rect = GUIUtils.GetPaddedRect(0, styles.Padding, true);

            GUI.DrawTexture(rect, styles.BG);
            GUIUtils.ContractRect(ref rect, styles.GlobalPadding);

            if (pointerEvents.Count == 0)
                drawNoData(rect);
            else
                eventList.Draw(rect);
        }

        private void drawFilters(Rect rect)
        {
            //GUI.Toggle(rect, true, "     Test", styles.FilterToggle);

            GUI.Label(rect, "Show pointer events:");

            rect.y += 20;
            rect.height -= 20;
            var scrollRect = new Rect(rect);
            scrollRect.height *= 2;
            scrollRect.width -= 40;
            //scrollRect.x = 0;
            //scrollRect.y = 0;

            //using (var scope = new GUI.ScrollViewScope(rect, filterScroll, scrollRect))
            //{
            scrollRect.height = 14;
            var names = Enum.GetNames(typeof (PointerEvent));
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                for (var i = 1; i < names.Length; i++)
                {
                    var evt = (PointerEvent) i;
                    filterState.SetEventValue(evt,
                        GUI.Toggle(scrollRect, filterState.IsEventEnabled(evt), "     " + names[i], styles.FilterToggle));
                    scrollRect.y += scrollRect.height;
                }
                if (changeScope.changed) filterState.Save();
            }
            //    filterScroll = scope.scrollPosition;
            //}

            scrollRect.y += 4;
            scrollRect.height = 20;
            using (var scope = new EditorGUI.DisabledScope(pointerList.SelectedId == -1))
            {
                if (GUI.Button(scrollRect, "Apply filter"))
                {
                    updateEventList();
                }
            }
        }

        private void drawTabs()
        {
            var rect = GUILayoutUtility.GetRect(0, styles.TabHeight, GUILayout.ExpandWidth(true));
            rect.x += styles.Padding;
            rect.y -= styles.Padding;

            rect.width = styles.TabWidth;
            if (drawTab(rect, "Pointers", activeTab == Tab.Pointers))
                activeTab = Tab.Pointers;
            rect.x += rect.width;
            if (drawTab(rect, "Event", activeTab == Tab.Event))
                activeTab = Tab.Event;
            rect.x += rect.width;
            if (drawTab(rect, "Filters", activeTab == Tab.Filters))
                activeTab = Tab.Filters;
        }

        private void drawRefresh()
        {
            var rect = GUILayoutUtility.GetRect(0, styles.RefreshHeight, GUILayout.ExpandWidth(true));
            GUIUtils.ContractRect(ref rect, styles.Padding);

            var sourceRect = new Rect(rect);
            sourceRect.width = 50;
            GUI.Label(sourceRect, "  Source", styles.SmallText);
            sourceRect.x += sourceRect.width;
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                logType = (LogType) EditorGUI.EnumPopup(sourceRect, "", logType);
                if (scope.changed) updateLogType(logType);
            }

            if (logType == LogType.File)
            {
                sourceRect.x += sourceRect.width + 2;
                sourceRect.width = 40;
                sourceRect.height = 15;
                if (GUI.Button(sourceRect, "Load", styles.SmallButton))
                {
                    loadLogFile();
                }
            }

            var refreshRect = new Rect(rect);
            refreshRect.x = refreshRect.width - 50 - 60;
            refreshRect.width = 50;
            autoRefresh = GUI.Toggle(refreshRect, autoRefresh, "     Auto", styles.FilterToggle);

            using (var scope = new EditorGUI.DisabledScope(autoRefresh))
            {
                rect.x = rect.width - 60;
                rect.width = 60;
                rect.height = 15;
                rect.y -= 1;
                if (GUI.Button(rect, "Refresh", styles.SmallButton))
                {
                    updateEventList();
                }
            }
        }

        private void drawSelectedEvent(Rect rect)
        {
            if (selectedEvent.Id == -1)
            {
                GUI.Label(rect, "No event selected.", styles.EnterPlayModeText);
                return;
            }

            var transform = selectedEvent.State.Target;
            var path = selectedEvent.State.TargetPath;

            GUI.Label(rect, string.Format("{0}\nPosition: {1}\nPrevious: {2}\nFlags: {3}, Buttons: {4}",
                getEventString(selectedEventId), selectedEvent.State.Position,
                selectedEvent.State.PreviousPosition, selectedEvent.State.Flags,
                PointerUtils.ButtonsToString(selectedEvent.State.Buttons)));
            rect.y += 64;
            rect.height = 20;
            GUI.Label(rect, "Target: ");
            using (var scope = new EditorGUI.DisabledScope(true))
            {
                var fieldRect = new Rect(rect);
                fieldRect.x += 50;
                fieldRect.width -= 50;
                EditorGUI.ObjectField(fieldRect, transform, typeof (Transform), true);
            }

            if (path != null)
            {
                rect.y += 20;
                rect.height = 16;
                GUI.Label(rect, path, styles.SmallText);
            }
        }

        private bool drawTab(Rect rect, string content, bool selected)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        Event.current.Use();
                        return true;
                    }
                    break;
                case EventType.Layout:
                case EventType.Repaint:
                    if (selected) GUI.DrawTexture(rect, styles.BG);
                    rect.x += styles.TabPadding;
                    GUI.Label(rect, content);
                    break;
            }

            return false;
        }

        private void drawNoData(Rect rect)
        {
            GUI.Label(rect, "No data available.", styles.EnterPlayModeText);
        }

        private void drawPlaymodeText(Rect rect)
        {
            GUI.Label(rect, "Data is only available in Play Mode.", styles.EnterPlayModeText);
        }

        private void drawPointerItem(int id, Rect rect, bool selected)
        {
            var bg = GUI.backgroundColor;
            if (id == -1)
            {
                GUI.backgroundColor = styles.PointerItemEmpty;
                GUI.Box(rect, "", styles.PointerItemStyle);
                GUI.backgroundColor = bg;
                return;
            }

            if (selected)
            {
                GUI.backgroundColor = styles.PointerItemSelected;
            }

            GUI.Box(rect, pointerStrings[id], styles.PointerItemStyle);
            GUI.backgroundColor = bg;
        }

        private void drawEventItem(int id, Rect rect, bool selected)
        {
            var bg = GUI.backgroundColor;
            if (id == -1)
            {
                GUI.backgroundColor = styles.PointerItemEmpty;
                GUI.Box(rect, "", styles.PointerItemStyle);
                GUI.backgroundColor = bg;
                return;
            }

            if (selected)
            {
                GUI.backgroundColor = styles.PointerItemSelected;
            }

            GUI.Box(rect, getEventString(id), styles.PointerItemStyle);
            GUI.backgroundColor = bg;
        }

        #endregion

        #region List handlers

        private void pointerSelectionChangeHandler(int id)
        {
            selectPointer();
        }

        private void eventSelectionChangeHandler(int id)
        {
            selectEvent();
        }

        #endregion

        private class PointerVisualizer
        {
            private int currentDebugId = -1;

            public PointerVisualizer() {}

            public void Show(Vector2 position)
            {
                if (!Application.isPlaying) return;

                if (currentDebugId != -1) Hide();
                currentDebugId = GLDebug.DrawSquareScreenSpace(position, 0, Vector2.one * 20, GLDebug.MULTIPLY, float.MaxValue);
            }

            public void Hide()
            {
                if (!Application.isPlaying) return;

                GLDebug.RemoveFigure(currentDebugId);
                currentDebugId = -1;
            }
        }

        [Serializable]
        private class FilterState : ISerializationCallbackReceiver
        {
            private const string KEY = "TouchScript:Debugger:FilterState";

            [SerializeField]
            private List<bool> pointerEvents;

            public uint PointerEventMask
            {
                get { return BinaryUtils.ToBinaryMask(pointerEvents); }
            }

            public FilterState()
            {
                var eventsCount = Enum.GetValues(typeof (PointerEvent)).Length;
                pointerEvents = new List<bool>(eventsCount);
                syncPointerEvents(eventsCount);
            }

            public bool IsEventEnabled(PointerEvent evt)
            {
                var id = (int) evt;
                if (id >= pointerEvents.Count) return false;
                return pointerEvents[id];
            }

            public void SetEventValue(PointerEvent evt, bool value)
            {
                pointerEvents[(int) evt] = value;
            }

            public void Save()
            {
                var json = JsonUtility.ToJson(this);
                EditorPrefs.SetString(KEY, json);
            }

            public void Load()
            {
                if (!EditorPrefs.HasKey(KEY)) return;
                var json = EditorPrefs.GetString(KEY);
                JsonUtility.FromJsonOverwrite(json, this);
            }

            private void syncPointerEvents(int count)
            {
                for (var i = pointerEvents.Count; i < count; i++) pointerEvents.Add(true);
            }

            public void OnBeforeSerialize() {}

            public void OnAfterDeserialize()
            {
                var eventsCount = Enum.GetValues(typeof (PointerEvent)).Length;
                if (pointerEvents.Count != eventsCount)
                {
                    Debug.Log("FilterState serialization error!");
                    if (pointerEvents.Count > eventsCount)
                    {
                        pointerEvents = new List<bool>(eventsCount);
                    }
                    syncPointerEvents(eventsCount);
                }
            }
        }
    }
}

#endif