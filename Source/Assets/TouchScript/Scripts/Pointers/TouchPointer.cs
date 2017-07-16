/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{
    /// <summary>
    /// A pointer of type <see cref="Pointer.PointerType.Touch"/>.
    /// </summary>
    public class TouchPointer : Pointer
    {
        #region Public consts

        public const float DEFAULT_PRESSURE = 0.5f;
        public const float DEFAULT_ROTATION = 0f;

        #endregion

        #region Public properties

        public float Rotation { get; set; }

        public float Pressure { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchPointer"/> class.
        /// </summary>
        public TouchPointer(IInputSource input) : base(input)
        {
            Type = PointerType.Touch;
        }

        #endregion

        #region Internal functions

        internal override void INTERNAL_Reset()
        {
            base.INTERNAL_Reset();

            Rotation = DEFAULT_ROTATION;
            Pressure = DEFAULT_PRESSURE;
        }

        #endregion
    }
}