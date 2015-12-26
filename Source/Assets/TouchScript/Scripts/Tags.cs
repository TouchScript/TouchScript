/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Immutable tag collection for touches.
    /// </summary>
    [Serializable]
    public sealed class Tags : ISerializationCallbackReceiver
    {
        #region Constants

        /// <summary>
        /// Touch.
        /// </summary>
        public const string INPUT_TOUCH = "Touch";

        /// <summary>
        /// Mouse.
        /// </summary>
        public const string INPUT_MOUSE = "Mouse";

        /// <summary>
        /// Pen.
        /// </summary>
        public const string INPUT_PEN = "Pen";

        /// <summary>
        /// Object.
        /// </summary>
        public const string INPUT_OBJECT = "Object";

        /// <summary>
        /// Empty tag list.
        /// </summary>
        public static readonly Tags EMPTY = new Tags();

        #endregion

        #region Public properties

        #endregion

        #region Private variables

        [SerializeField]
        private List<string> tagList = new List<string>();

        private HashSet<string> tags = new HashSet<string>();
        private string stringValue;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Tags"/> class.
        /// </summary>
        /// <param name="tags"> Tags to copy. </param>
        /// <param name="add"> Tags to add. </param>
        public Tags(Tags tags, IEnumerable<string> add) : this(tags)
        {
            if (add == null) return;
            foreach (var tag in add)
            {
                if (string.IsNullOrEmpty(tag)) continue;
                this.tags.Add(tag);
            }
#if UNITY_EDITOR
            syncTagList();
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tags"/> class.
        /// </summary>
        /// <param name="tags"> Tags to copy. </param>
        /// <param name="add"> Tag to add. </param>
        public Tags(Tags tags, string add) : this(tags)
        {
            if (string.IsNullOrEmpty(add)) return;
            this.tags.Add(add);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tags"/> class.
        /// </summary>
        /// <param name="tags"> Tags to copy. </param>
        public Tags(Tags tags) : this()
        {
            if (tags == null) return;
            foreach (var tag in tags.tags) this.tags.Add(tag);
#if UNITY_EDITOR
            syncTagList();
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tags"/> class.
        /// </summary>
        /// <param name="tags"> Tags to copy. </param>
        public Tags(IEnumerable<string> tags) : this()
        {
            if (tags == null) return;
            foreach (var tag in tags)
            {
                if (string.IsNullOrEmpty(tag)) continue;
                this.tags.Add(tag);
            }
#if UNITY_EDITOR
            syncTagList();
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tags"/> class.
        /// </summary>
        /// <param name="tags"> Tags to add. </param>
        public Tags(params string[] tags) : this()
        {
            if (tags == null) return;
            var count = tags.Length;
            for (var i = 0; i < count; i++)
            {
                var tag = tags[i];
                if (string.IsNullOrEmpty(tag)) continue;
                this.tags.Add(tag);
            }
#if UNITY_EDITOR
            syncTagList();
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tags"/> class.
        /// </summary>
        /// <param name="tag"> Tag to add. </param>
        public Tags(string tag) : this()
        {
            if (string.IsNullOrEmpty(tag)) return;
            tags.Add(tag);
#if UNITY_EDITOR
            syncTagList();
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tags"/> class.
        /// </summary>
        public Tags() {}

        #endregion

        #region Public methods

        /// <summary>
        /// Checks if this collection contains a tag.
        /// </summary>
        /// <param name="tag">Tag.</param>
        /// <returns>True if tag is in this collection; false otherwise.</returns>
        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }


        /// <exclude />
        public void OnBeforeSerialize()
        {
#if !UNITY_EDITOR
            tagList.Clear();
            tagList.AddRange(tags);
#endif
        }

        /// <exclude />
        public void OnAfterDeserialize()
        {
            tags.Clear();
            foreach (var tag in tagList) tags.Add(tag);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (stringValue == null)
            {
                if (tags.Count == 0)
                {
                    stringValue = "";
                }
                else if (tags.Count == 1)
                {
                    foreach (var tag in tags) stringValue = tag; // doh!?
                }
                else
                {
                    var sb = new StringBuilder(100);
                    foreach (var tag in tags)
                    {
                        sb.Append(tag);
                        sb.Append(", ");
                    }
                    stringValue = sb.ToString(0, sb.Length - 2);
                }
            }
            return stringValue;
        }

        #endregion

        #region Private functions

#if UNITY_EDITOR
        // When Tags is created in editor as a component's property need to copy all tags to tagList so Unity could serialize them.
        private void syncTagList()
        {
            tagList.Clear();
            tagList.AddRange(tags);
        }
#endif

        #endregion
    }
}