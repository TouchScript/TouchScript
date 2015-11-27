/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils.Attributes;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Input source which transfers mouse clicks to touches.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/Mouse Input")]
    public sealed class MouseInput : InputSource
    {
        #region Public properties

        /// <summary>
        /// Indicates if this input source should be disabled on mobile platforms.
        /// </summary>
        /// <remarks>
        /// Operation Systems which support touch input send first touches as mouse clicks which may result in duplicated touch points in exactly the same coordinates. This affects clusters and multitouch gestures.
        /// </remarks>
        [ToggleLeft] public bool DisableOnMobilePlatforms = true;

        /// <summary>
        /// Tags added to touches coming from this input.
        /// </summary>
        public Tags Tags = new Tags(Tags.INPUT_MOUSE);

        #endregion

        #region Private variables

        private int mousePointId = -1;
        private int fakeMousePointId = -1;
        private Vector3 mousePointPos = Vector3.zero;

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void OnEnable()
        {
            if (DisableOnMobilePlatforms)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.WP8Player:
                    case RuntimePlatform.MetroPlayerARM:
                    case RuntimePlatform.MetroPlayerX64:
                    case RuntimePlatform.MetroPlayerX86:
                        // don't need mouse here
                        enabled = false;
                        return;
                }
            }

            base.OnEnable();

            mousePointId = -1;
            fakeMousePointId = -1;
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            if (mousePointId != -1) cancelTouch(mousePointId);
            if (fakeMousePointId != -1) cancelTouch(fakeMousePointId);

            base.OnDisable();
        }

        /// <inheritdoc />
        protected override void Update()
        {
            base.Update();

            var upHandled = false;
            if (Input.GetMouseButtonUp(0))
            {
                if (mousePointId != -1)
                {
                    endTouch(mousePointId);
                    mousePointId = -1;
                }
                upHandled = true;
            }

            if (fakeMousePointId > -1 && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                endTouch(fakeMousePointId);
                fakeMousePointId = -1;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var pos = mousePointPos = Input.mousePosition;
                if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && fakeMousePointId == -1)
                {
                    if (fakeMousePointId == -1) fakeMousePointId = beginTouch(new Vector2(pos.x, pos.y)).Id;
                }
                else
                {
                    if (mousePointId == -1) mousePointId = beginTouch(new Vector2(pos.x, pos.y)).Id;
                }
            }
            else if (Input.GetMouseButton(0))
            {
                var pos = Input.mousePosition;
                if (mousePointPos != pos)
                {
                    mousePointPos = pos;
                    if (fakeMousePointId > -1 && mousePointId == -1)
                    {
                        moveTouch(fakeMousePointId, new Vector2(pos.x, pos.y));
                    }
                    else
                    {
                        moveTouch(mousePointId, new Vector2(pos.x, pos.y));
                    }
                }
            }

            if (Input.GetMouseButtonUp(0) && !upHandled)
            {
                endTouch(mousePointId);
                mousePointId = -1;
            }
        }

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override ITouch beginTouch(Vector2 position)
        {
            return beginTouch(position, new Tags(Tags));
        }

        #endregion
    }
}