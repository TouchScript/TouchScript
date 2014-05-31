using System;
using System.Collections.Generic;
using UnityEngine;

namespace TouchScript
{
    [Serializable]
    public sealed class Tags
    {
        public const string INPUT_TOUCH = "Touch";
        public const string INPUT_MOUSE = "Mouse";
        public const string INPUT_PEN = "Pen";
        public const string INPUT_OBJECT = "Object";

        public const string SOURCE_WINDOWS = "Windows";
        public const string SOURCE_TUIO = "TUIO";

        public ICollection<string> TagList { get { return tagList.AsReadOnly(); }}

        public int Count { get { return tagList.Count; } }

        [SerializeField]
        private List<string> tagList = new List<string>();

        public Tags(Tags tags) : this()
        {
            if (tags == null) return;
            foreach (var tag in tags.tagList)
            {
                AddTag(tag);
            }
        }

        public Tags(IEnumerable<string> tags) : this()
        {
            foreach (var tag in tags)
            {
                AddTag(tag);
            }
        }

        public Tags(string tag) : this()
        {
            tagList.Add(tag);
        }

        public Tags()
        {}

        public void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
            if (tagList.Contains(tag)) return;
            tagList.Add(tag);
        }

        public void RemoveTag(string tag)
        {
            tagList.Remove(tag);
        }

        public bool HasTag(string tag)
        {
            return tagList.Contains(tag);
        }

        public override string ToString()
        {
            return String.Join(", ", tagList.ToArray());
        }
    }
}
