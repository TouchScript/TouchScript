/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Gestures;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TouchScript.Examples.Multiuser
{
    public class Logo : MonoBehaviour
    {
        private static Color[] COLORS = new[]
        {Color.yellow, Color.red, Color.magenta, Color.green, Color.cyan, Color.blue};

        private void OnEnable()
        {
            GetComponent<TapGesture>().Tapped += tappedHandler;
        }

        private void OnDisable()
        {
            GetComponent<TapGesture>().Tapped -= tappedHandler;
        }

        private void changeColor()
        {
            Color newColor = COLORS[Random.Range(0, COLORS.Length)];
            while (newColor == GetComponent<Renderer>().material.color)
                newColor = COLORS[Random.Range(0, COLORS.Length)];

            GetComponent<Renderer>().material.color = newColor;
        }

        private void tappedHandler(object sender, EventArgs eventArgs)
        {
            changeColor();
        }
    }
}