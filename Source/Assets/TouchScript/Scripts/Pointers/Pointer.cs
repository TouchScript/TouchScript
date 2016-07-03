/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Hit;
using TouchScript.InputSources;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript.Pointers
{
    /// <summary>
    /// <para>Representation of a pointer (touch, mouse) within TouchScript.</para>
    /// <para>An instance of this class is created when user touches the screen. A unique id is assigned to it which doesn't change throughout its life.</para>
    /// <para><b>Attention!</b> Do not store references to these objects beyond pointer's lifetime (i.e. when target finger is lifted off). These objects may be reused internally. Store unique ids instead.</para>
    /// </summary>
    public class Pointer
    {

        #region Constants

        public const int INVALID_POINTER = -1;

        public enum PointerType
        {
            Touch,
            Mouse,
            Pen,
            Object
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Internal unique pointer id.
        /// </summary>
        public int Id { get; private set; }

        public PointerType Type { get; protected set; }

        /// <summary>
        /// Original hit target.
        /// </summary>
        public Transform Target { get; internal set; }

        /// <summary>
        /// Current position in screen coordinates.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }

        /// <summary>
        /// Previous position (during last frame) in screen coordinates.
        /// </summary>
        public Vector2 PreviousPosition { get; private set; }

        /// <summary>
        /// Original hit information.
        /// </summary>
        public TouchHit Hit { get; internal set; }

        /// <summary>
        /// Original layer which registered this pointer.
        /// <seealso cref="TouchLayer"/>
        /// <seealso cref="CameraLayer"/>
        /// <seealso cref="CameraLayer2D"/>
        /// </summary>
        public TouchLayer Layer { get; internal set; }

        /// <summary>
        /// Original input source which created this pointer.
        /// <seealso cref="IInputSource"/>
        /// </summary>
        public IInputSource InputSource { get; private set; }

        /// <summary>
        /// Projection parameters for the layer which created this pointer.
        /// </summary>
        public ProjectionParams ProjectionParams
        {
            get { return Layer.GetProjectionParams(this); }
        }

        #endregion

        #region Private variables

        private int refCount = 0;
        private Vector2 position = Vector2.zero;
        private Vector2 newPosition = Vector2.zero;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            return Equals(other as Pointer);
        }

        /// <inheritdoc />
        public bool Equals(Pointer other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Pointer"/> class.
        /// </summary>
        public Pointer(IInputSource input)
        {
            Type = PointerType.Touch;
            InputSource = input;
			INTERNAL_Reset();
        }

        #endregion

        #region Internal methods

        internal void INTERNAL_Init(int id, Vector2 position)
        {
            Id = id;
            this.position = PreviousPosition = newPosition = position;
        }

        internal void INTERNAL_Reset()
        {
            Id = INVALID_POINTER;
            refCount = 0;
            Hit = default(TouchHit);
            Target = null;
            Layer = null;
        }

        internal void INTERNAL_ResetPosition()
        {
            PreviousPosition = position;
            position = newPosition;
            newPosition = position;
        }

        internal void INTERNAL_SetPosition(Vector2 value)
        {
            newPosition = value;
        }

        internal void INTERNAL_Retain()
        {
            refCount++;
        }

        internal int INTERNAL_Release()
        {
            return --refCount;
        }

        #endregion

    }
}