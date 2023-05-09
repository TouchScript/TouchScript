using System.Collections.Generic;
using TouchScript.Pointers;
using TouchScript.Utils;
using TuioNet.Common;
using TuioNet.Tuio20;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Processes TUIO 2.0 input
    /// </summary>
    
    [AddComponentMenu("TouchScript/Input Sources/TUIO 2.0 Input")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_InputSources_TuioInput.htm")]
    public sealed class Tuio20Input : TuioInput, ITuio20Listener
    {
        private Tuio20Client _client;
        protected override void Init()
        {
            if (IsInitialized) return;
            _client = new Tuio20Client(_connectionType, _ipAddress, _port, false);
            Connect();
            IsInitialized = true;
        }
        
        protected override void Connect()
        {
            _client?.Connect();
            _client?.AddTuioListener(this);
        }

        protected override void Disconnect()
        {
            _client.RemoveAllTuioListeners();
            _client?.Disconnect();
        }
        
        public override bool UpdateInput()
        {
            _client.ProcessMessages();
            return true;
        }

        public void TuioAdd(Tuio20Object tuio20Object)
        {
            lock (this)
            {
                if (tuio20Object.ContainsNewTuioPointer())
                {
                    var tuioPointer = tuio20Object.Pointer;
                    var screenPosition = new Vector2
                    {
                        x = tuioPointer.Position.X * ScreenWidth,
                        y = (1f - tuioPointer.Position.Y) * ScreenHeight
                    };
                   TouchToInternalId.Add(tuioPointer.ComponentId, AddTouch(screenPosition));
                }

                if (tuio20Object.ContainsNewTuioToken())
                {
                    var token = tuio20Object.Token;
                    var screenPosition = new Vector2
                    {
                        x = token.Position.X * ScreenWidth,
                        y = (1f - token.Position.Y) * ScreenHeight
                    };
                    var objectPointer = AddObject(screenPosition);
                    UpdateObjectProperties(objectPointer,token);
                    ObjectToInternalId.Add(token.ComponentId, objectPointer);
                }
                
            }
        }

        public void TuioUpdate(Tuio20Object tuio20Object)
        {
            lock (this)
            {
                if (tuio20Object.ContainsTuioPointer())
                {
                    var tuioPointer = tuio20Object.Pointer;
                    if(!TouchToInternalId.TryGetValue(tuioPointer.ComponentId, out var touchPointer)) return;
                    var screenPosition = new Vector2
                    {
                        x = tuioPointer.Position.X * ScreenWidth,
                        y = (1f - tuioPointer.Position.Y) * ScreenHeight
                    };
                    touchPointer.Position = RemapCoordinates(screenPosition);
                    UpdatePointer(touchPointer);
                }

                if (tuio20Object.ContainsTuioToken())
                {
                    var token = tuio20Object.Token;
                    if (!ObjectToInternalId.TryGetValue(token.ComponentId, out var objectPointer)) return;
                    var screenPosition = new Vector2
                    {
                        x = token.Position.X * ScreenWidth,
                        y = (1f - token.Position.Y) * ScreenHeight
                    };
                    objectPointer.Position = RemapCoordinates(screenPosition);
                    UpdateObjectProperties(objectPointer, token);
                    UpdatePointer(objectPointer);
                }
            }
        }

        public void TuioRemove(Tuio20Object tuio20Object)
        {
            lock (this)
            {
                if (tuio20Object.ContainsTuioPointer())
                {
                    var tuioPointer = tuio20Object.Pointer;
                    if (!TouchToInternalId.TryGetValue(tuioPointer.ComponentId, out var touchPointer)) return;
                    TouchToInternalId.Remove(tuioPointer.ComponentId);
                    ReleasePointer(touchPointer);
                    RemovePointer(touchPointer);
                }

                if (tuio20Object.ContainsTuioToken())
                {
                    var token = tuio20Object.Token;
                    if (!ObjectToInternalId.TryGetValue(token.ComponentId, out var objectPointer)) return;
                    ObjectToInternalId.Remove(token.ComponentId);
                    ReleasePointer(objectPointer);
                    RemovePointer(objectPointer);
                }
            }
        }

        public void TuioRefresh(TuioTime tuioTime) { }
        
        private void UpdateObjectProperties(ObjectPointer pointer, Tuio20Token token)
        {
            pointer.ObjectId = (int)token.ComponentId;
            pointer.Angle = token.Angle;
        }
    }
}