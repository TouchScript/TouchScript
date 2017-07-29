/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_TUIO
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
using System;
using System.Collections.Generic;
using TouchScript.Pointers;
using TouchScript.Utils;
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
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_InputSources_TuioInput.htm")]
    public sealed class TuioInput : InputSource
    {
        #region Constants

        /// <summary>
        /// Type of TUIO input object.
        /// </summary>
        [Flags]
        public enum InputType
        {
            /// <summary>
            /// Pointer.
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

        #endregion

        #region Private variables

        [SerializeField]
        private int tuioPort = 3333;

        [SerializeField]
        private InputType supportedInputs = InputType.Cursors | InputType.Blobs | InputType.Objects;

        private TuioServer server;
        private CursorProcessor cursorProcessor;
        private ObjectProcessor objectProcessor;
        private BlobProcessor blobProcessor;

        private Dictionary<TuioCursor, TouchPointer> cursorToInternalId = new Dictionary<TuioCursor, TouchPointer>(10);
        private Dictionary<TuioBlob, ObjectPointer> blobToInternalId = new Dictionary<TuioBlob, ObjectPointer>();
        private Dictionary<TuioObject, ObjectPointer> objectToInternalId = new Dictionary<TuioObject, ObjectPointer>();
        private int screenWidth;
        private int screenHeight;

        private ObjectPool<TouchPointer> touchPool;
        private ObjectPool<ObjectPointer> objectPool;

        #endregion

        #region Constructor

        public TuioInput()
        {
            touchPool = new ObjectPool<TouchPointer>(20, () => new TouchPointer(this), null, resetPointer);
            objectPool = new ObjectPool<ObjectPointer>(10, () => new ObjectPointer(this), null, resetPointer);
        }

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override bool UpdateInput()
        {
            if (base.UpdateInput()) return true;

            screenWidth = Screen.width;
            screenHeight = Screen.height;

            return true;
        }

        /// <inheritdoc />
        public override bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            base.CancelPointer(pointer, shouldReturn);
            lock (this)
            {
                if (pointer.Type == Pointer.PointerType.Touch)
                {
                    TuioCursor cursor = null;
                    foreach (var touchPoint in cursorToInternalId)
                    {
                        if (touchPoint.Value.Id == pointer.Id)
                        {
                            cursor = touchPoint.Key;
                            break;
                        }
                    }
                    if (cursor != null)
                    {
                        cancelPointer(pointer);
                        if (shouldReturn)
                        {
                            cursorToInternalId[cursor] = internalReturnTouch(pointer as TouchPointer);
                        }
                        else
                        {
                            cursorToInternalId.Remove(cursor);
                        }
                        return true;
                    }
                    return false;
                }

                TuioObject obj = null;
                foreach (var touchPoint in objectToInternalId)
                {
                    if (touchPoint.Value.Id == pointer.Id)
                    {
                        obj = touchPoint.Key;
                        break;
                    }
                }
                if (obj != null)
                {
                    cancelPointer(pointer);
                    if (shouldReturn)
                    {
                        objectToInternalId[obj] = internalReturnObject(pointer as ObjectPointer, pointer.Position);
                    }
                    else
                    {
                        objectToInternalId.Remove(obj);
                    }
                    return true;
                }

                TuioBlob blob = null;
                foreach (var touchPoint in blobToInternalId)
                {
                    if (touchPoint.Value.Id == pointer.Id)
                    {
                        blob = touchPoint.Key;
                        break;
                    }
                }
                if (blob != null)
                {
                    cancelPointer(pointer);
                    if (shouldReturn)
                    {
                        blobToInternalId[blob] = internalReturnObject(pointer as ObjectPointer, pointer.Position);
                    }
                    else
                    {
                        blobToInternalId.Remove(blob);
                    }
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region Internal methods

        /// <inheritdoc />
        public override void INTERNAL_DiscardPointer(Pointer pointer)
        {
            if (pointer.Type == Pointer.PointerType.Touch)
            {
                touchPool.Release(pointer as TouchPointer);
            }
            else if (pointer.Type == Pointer.PointerType.Object)
            {
                objectPool.Release(pointer as ObjectPointer);
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

        private TouchPointer internalAddTouch(Vector2 position)
        {
            var pointer = touchPool.Get();
            pointer.Position = remapCoordinates(position);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown | Pointer.PointerButtonState.FirstButtonPressed;
            addPointer(pointer);
            pressPointer(pointer);
            return pointer;
        }

        private TouchPointer internalReturnTouch(TouchPointer pointer)
        {
            var newPointer = touchPool.Get();
            newPointer.CopyFrom(pointer);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown | Pointer.PointerButtonState.FirstButtonPressed;
            newPointer.Flags |= Pointer.FLAG_RETURNED;
            addPointer(newPointer);
            pressPointer(newPointer);
            return newPointer;
        }

        private ObjectPointer internalAddObject(Vector2 position)
        {
            var pointer = objectPool.Get();
            pointer.Position = remapCoordinates(position);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown | Pointer.PointerButtonState.FirstButtonPressed;
            addPointer(pointer);
            pressPointer(pointer);
            return pointer;
        }

        private ObjectPointer internalReturnObject(ObjectPointer pointer, Vector2 position)
        {
            var newPointer = objectPool.Get();
            newPointer.CopyFrom(pointer);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown | Pointer.PointerButtonState.FirstButtonPressed;
            newPointer.Flags |= Pointer.FLAG_RETURNED;
            addPointer(newPointer);
            pressPointer(newPointer);
            return newPointer;
        }

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

            foreach (var i in cursorToInternalId) cancelPointer(i.Value);
            foreach (var i in blobToInternalId) cancelPointer(i.Value);
            foreach (var i in objectToInternalId) cancelPointer(i.Value);
            cursorToInternalId.Clear();
            blobToInternalId.Clear();
            objectToInternalId.Clear();
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

        private void updateBlobProperties(ObjectPointer obj, TuioBlob target)
        {
            obj.Width = target.Width;
            obj.Height = target.Height;
            obj.Angle = target.Angle;
        }

        private void updateObjectProperties(ObjectPointer obj, TuioObject target)
        {
            obj.ObjectId = target.ClassId;
            obj.Angle = target.Angle;
        }

        private void resetPointer(Pointer p)
        {
            p.INTERNAL_Reset();
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
                cursorToInternalId.Add(entity, internalAddTouch(new Vector2(x, y)));
            }
        }

        private void OnCursorUpdated(object sender, TuioCursorEventArgs e)
        {
            var entity = e.Cursor;
            lock (this)
            {
                TouchPointer touch;
                if (!cursorToInternalId.TryGetValue(entity, out touch)) return;

                var x = entity.X * screenWidth;
                var y = (1 - entity.Y) * screenHeight;

                touch.Position = remapCoordinates(new Vector2(x, y));
                updatePointer(touch);
            }
        }

        private void OnCursorRemoved(object sender, TuioCursorEventArgs e)
        {
            var entity = e.Cursor;
            lock (this)
            {
                TouchPointer touch;
                if (!cursorToInternalId.TryGetValue(entity, out touch)) return;

                cursorToInternalId.Remove(entity);
                releasePointer(touch);
                removePointer(touch);
            }
        }

        private void OnBlobAdded(object sender, TuioBlobEventArgs e)
        {
            var entity = e.Blob;
            lock (this)
            {
                var x = entity.X * screenWidth;
                var y = (1 - entity.Y) * screenHeight;
                var touch = internalAddObject(new Vector2(x, y));
                updateBlobProperties(touch, entity);
                blobToInternalId.Add(entity, touch);
            }
        }

        private void OnBlobUpdated(object sender, TuioBlobEventArgs e)
        {
            var entity = e.Blob;
            lock (this)
            {
                ObjectPointer touch;
                if (!blobToInternalId.TryGetValue(entity, out touch)) return;

                var x = entity.X * screenWidth;
                var y = (1 - entity.Y) * screenHeight;

                touch.Position = remapCoordinates(new Vector2(x, y));
                updateBlobProperties(touch, entity);
                updatePointer(touch);
            }
        }

        private void OnBlobRemoved(object sender, TuioBlobEventArgs e)
        {
            var entity = e.Blob;
            lock (this)
            {
                ObjectPointer touch;
                if (!blobToInternalId.TryGetValue(entity, out touch)) return;

                blobToInternalId.Remove(entity);
                releasePointer(touch);
                removePointer(touch);
            }
        }

        private void OnObjectAdded(object sender, TuioObjectEventArgs e)
        {
            var entity = e.Object;
            lock (this)
            {
                var x = entity.X * screenWidth;
                var y = (1 - entity.Y) * screenHeight;
                var touch = internalAddObject(new Vector2(x, y));
                updateObjectProperties(touch, entity);
                objectToInternalId.Add(entity, touch);
            }
        }

        private void OnObjectUpdated(object sender, TuioObjectEventArgs e)
        {
            var entity = e.Object;
            lock (this)
            {
                ObjectPointer touch;
                if (!objectToInternalId.TryGetValue(entity, out touch)) return;

                var x = entity.X * screenWidth;
                var y = (1 - entity.Y) * screenHeight;

                touch.Position = remapCoordinates(new Vector2(x, y));
                updateObjectProperties(touch, entity);
                updatePointer(touch);
            }
        }

        private void OnObjectRemoved(object sender, TuioObjectEventArgs e)
        {
            var entity = e.Object;
            lock (this)
            {
                ObjectPointer touch;
                if (!objectToInternalId.TryGetValue(entity, out touch)) return;

                objectToInternalId.Remove(entity);
                releasePointer(touch);
                removePointer(touch);
            }
        }

        #endregion
    }
}

#endif
#endif