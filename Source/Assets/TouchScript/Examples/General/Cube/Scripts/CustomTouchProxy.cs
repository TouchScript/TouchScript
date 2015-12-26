using UnityEngine;
using System.Collections;
using TouchScript.Behaviors.Visualizer;

namespace TouchScript.Examples.Cube 
{
    public class CustomTouchProxy : TouchScript.Behaviors.Visualizer.TouchProxy 
    {
        protected override void updateOnce(TouchPoint touch) {
            if (touch.InputSource is RedirectInput) Hide();
            
            base.updateOnce(touch);
        }
    }
}