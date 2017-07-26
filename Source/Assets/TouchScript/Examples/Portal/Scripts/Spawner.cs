/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Gestures;
using UnityEngine;

namespace TouchScript.Examples.Portal
{
    /// <exclude />
    public class Spawner : MonoBehaviour
    {

        public Transform Prefab;
        public Transform Position;

        private PressGesture press;

        private void OnEnable()
        {
            press = GetComponent<PressGesture>();
            press.Pressed += pressHandler;
        }

        private void OnDisable()
        {
            press.Pressed -= pressHandler;
        }

        private void pressHandler(object sender, EventArgs eventArgs)
        {
            var target = Instantiate(Prefab, Position.parent);
            target.position = Position.position;

            LayerManager.Instance.SetExclusive(target);
            press.Cancel(true, true);
            LayerManager.Instance.ClearExclusive();
        }
    }
}