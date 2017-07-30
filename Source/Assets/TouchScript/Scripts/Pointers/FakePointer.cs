/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Hit;
using TouchScript.InputSources;
using UnityEngine;

namespace TouchScript.Pointers
{
    /// <summary>
    /// Fake pointer.
    /// </summary>
    /// <seealso cref="TouchScript.Pointers.Pointer" />
    public class FakePointer : IPointer
    {
        #region Public properties

        /// <inheritdoc />
        public int Id { get; private set; }

        /// <inheritdoc />
        public Pointer.PointerType Type { get; private set; }

        /// <inheritdoc />
        public IInputSource InputSource { get; private set; }

        /// <inheritdoc />
        public Vector2 Position { get; set; }

        /// <inheritdoc />
        public uint Flags { get; private set; }

        /// <inheritdoc />
        public Pointer.PointerButtonState Buttons { get; private set; }

        /// <inheritdoc />
        public Vector2 PreviousPosition { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FakePointer"/> class.
        /// </summary>
        /// <param name="position">The position.</param>
        public FakePointer(Vector2 position) : this()
        {
            Position = position;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakePointer"/> class.
        /// </summary>
        public FakePointer()
        {
            Id = Pointer.INVALID_POINTER;
            Type = Pointer.PointerType.Unknown;
            Flags = Pointer.FLAG_ARTIFICIAL;
        }

        #endregion

        #region Public methods

        /// <inheritdoc />
        public HitData GetOverData(bool forceRecalculate = false)
        {
            HitData overData;
            LayerManager.Instance.GetHitTarget(this, out overData);
            return overData;
        }

        #endregion
    }
}