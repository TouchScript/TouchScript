using UnityEngine;
using System;
using System.Collections.Generic;

namespace TouchScript.InputSources {	
	struct TouchState {
		public int Id;
		public TouchPhase Phase;
		public Vector2 Position;
		
		public TouchState(int anId, TouchPhase aPhase, Vector2 aPosition) {
			Id = anId;
			Phase = aPhase;
			Position = aPosition;
		}
	}
	
	/// <summary>
    /// iOS Input Source
    /// </summary>
	public class IOSInput : InputSource
	{
		#region Private variables
		  private Dictionary<int, TouchState> touchStates = new Dictionary<int, TouchState>();
		  private HashSet<int> touchIds = new HashSet<int>();

//        private int mousePointId = -1;
//        private Vector3 mousePointPos = Vector3.zero;

        #endregion

        #region Unity

        protected override void Update() {
            base.Update();
			
			for (var i = 0; i < Input.touchCount; ++i) {
	     		var t = Input.GetTouch(i);
				
				switch (t.phase) {
				case TouchPhase.Began:
					if (touchIds.Contains(t.fingerId)) {
						// ending previous touch (maybe we missed frame)
						endTouch(t.fingerId);
						int id = beginTouch(t.position);
						touchStates[t.fingerId] = new TouchState(id, t.phase, t.position);
					} else {
						touchIds.Add(t.fingerId);
						int id = beginTouch(t.position);
						touchStates.Add(t.fingerId, new TouchState(id, t.phase, t.position));												
					}					
						break;
				case TouchPhase.Moved:
					if (touchIds.Contains(t.fingerId)) {
						var ts = touchStates[t.fingerId];
						touchStates[t.fingerId] = new TouchState(ts.Id, t.phase, t.position);
						moveTouch(ts.Id, t.position);						
					} else {
						//maybe we missed began phase
						touchIds.Add(t.fingerId);
						int id = beginTouch(t.position);						
						touchStates.Add(t.fingerId, new TouchState(id, t.phase, t.position));												
					}
						break;
				case TouchPhase.Ended:
					if (touchIds.Contains(t.fingerId)) {
						var ts = touchStates[t.fingerId];
						touchIds.Remove(t.fingerId);
						touchStates.Remove(t.fingerId);						
						endTouch(ts.Id);
					} else {
						//maybe we totally missed one finger begin-end transition
						int id = beginTouch(t.position);
						endTouch(id);
					}
						break;
				case TouchPhase.Canceled:
					if (touchIds.Contains(t.fingerId)) {
						var ts = touchStates[t.fingerId];
						touchIds.Remove(t.fingerId);
						touchStates.Remove(t.fingerId);						
						endTouch(ts.Id);
					} else {
						//maybe we totally missed one finger begin-end transition
						int id = beginTouch(t.position);
						cancelTouch(id);
					}
						break;
				case TouchPhase.Stationary:
					if (touchIds.Contains(t.fingerId)) {
						//do nothing
					} else {
						touchIds.Add(t.fingerId);
						int id = beginTouch(t.position);
						touchStates.Add(t.fingerId, new TouchState(id, t.phase, t.position));						
					}
						break;
				}
    		}
        }

        #endregion
	}
}

