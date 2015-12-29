/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
using System;
using System.Collections.Generic;
using TUIOsharp;
using TUIOsharp.DataProcessors;
using TUIOsharp.Entities;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Processes TUIO 1.1 input.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/TUIO Input")]
    [HelpURL("http://touchscript.github.io/docs/Index.html?topic=html/T_TouchScript_InputSources_TuioInput.htm")]
    public sealed class TuioInput : InputSource
    {
        #region Constants

        /// <summary>
        /// TUIO tag used for touches.
        /// </summary>
        public const string SOURCE_TUIO = "TUIO";

        /// <summary>
        /// Type of TUIO input object.
        /// </summary>
        [Flags]
        public enum InputType
        {
            /// <summary>
            /// Touch/pointer.
            /// </summary>
            Cursors = 1 << 0,

            /// <summary>
            /// Shape.
            /// </summary>
            Blobs = 1 << 1,

            /// <summary>
            /// Tagged object.
            /// </summary>
            Objects = 1 << 2
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Port to listen to.
        /// </summary>
        public int TuioPort
        {
            get { return tuioPort; }
            set
            {
                if (tuioPort == value) return;
                tuioPort = value;
                connect();
            }
        }

        /// <summary>
        /// What input types should the input source listen to.
        /// </summary>
        public InputType SupportedInputs
        {
            get { return supportedInputs; }
            set
            {
                if (supportedInputs == value) return;
                supportedInputs = value;
                updateInputs();
            }
        }

        /// <summary>
        /// List of TUIO object ids to tag mappings.
        /// </summary>
        public IList<TuioObjectMapping> TuioObjectMappings
        {
            get { return tuioObjectMappings; }
        }

        /// <summary>
        /// Tags for new cursors.
        /// </summary>
        public Tags CursorTags
        {
            get { return cursorTags; }
        }

        /// <summary>
        /// Tags for new blobs.
        /// </summary>
        public Tags BlobTags
        {
            get { return blobTags; }
        }

        /// <summary>
        /// Tags for new objects.
        /// </summary>
        public Tags ObjectTags
        {
            get { return objectTags; }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private int tuioPort = 3333;

        [SerializeField]
        private InputType supportedInputs = InputType.Cursors | InputType.Blobs | InputType.Objects;

        [SerializeField]
        private List<TuioObjectMapping> tuioObjectMappings = new List<TuioObjectMapping>();

        [SerializeField]
        private Tags cursorTags = new Tags(SOURCE_TUIO, Tags.INPUT_TOUCH);

        [SerializeField]
        private Tags blobTags = new Tags(SOURCE_TUIO, Tags.INPUT_TOUCH);

        [SerializeField]
        private Tags objectTags = new Tags(SOURCE_TUIO, Tags.INPUT_OBJECT);

        private TuioServer server;
        private CursorProcessor cursorProcessor;
        private ObjectProcessor objectProcessor;
        private BlobProcessor blobProcessor;

        private Dictionary<TuioCursor, TouchPoint> cursorToInternalId = new Dictionary<TuioCursor, TouchPoint>();
        private Dictionary<TuioBlob, TouchPoint> blobToInternalId = new Dictionary<TuioBlob, TouchPoint>();
        private Dictionary<TuioObject, TouchPoint> objectToInternalId = new Dictionary<TuioObject, TouchPoint>();
        private int screenWidth;
        private int screenHeight;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override void UpdateInput()
        {
            base.UpdateInput();
            screenWidth = Screen.width;
            screenHeight = Screen.height;
        }

        /// <inheritdoc />
        public override void CancelTouch(TouchPoint touch, bool @return)
        {
            base.CancelTouch(touch, @return);
            lock (this)
            {
                TuioCursor cursor = null;
                foreach (var touchPoint in cursorToInternalId)
                {
                    if (touchPoint.Value.Id == touch.Id)
                    {
                        cursor = touchPoint.Key;
                        break;
                    }
                }
                if (cursor != null)
                {
                    cancelTouch(touch.Id);
                    if (@return)
                    {
                        cursorToInternalId[cursor] = beginTouch(touch.Position, touch.Tags, false);
                    }
                    else
                    {
                        cursorToInternalId.Remove(cursor);
                    }
                    return;
                }

                TuioBlob blob = null;
                foreach (var touchPoint in blobToInternalId)
                {
                    if (touchPoint.Value.Id == touch.Id)
                    {
                        blob = touchPoint.Key;
                        break;
                    }
                }
                if (blob != null)
                {
                    cancelTouch(touch.Id);
                    if (@return)
                    {
                        var t = beginTouch(touch.Position, touch.Tags, false);
                        t.Properties = touch.Properties;
                        blobToInternalId[blob] = t;
                    }
                    else
                    {
                        blobToInternalId.Remove(blob);
                    }
                    return;
                }

                TuioObject obj = null;
                foreach (var touchPoint in objectToInternalId)
                {
                    if (touchPoint.Value.Id == touch.Id)
                    {
                        obj = touchPoint.Key;
                        break;
                    }
                }
                if (obj != null)
                {
                    cancelTouch(touch.Id);
                    if (@return)
                    {
                        var t = beginTouch(touch.Position, touch.Tags, false);
                        t.Properties = touch.Properties;
                        objectToInternalId[obj] = t;
                    }
                    else
                    {
                        objectToInternalId.Remove(obj);
                    }
                }
            }
        }

        #endregion

        #region Unity

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            screenWidth = Screen.width;
            screenHeight = Screen.height;

            cursorProcessor = new CursorProcessor();
            cursorProcessor.CursorAdded += OnCursorAdded;
            cursorProcessor.CursorUpdated += OnCursorUpdated;
            cursorProcessor.CursorRemoved += OnCursorRemoved;

            blobProcessor = new BlobProcessor();
            blobProcessor.BlobAdded += OnBlobAdded;
            blobProcessor.BlobUpdated += OnBlobUpdated;
            blobProcessor.BlobRemoved += OnBlobRemoved;

            objectProcessor = new ObjectProcessor();
            objectProcessor.ObjectAdded += OnObjectAdded;
            objectProcessor.ObjectUpdated += OnObjectUpdated;
            objectProcessor.ObjectRemoved += OnObjectRemoved;

            connect();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            disconnect();
            base.OnDisable();
        }

        #endregion

        #region Private functions

        private void connect()
        {
            if (!Application.isPlaying) return;
            if (server != null) disconnect();

            server = new TuioServer(TuioPort);
            server.Connect();
            updateInputs();
        }

        private void disconnect()
        {
            if (server != null)
            {
                server.RemoveAllDataProcessors();
                server.Disconnect();
                server = null;
            }

            foreach (var i in cursorToInternalId) cancelTouch(i.Value.Id);
            foreach (var i in blobToInternalId) cancelTouch(i.Value.Id);
            foreach (var i in objectToInternalId) cancelTouch(i.Value.Id);
        }

        private void updateInputs()
        {
            if (server == null) return;

            if ((supportedInputs & InputType.Cursors) != 0) server.AddDataProcessor(cursorProcessor);
            else server.RemoveDataProcessor(cursorProcessor);
            if ((supportedInputs & InputType.Blobs) != 0) server.AddDataProcessor(blobProcessor);
            else server.RemoveDataProcessor(blobProcessor);
            if ((supportedInputs & InputType.Objects) != 0) server.AddDataProcessor(objectProcessor);
            else server.RemoveDataProcessor(objectProcessor);
        }

        private void updateBlobProperties(TouchPoint touch, TuioBlob blob)
        {
            var props = touch.Properties;

            props["Angle"] = blob.Angle;
            props["Width"] = blob.Width;
            props["Height"] = blob.Height;
            props["Area"] = blob.Area;
            props["RotationVelocity"] = blob.RotationVelocity;
            props["RotationAcceleration"] = blob.RotationAcceleration;
        }

        private void updateObjectProperties(TouchPoint touch, TuioObject obj)
        {
            var props = touch.Properties;

            props["Angle"] = obj.Angle;
            props["ObjectId"] = obj.ClassId;
            props["RotationVelocity"] = obj.RotationVelocity;
            props["RotationAcceleration"] = obj.RotationAcceleration;
        }

        private string getTagById(int id)
        {
            var count = TuioObjectMappings.Count;
            for (var i = 0; i < count; i++)
            {
                var tuioObjectMapping = tuioObjectMappings[i];
                if (tuioObjectMapping.Id == id) return tuioObjectMapping.Tag;
            }
            return null;
        }

        #endregion

        #region Event handlers

        private void OnCursorAdded(object sender, TuioCursorEventArgs e)
        {
            var entity = e.Cursor;
            lock (this)
            {
                var x = entity.X * screenWidth;
                var y = (1 - entity.Y) * screenHeight;
                cursorToInternalId.Add(entity, beginTouch(new Vector2(x, y), CursorTags));
            }
        }

        private void OnCursorUpdated(object sender, TuioCursorEventArgs e)
        {
            var entity = e.Cursor;
            lock (this)
            {
                TouchPoint touch;
                if (!cursorToInternalId.TryGetValue(entity, out touch)) return;

                var x = entity.X * screenWidth;
                var y = (1 - entity.Y) * screenHeight;

                moveTouch(touch.Id, new Vector2(x, y));
            }
        }

        private void OnCursorRemoved(object sender, TuioCursorEventArgs e)
        {
            var entity = e.Cursor;
            lock (this)
            {
                TouchPoint touch;
                if (!cursorToInternalId.TryGetValue(entity, out touch)) return;

                cursorToInternalId.Remove(entity);
                endTouch(touch.Id);
            }
        }

        private void OnBlobAdded(object sender, TuioBlobEventArgs e)
        {
            var entity = e.Blob;
            lock (this)
            {
                var x = entity.X * screenWidth;
                var y = (1 - entity.Y) * screenHeight;
                var touch = beginTouch(new Vector2(x, y), BlobTags);
                updateBlobProperties(touch, entity);
                blobToInternalId.Add(entity, touch);
            }
        }

        private void OnBlobUpdated(object sender, TuioBlobEventArgs e)
        {
            var entity = e.Blob;
            lock (this)
            {
                TouchPoint touch;
                if (!blobToInternalId.TryGetValue(entity, out touch)) return;

                var x = entity.X * screenWidth;
                var y = (1 - entity.Y) * screenHeight;

                moveTouch(touch.Id, new Vector2(x, y));
                updateBlobProperties(touch, entity);
            }
        }

        private void OnBlobRemoved(object sender, TuioBlobEventArgs e)
        {
            var entity = e.Blob;
            lock (this)
            {
                TouchPoint touch;
                if (!blobToInternalId.TryGetValue(entity, out touch)) return;

                blobToInternalId.Remove(entity);
                endTouch(touch.Id);
            }
        }

        private void OnObjectAdded(object sender, TuioObjectEventArgs e)
        {
            var entity = e.Object;
            lock (this)
            {
                var x = entity.X * screenWidth;
                var y = (1 - entity.Y) * screenHeight;
                var touch = beginTouch(new Vector2(x, y), new Tags(ObjectTags, getTagById(entity.ClassId)));
                updateObjectProperties(touch, entity);
                objectToInternalId.Add(entity, touch);
            }
        }

        private void OnObjectUpdated(object sender, TuioObjectEventArgs e)
        {
            var entity = e.Object;
            lock (this)
            {
                TouchPoint touch;
                if (!objectToInternalId.TryGetValue(entity, out touch)) return;

                var x = entity.X * screenWidth;
                var y = (1 - entity.Y) * screenHeight;

                moveTouch(touch.Id, new Vector2(x, y));
                updateObjectProperties(touch, entity);
            }
        }

        private void OnObjectRemoved(object sender, TuioObjectEventArgs e)
        {
            var entity = e.Object;
            lock (this)
            {
                TouchPoint touch;
                if (!objectToInternalId.TryGetValue(entity, out touch)) return;

                objectToInternalId.Remove(entity);
                endTouch(touch.Id);
            }
        }

        #endregion
    }

    /// <summary>
    /// TUIO object id to tag mapping value object.
    /// </summary>
    [Serializable]
    public class TuioObjectMapping
    {
        /// <summary>
        /// TUIO object id.
        /// </summary>
        public int Id;

        /// <summary>
        /// Tag to attach to this object.
        /// </summary>
        public string Tag;
    }
}

#endif