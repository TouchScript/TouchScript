/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Behaviors.Cursors
{
    /// <summary>
    /// Cursor for object pointers.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Cursors_ObjectCursor.htm")]
    public class ObjectCursor : TextPointerCursor<ObjectPointer>
    {
        #region Public properties

        /// <summary>
        /// Should the value of <see cref="ObjectPointer.ObjectId"/> be shown on the cursor.
        /// </summary>
        public bool ShowObjectId = false;

        /// <summary>
        /// Should the values of <see cref="ObjectPointer.Width"/> and <see cref="ObjectPointer.Height"/> be shown on the cursor.
        /// </summary>
        public bool ShowSize = false;

        /// <summary>
        /// Should the value of <see cref="ObjectPointer.Angle"/> be shown on the cursor.
        /// </summary>
        public bool ShowAngle = false;

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override void GenerateText(ObjectPointer pointer, StringBuilder str)
        {
            base.GenerateText(pointer, str);

            if (ShowObjectId)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("ObjectId: ");
                str.Append(pointer.ObjectId);
            }
            if (ShowSize)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Size: ");
                str.Append(pointer.Width);
                str.Append("x");
                str.Append(pointer.Height);
            }
            if (ShowAngle)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Angle: ");
                str.Append(pointer.Angle);
            }
        }

        /// <inheritdoc />
        protected override void UpdatePointerInternal(IPointer pointer)
        {
            base.UpdatePointerInternal(pointer);
            if (pointer is not ObjectPointer objectPointer) return;
            var angle = objectPointer.Angle * Mathf.Rad2Deg;
            var rotation = Quaternion.AngleAxis(angle, Vector3.back);
            Rect.rotation = rotation;
        }

        /// <inheritdoc />
        protected override bool TextIsVisible()
        {
            return base.TextIsVisible() || ShowObjectId || ShowSize || ShowAngle;
        }

        /// <inheritdoc />
        protected override uint GetHash(ObjectPointer pointer)
        {
            var hash = base.GetHash(pointer);

            if (ShowSize) hash += (uint) (pointer.Width * 1024 + pointer.Height * 1024 * 1024) << 8;
            if (ShowAngle) hash += (uint) (pointer.Angle * 1024) << 24;

            return hash;
        }

        #endregion
    }
}