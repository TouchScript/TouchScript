/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;
using UnityEngine;

namespace TouchScript.Pointers
{
    /// <summary>
    /// A pointer of type <see cref="Pointer.PointerType.Mouse"/>.
    /// </summary>
    public class MousePointer : Pointer
    {
        #region Public properties

        /// <summary>
        /// Mouse scroll delta this frame.
        /// </summary>
        public Vector2 ScrollDelta { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MousePointer"/> class.
        /// </summary>
        public MousePointer(IInputSource input) : base(input)
        {
            Type = PointerType.Mouse;
        }

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override void CopyFrom(Pointer target)
        {
            base.CopyFrom(target);

            var mouseTarget = target as MousePointer;
            if (mouseTarget == null) return;
            ScrollDelta = mouseTarget.ScrollDelta;
        }

        #endregion

        #region Internal functions

        //internal override void INTERNAL_Reset()
        //{
        //    base.INTERNAL_Reset();
        //}

        #endregion
    }
}