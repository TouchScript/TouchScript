/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Input source to grab mouse clicks as touch points.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/Mouse Input")]
    public class MouseInput : InputSource
    {

        #region Public properties

        public bool DestroyOnMobileDevices = true;

        #endregion

        #region Private variables

        private int mousePointId = -1;
        private int fakeMousePointId = -1;
        private Vector3 mousePointPos = Vector3.zero;

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void Start()
        {
            if (DestroyOnMobileDevices)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.WP8Player:
                        // don't need mouse here
                        Destroy(this);
                        return;
                }
            }
            base.Start();
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
                    upHandled = true;
                }
            }

            if (fakeMousePointId > -1 && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                endTouch(fakeMousePointId);
                fakeMousePointId = -1;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var pos = Input.mousePosition;
                if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && fakeMousePointId == -1)
                {
                    if (fakeMousePointId == -1) fakeMousePointId = beginTouch(new Vector2(pos.x, pos.y));
                } else
                {
                    if (mousePointId == -1) mousePointId = beginTouch(new Vector2(pos.x, pos.y));
                }
            } else if (Input.GetMouseButton(0))
            {
                var pos = Input.mousePosition;
                if (mousePointPos != pos)
                {
                    mousePointPos = pos;
                    if (fakeMousePointId > -1 && mousePointId == -1)
                    {
                        moveTouch(fakeMousePointId, new Vector2(pos.x, pos.y));
                    } else
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
    }
}