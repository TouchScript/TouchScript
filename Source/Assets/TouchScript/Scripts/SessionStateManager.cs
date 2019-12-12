using TouchScript.Core;
using TouchScript.Layers.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace TouchScript
{
    /// <summary>
    /// Manages state that is valid only for single play mode session.
    /// </summary>
    static class SessionStateManager
    {
        public static TouchManagerInstance TouchManagerInstance => _sessionState.TouchManager;
        public static ILayerManager LayerManager => _sessionState.LayerManager;
        public static IGestureManager GestureManager => _sessionState.GestureManager;
        public static TouchScriptInputModule TouchScriptInputModule => _sessionState.TouchScriptInputModule;


        static SessionState _sessionStateBacking;

        static SessionState _sessionState
        {
            get
            {
                if (!Application.isPlaying) return null;
                if (_sessionStateBacking == null || _sessionStateBacking.PlayModeSession != _playModeSession)
                {
                    var root = new GameObject("TouchScript");
                    Object.DontDestroyOnLoad(root);
                    _sessionStateBacking = new SessionState(_playModeSession, root);
                }
                return _sessionStateBacking;
            }
        }

        static int _playModeSession;


#if UNITY_EDITOR
        static SessionStateManager()
        {
            UnityEditor.EditorApplication.playModeStateChanged += playModeStateChange =>
            {
                if (playModeStateChange == UnityEditor.PlayModeStateChange.ExitingEditMode)
                    _playModeSession++;
                else if (playModeStateChange == UnityEditor.PlayModeStateChange.EnteredEditMode)
                    _sessionStateBacking = null;
            };
        }
#endif


        class SessionState
        {
            public readonly int PlayModeSession;

            readonly GameObject _root;
            TouchManagerInstance _touchManager;
            LayerManagerInstance _layerManager;
            GestureManagerInstance _gestureManager;
            TouchScriptInputModule _touchScriptInputModule;

            public TouchManagerInstance TouchManager
            {
                get
                {
                    if (!ReferenceEquals(_touchManager, null)) return _touchManager;
                    if (_root.TryGetComponent<TouchManagerInstance>(out var touchManager))
                        return _touchManager = touchManager;
                    return _touchManager = _root.AddComponent<TouchManagerInstance>();
                }
            }

            public LayerManagerInstance LayerManager
            {
                get
                {
                    if (!ReferenceEquals(_layerManager, null)) return _layerManager;
                    if (_root.TryGetComponent<LayerManagerInstance>(out var layerManager))
                        return _layerManager = layerManager;
                    return _layerManager = _root.AddComponent<LayerManagerInstance>();
                }
            }

            public GestureManagerInstance GestureManager
            {
                get
                {
                    if (!ReferenceEquals(_gestureManager, null)) return _gestureManager;
                    if (_root.TryGetComponent<GestureManagerInstance>(out var gestureManager))
                        return _gestureManager = gestureManager;
                    return _gestureManager = _root.AddComponent<GestureManagerInstance>();
                }
            }

            public TouchScriptInputModule TouchScriptInputModule => _touchScriptInputModule != null
                ? _touchScriptInputModule : _touchScriptInputModule = ResolveInputModule();

            public SessionState(int playModeSession, GameObject root)
            {
                PlayModeSession = playModeSession;
                _root = root;
            }

            static TouchScriptInputModule ResolveInputModule()
            {
                var es = EventSystem.current;
                if (es == null)
                {
                    var type = typeof(EventSystem);
                    var objects = Object.FindObjectsOfType(type);
                    if (objects.Length != 0) es = (EventSystem) objects[0];
                    else
                    {
                        es = new GameObject(type.Name, typeof(EventSystem))
                            .GetComponent<EventSystem>();
                    }
                }

                var instance = es.GetComponent<TouchScriptInputModule>();
                if (instance == null) instance = es.gameObject.AddComponent<TouchScriptInputModule>();
                return instance;
            }
        }
    }
}