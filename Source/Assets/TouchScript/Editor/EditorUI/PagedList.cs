/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.EditorUI
{

	public class PagedList
	{

        private class Styles
        {
            public int HeaderHeight = 0;
            public int FooterHeight = 20;

            public int FooterButtonWidth = 60;
            public int FooterButtonHeight = 20;
            public int FooterButtonSpace = 10;
            public int FooterTextWidth = 8;

            public GUIContent TextPrev = new GUIContent("< Prev");
            public GUIContent TextNext = new GUIContent("Next >");

			public int GetIntFieldSize(int value)
			{
				if (value < 10) return FooterTextWidth + 8;
				if (value < 100) return 2 * FooterTextWidth + 8;
				if (value < 1000) return 3 * FooterTextWidth + 8;
				return 4;
			}
        }

		public int ItemHeight { get; set; }
		public int Count 
        { 
            get
            {
                return count;        
            }
            set
            {
                if (count == value) return;
                count = value;
                reset();
            } 
        }
        public int PagesTotal
        {
            get
            {
                return pagesTotal;
            }
        }
        public int SelectedId
        {
            get
            {
                return selectedId;
            }
        }

        private static Styles styles;

		private Action<int> onSelectionChange;
		private Action<int, Rect, bool> drawItem;

        private int count = 0;
        // Starts from 1
		private int page = 1;
        private int pagesTotal = 1;
        private int itemsPerPage = 1;

		private int selectedId = -1;
		private int oldSelectedId = -1;

		public PagedList(int itemHeight, Action<int, Rect, bool> drawItem, Action<int> onSelectionChange)
		{
            if (styles == null) styles = new Styles();

			ItemHeight = itemHeight;
			this.onSelectionChange = onSelectionChange;
			this.drawItem = drawItem;
		}

		public int FitHeight(int numberOfItems)
		{
			return ItemHeight * numberOfItems + styles.FooterHeight + styles.HeaderHeight;
		}

		public void Draw(Rect rect)
		{
			var h = rect.height;
			h -= styles.HeaderHeight + styles.FooterHeight;
			if (h < 0) return;
			rect.y += styles.HeaderHeight;
			rect.height = ItemHeight;

			itemsPerPage = Mathf.FloorToInt(h / 22f);
            pagesTotal = Mathf.CeilToInt((float)count / itemsPerPage);

            int start = (Count - 1) - (page - 1) * itemsPerPage;
			if (start < 0) return;

			var i = start;
			var t = 0;
			while (t < itemsPerPage)
			{
				if (i < 0) draw(-1, rect);
				else draw(i, rect);
				rect.y += ItemHeight;
				i--;
				t++;
			}

            rect.height = styles.FooterHeight;
            drawFooter(rect);

			if (oldSelectedId != selectedId)
			{
				oldSelectedId = selectedId;
				onSelectionChange(selectedId);
			}
		}

        private void drawFooter(Rect parentRect)
        {
            parentRect.y += 5;
            parentRect.height -= 5;

            var rect = new Rect(parentRect.x, parentRect.y, styles.FooterButtonWidth, styles.FooterButtonHeight);
            if (GUI.Button(rect, styles.TextPrev))
            {
                setPage(page - 1);
            }

            rect.x += rect.width + styles.FooterButtonSpace;
            rect.width = styles.GetIntFieldSize(page);
            var newPage = EditorGUI.DelayedIntField(rect, page);
            if (page != newPage) setPage(newPage);

            rect.x += rect.width + styles.FooterButtonSpace;
            rect.width = 16;
            GUI.Label(rect, "of");

			rect.x += rect.width + styles.FooterButtonSpace;
			rect.width = styles.GetIntFieldSize(page);

            using (var scope = new EditorGUI.DisabledScope(true))
            {
                EditorGUI.IntField(rect, PagesTotal);
            }

            rect.x += rect.width + styles.FooterButtonSpace;
            rect.width = styles.FooterButtonWidth;
            if (GUI.Button(rect, styles.TextNext))
			{
                setPage(page + 1);
			}
        }

		private void draw(int id, Rect rect)
		{
			switch (Event.current.type)
			{
				case EventType.Repaint:
				case EventType.Layout:
					drawItem(id, rect, selectedId == id);
					break;
				case EventType.MouseDown:
					if (rect.Contains(Event.current.mousePosition))
					{
						selectedId = id;
                        Event.current.Use();
                        //GUI.changed = true;
					}
					break;
			}
		}

        private void setPage(int newPage)
        {
            if (newPage < 1) newPage = 1;
            else if (newPage > PagesTotal) newPage = PagesTotal;
            page = newPage;
        }

        private void reset()
        {
			page = 1;
			selectedId = -1;
			oldSelectedId = -1;
        }

	}
}