/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Tags collection for touches.
    /// </summary>
    [Serializable]
    public sealed class Tags
    {
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
        /// Windows native touch source.
        /// </summary>
        public const string SOURCE_WINDOWS = "Windows";

        /// <summary>
        /// TUIO touch source.
        /// </summary>
        public const string SOURCE_TUIO = "TUIO";

        /// <summary>
        /// List of tags.
        /// </summary>
        public ICollection<string> TagList
        {
            get { return new ReadOnlyCollection<string>(tagList); }
        }

        /// <summary>
        /// Number of tags in this collection.
        /// </summary>
        public int Count
        {
            get { return tagList.Count; }
        }

        [SerializeField]
        private List<string> tagList = new List<string>();

        /// <summary>
        /// Creates an instance of Tags.
        /// </summary>
        /// <param name="tags">Tags collection to copy.</param>
        public Tags(Tags tags) : this()
        {
            if (tags == null) return;
            foreach (var tag in tags.tagList)
            {
                AddTag(tag);
            }
        }

        /// <summary>
        /// Creates an instance of Tags.
        /// </summary>
        /// <param name="tags">Tags collection to copy.</param>
        public Tags(IEnumerable<string> tags) : this()
        {
            foreach (var tag in tags)
            {
                AddTag(tag);
            }
        }

        /// <summary>
        /// Creates an instance of Tags.
        /// </summary>
        /// <param name="tag">Tag to add to the new collection.</param>
        public Tags(string tag) : this()
        {
            tagList.Add(tag);
        }

        /// <summary>
        /// Creates an instance of Tags.
        /// </summary>
        public Tags() {}

        /// <summary>
        /// Adds a tag to this collection.
        /// </summary>
        /// <param name="tag">Tag to add.</param>
        public void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
            if (tagList.Contains(tag)) return;
            tagList.Add(tag);
        }

        /// <summary>
        /// Removes a tag from this collection.
        /// </summary>
        /// <param name="tag">Tag to remove.</param>
        public void RemoveTag(string tag)
        {
            tagList.Remove(tag);
        }

        /// <summary>
        /// Checks if this collection contains a tag.
        /// </summary>
        /// <param name="tag">Tag.</param>
        /// <returns>True if tag is in this collection; false otherwise.</returns>
        public bool HasTag(string tag)
        {
            return tagList.Contains(tag);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Join(", ", tagList.ToArray());
        }
    }
}
