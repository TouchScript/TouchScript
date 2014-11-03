/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Hit;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// <para>Representation of a finger within TouchScript.</para>
    /// <para>An object implementing this interface is created when user touches the screen. A unique id is assigned to it which doesn't change throughout its life.</para>
    /// <para><b>Attention!</b> Do not store references to these objects beyond touch's lifetime (i.e. when target finger is lifted off). These objects may be reused internally. Store unique ids instead.</para>
    /// </summary>
    public interface ITouch
    {
        /// <summary>
        /// Internal unique touch point id.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Original hit target.
        /// </summary>
        Transform Target { get; }

        /// <summary>
        /// Current position in screen coordinates.
        /// </summary>
        Vector2 Position { get; }

        /// <summary>
        /// Previous position (during last frame) in screen coordinates.
        /// </summary>
        Vector2 PreviousPosition { get; }

        /// <summary>
        /// Original hit information.
        /// <seealso cref="ITouchHit"/>
        /// <seealso cref="ITouchHit2D"/>
        /// <seealso cref="ITouchHit3D"/>
        /// </summary>
        ITouchHit Hit { get; }

        /// <summary>
        /// Original layer which created this touch object.
        /// <seealso cref="TouchLayer"/>
        /// <seealso cref="CameraLayer"/>
        /// <seealso cref="CameraLayer2D"/>
        /// </summary>
        TouchLayer Layer { get; }

        /// <summary>
        /// Tags collection for this touch object.
        /// </summary>
        Tags Tags { get; }

        /// <summary>
        /// List of custom properties (key-value pairs) for this touch object.
        /// </summary>
        IDictionary<string, System.Object> Properties { get; }
    }
}
