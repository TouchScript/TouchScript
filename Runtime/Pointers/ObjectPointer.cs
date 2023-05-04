/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;

namespace TouchScript.Pointers
{
    /// <summary>
    /// A pointer of type <see cref="Pointer.PointerType.Object"/>.
    /// </summary>
    public class ObjectPointer : Pointer
    {
        #region Public consts

        /// <summary>
        /// Default object id value when device doesn't provide it.
        /// </summary>
        public const int DEFAULT_OBJECT_ID = 0;


        /// <summary>
        /// Default width value when device doesn't provide it.
        /// </summary>
        public const float DEFAULT_WIDTH = 1f;

        /// <summary>
        /// Default height value when device doesn't provide it.
        /// </summary>
        public const float DEFAULT_HEIGHT = 1f;

        /// <summary>
        /// Default angle value when device doesn't provide it.
        /// </summary>
        public const float DEFAULT_ANGLE = 0f;

        #endregion

        #region Public properties

        /// <summary>
        /// The Id of the physical object this pointer represents.
        /// </summary>
        public int ObjectId { get; internal set; }

        /// <summary>
        /// The Width of the physical object this pointer represents.
        /// </summary>
        public float Width { get; internal set; }

        /// <summary>
        /// The height of the physical object this pointer represents.
        /// </summary>
        public float Height { get; internal set; }

        /// <summary>
        /// The Rotation of the physical object this pointer represents.
        /// </summary>
        public float Angle { get; internal set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPointer"/> class.
        /// </summary>
        public ObjectPointer(IInputSource input) : base(input)
        {
            Type = PointerType.Object;
        }

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override void CopyFrom(Pointer target)
        {
            base.CopyFrom(target);
            var obj = target as ObjectPointer;
            if (obj == null) return;

            ObjectId = obj.ObjectId;
            Width = obj.Width;
            Height = obj.Height;
            Angle = obj.Angle;
        }

        #endregion

        #region Internal functions

        /// <inheritdoc />
        internal override void INTERNAL_Reset()
        {
            base.INTERNAL_Reset();
            ObjectId = DEFAULT_OBJECT_ID;
            Width = DEFAULT_WIDTH;
            Height = DEFAULT_HEIGHT;
            Angle = DEFAULT_ANGLE;
        }

        #endregion
    }
}