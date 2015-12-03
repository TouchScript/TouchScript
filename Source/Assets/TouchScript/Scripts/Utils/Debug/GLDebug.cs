/*
 * @author Valentin Simonov / http://va.lent.in/
 * Based on http://pastebin.com/69QP1s45
 */


#if TOUCHSCRIPT_DEBUG

using System.Collections.Generic;
using UnityEngine;

namespace TouchScript.Utils.Debug 
{
    public class GLDebug : MonoBehaviour
    {

        public static readonly Color MULTIPLY = new Color(0, 0, 0, 0);
        public static readonly Vector2 DEFAULT_SCREEN_SPACE_SCALE = new Vector2(10, 10);

        private static GLDebug instance
        {
            get
            {
                if (!_instance && Application.isPlaying)
                {
                    if (Camera.main)
                    {
                        _instance = Camera.main.gameObject.AddComponent<GLDebug>();
                    }
                    else
                    {
                        var go = new GameObject("GLDebug");
                        var camera = go.AddComponent<Camera>();
                        camera.clearFlags = CameraClearFlags.Nothing;
                        camera.depth = 9000;
                        _instance = go.AddComponent<GLDebug>();
                    }
                }
                return _instance;
            }
        }

        public KeyCode ToggleKey;
        public bool DisplayLines = true;

        private static GLDebug _instance;

        private static int nextFigureId = 1;
        private Material materialDepthTest;
        private Material materialNoDepthTest;
        private Material materialMultiplyDepthTest;
        private Material materialMultiplyNoDepthTest;
        private Dictionary<int, Figure> figuresDepthTest;
        private Dictionary<int, Figure> figuresMultiplyDepthTest;
        private Dictionary<int, Figure> figuresNoDepthTest;
        private Dictionary<int, Figure> figuresMultiplyNoDepthTest;
        private Dictionary<int, Figure> figuresScreenSpace;
        private Dictionary<int, Figure> figuresMultiplyScreenSpace;
        private Dictionary<int, Figure> figuresTmp;

        #region Public methods

        public static void RemoveFigure(int id)
        {
            instance.figuresDepthTest.Remove(id);
            instance.figuresNoDepthTest.Remove(id);
            instance.figuresScreenSpace.Remove(id);
            instance.figuresMultiplyDepthTest.Remove(id);
            instance.figuresMultiplyNoDepthTest.Remove(id);
            instance.figuresMultiplyScreenSpace.Remove(id);
        }

        #region Line

        public static int DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawLine(null, start, end, color, duration, depthTest);
        }

        public static int DrawLine(int? id, Vector3 start, Vector3 end, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return drawFigure(id, new List<Line>() { new Line(start, end) }, color ?? Color.white, duration, depthTest);
        }

        public static int DrawLineScreenSpace(Vector2 start, Vector2 end, Color? color = null, float duration = 0)
        {
            return DrawLineScreenSpace(null, start, end, color, duration);
        }

        public static int DrawLineScreenSpace(int? id, Vector2 start, Vector2 end, Color? color = null, float duration = 0)
        {
            return drawFigureScreenSpace(id, new List<Line>() { new Line(start, end) }, color ?? Color.white, duration);
        }

        #endregion

        #region Ray

        public static int DrawRay(Vector3 start, Vector3 dir, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawRay(null, start, dir, color, duration, depthTest);
        }

        public static int DrawRay(int? id, Vector3 start, Vector3 dir, Color? color = null, float duration = 0, bool depthTest = false)
        {
            if (dir == Vector3.zero)
                return 0;
            return DrawLine(start, start + dir, color, duration, depthTest);
        }

        #endregion

        #region Cross

        public static int DrawCross(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawCross(null, Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color, duration, depthTest);
        }

        public static int DrawCross(int? id, Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawCross(id, Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color, duration, depthTest);
        }

        public static int DrawCross(Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawCross(null, matrix, color, duration, depthTest);
        }

        public static int DrawCross(int? id, Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return drawFigure(id, createCrossLines(matrix), color ?? Color.white, duration, depthTest);
        }

        public static int DrawCrossScreenSpace(Vector2 pos, float rot = 0, Vector2? scale = null, Color? color = null, float duration = 0)
        {
            return DrawCrossScreenSpace(null, pos, rot, scale, color, duration);
        }

        public static int DrawCrossScreenSpace(int? id, Vector2 pos, float rot = 0, Vector2? scale = null, Color? color = null, float duration = 0)
        {
            return drawFigureScreenSpace(id, createScreenSpaceCrossLines(pos, rot, scale ?? DEFAULT_SCREEN_SPACE_SCALE), color ?? Color.white, duration);
        }

        #endregion

        #region Arrow

        public static int DrawArrow(Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawArrow(null, start, end, arrowHeadLength, arrowHeadAngle, color, duration, depthTest);
        }

        public static int DrawArrow(int? id, Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
        {
            if (start == end)
                return 0;

            return drawFigure(id, createArrowLines(start, end, arrowHeadLength, arrowHeadAngle), color ?? Color.white, duration, depthTest);
        }

        #endregion

        #region Plane with normal

        public static int DrawPlaneWithNormal(Vector3 pos, Vector3 normal, float scale = 1f, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawPlaneWithNormal(null, pos, normal, scale, color, duration, depthTest);
        }

        public static int DrawPlaneWithNormal(int? id, Vector3 pos, Vector3 normal, float scale = 1f, Color? color = null, float duration = 0, bool depthTest = false)
        {
            var lines = createArrowLines(pos, pos + normal);
            lines.AddRange(createCrossLines(Matrix4x4.TRS(pos, Quaternion.LookRotation(normal) * Quaternion.Euler(0, 0, 45f), Vector3.one)));
            lines.AddRange(createSquareLines(Matrix4x4.TRS(pos, Quaternion.FromToRotation(Vector3.up, normal), Vector3.one * scale)));
            return drawFigure(id, lines, color ?? Color.white, duration, depthTest);
        }

        #endregion

        #region Line with cross

        public static int DrawLineWithCross(Vector3 start, Vector3 end, float crossRelativePosition = 0.5f, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawLineWithCross(null, start, end, crossRelativePosition, scale, color, duration, depthTest);
        }

        public static int DrawLineWithCross(int? id, Vector3 start, Vector3 end, float crossRelativePosition = 0.5f, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
        {
            var lines = new List<Line>() {new Line(start, end)};
            // TODO: Calculate cross rotation
            lines.AddRange(createCrossLines(Matrix4x4.TRS(Vector3.Lerp(start, end, crossRelativePosition), Quaternion.identity, scale ?? Vector3.one)));
            return drawFigure(id, lines, color ?? Color.white, duration, depthTest);
        }

        public static int DrawLineWithCrossScreenSpace(Vector2 start, Vector2 end, float crossRelativePosition, Vector2? scale = null, Color? color = null, float duration = 0)
        {
            return DrawLineWithCrossScreenSpace(null, start, end, crossRelativePosition, scale, color, duration);
        }

        public static int DrawLineWithCrossScreenSpace(int? id, Vector2 start, Vector2 end, float crossRelativePosition, Vector2? scale = null, Color? color = null, float duration = 0)
        {
            var lines = new List<Line>() {new Line(start, end)};
            lines.AddRange(createScreenSpaceCrossLines(Vector2.Lerp(start, end, crossRelativePosition), Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg + 45f, scale ?? Vector2.one * 10));
            return drawFigureScreenSpace(id, lines, color ?? Color.white, duration);
        }

        #endregion

        #region Square

        public static int DrawSquare(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawSquare(null, pos, rot, scale, color, duration, depthTest);
        }

        public static int DrawSquare(int? id, Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawSquare(Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color, duration, depthTest);
        }

        public static int DrawSquare(Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawSquare(null, matrix, color, duration, depthTest);
        }

        public static int DrawSquare(int? id, Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return drawFigure(id, createSquareLines(matrix), color ?? Color.white, duration, depthTest);
        }

        public static int DrawSquareScreenSpace(Vector2 pos, float rot = 0, Vector2? scale = null, Color? color = null, float duration = 0)
        {
            return DrawSquareScreenSpace(null, pos, rot, scale, color, duration);
        }

        public static int DrawSquareScreenSpace(int? id, Vector2 pos, float rot = 0, Vector2? scale = null, Color? color = null, float duration = 0)
        {
            return drawFigureScreenSpace(id, createScreenSpaceSquareLines(pos, rot, scale ?? DEFAULT_SCREEN_SPACE_SCALE), color ?? Color.white, duration);
        }

        #endregion

        #region Cube

        public static int DrawCube(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawCube(null, pos, rot, scale, color, duration, depthTest);
        }

        public static int DrawCube(int? id, Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawCube(Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color, duration, depthTest);
        }

        public static int DrawCube(Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawCube(null, matrix, color, duration, depthTest);
        }

        public static int DrawCube(int? id, Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return drawFigure(id, createCubeLines(matrix), color ?? Color.white, duration, depthTest);
        }

        #endregion

        #endregion

        #region Unity methods

        private void Awake()
        {
            if (_instance)
            {
                Destroy(this);
                return;
            }

            _instance = this;
            figuresDepthTest = new Dictionary<int, Figure>();
            figuresNoDepthTest = new Dictionary<int, Figure>();
            figuresScreenSpace = new Dictionary<int, Figure>();
            figuresMultiplyDepthTest = new Dictionary<int, Figure>();
            figuresMultiplyNoDepthTest = new Dictionary<int, Figure>();
            figuresMultiplyScreenSpace = new Dictionary<int, Figure>();
            figuresTmp = new Dictionary<int, Figure>();

            setMaterials();
        }

        private void Update()
        {
            if (Input.GetKeyDown(ToggleKey))
                DisplayLines = !DisplayLines;
        }

        private void OnPostRender()
        {
            if (!DisplayLines) return;

            materialDepthTest.SetPass(0);
            GL.Begin(GL.LINES);
            figuresDepthTest = draw(figuresDepthTest);
            GL.End();

            materialMultiplyDepthTest.SetPass(0);
            GL.Begin(GL.LINES);
            figuresMultiplyDepthTest = draw(figuresMultiplyDepthTest);
            GL.End();

            materialNoDepthTest.SetPass(0);
            GL.Begin(GL.LINES);
            figuresNoDepthTest = draw(figuresNoDepthTest);
            GL.End();

            materialMultiplyNoDepthTest.SetPass(0);
            GL.Begin(GL.LINES);
            figuresMultiplyNoDepthTest = draw(figuresMultiplyNoDepthTest);
            GL.End();

            GL.PushMatrix();
            GL.LoadPixelMatrix();

            materialNoDepthTest.SetPass(0);
            GL.Begin(GL.LINES);
            figuresScreenSpace = draw(figuresScreenSpace);
            GL.End();

            materialMultiplyNoDepthTest.SetPass(0);
            GL.Begin(GL.LINES);
            figuresMultiplyScreenSpace = draw(figuresMultiplyScreenSpace);
            GL.End();
            GL.PopMatrix();
        }

        #endregion

        #region Private functions

        #region Misc

        private Dictionary<int, Figure> draw(Dictionary<int, Figure> figures)
        {
            figuresTmp.Clear();
            var newFigures = figuresTmp;
            foreach (var key in figures.Keys)
            {
                var value = figures[key];
                value.Duration = value.Draw();
                if (value.Duration > 0)
                {
                    newFigures[key] = value;
                }
            }
            figuresTmp = figures;
            return newFigures;
        }

        private void setMaterials()
        {
            materialDepthTest = new Material(Shader.Find("Hidden/DebugDepthTest"));
            materialNoDepthTest = new Material(Shader.Find("Hidden/DebugNoDepthTest"));
            materialMultiplyDepthTest = new Material(Shader.Find("Hidden/DebugMultiplyDepthTest"));
            materialMultiplyNoDepthTest = new Material(Shader.Find("Hidden/DebugMultiplyNoDepthTest"));
            materialDepthTest.hideFlags = HideFlags.HideAndDontSave;
            materialNoDepthTest.hideFlags = HideFlags.HideAndDontSave;
            materialMultiplyDepthTest.hideFlags = HideFlags.HideAndDontSave;
            materialMultiplyNoDepthTest.hideFlags = HideFlags.HideAndDontSave;
        }

        #endregion

        #region Figure creation

        private static int drawFigure(int? id, List<Line> lines, Color color, float duration = 0, bool depthTest = false)
        {
            if (duration == 0 && !instance.DisplayLines)
                return 0;

            int figureId;
            if (id.HasValue)
            {
                figureId = id.Value;
                RemoveFigure(figureId);
            }
            else
            {
                figureId = nextFigureId++;
            }
            if (depthTest)
            {
                if (color == MULTIPLY) instance.figuresMultiplyDepthTest.Add(figureId, new Figure(figureId, lines, Color.white, duration));
                else instance.figuresDepthTest.Add(figureId, new Figure(figureId, lines, color, duration));
            }
            else
            {
                if (color == MULTIPLY) instance.figuresMultiplyNoDepthTest.Add(figureId, new Figure(figureId, lines, Color.white, duration));
                else instance.figuresNoDepthTest.Add(figureId, new Figure(figureId, lines, color, duration));
            }
            return figureId;
        }

        private static int drawFigureScreenSpace(int? id, List<Line> lines, Color color, float duration = 0)
        {
            if (duration == 0 && !instance.DisplayLines)
                return 0;

            int figureId;
            if (id.HasValue)
            {
                figureId = id.Value;
                RemoveFigure(figureId);
            }
            else
            {
                figureId = nextFigureId++;
            }

            if (color == MULTIPLY) instance.figuresMultiplyScreenSpace.Add(figureId, new Figure(figureId, lines, Color.white, duration));
            else instance.figuresScreenSpace.Add(figureId, new Figure(figureId, lines, color, duration));
            return figureId;
        }

        #endregion

        #region Line helpers

        private static List<Line> createCrossLines(Matrix4x4 matrix)
        {
            Vector3
                p_1 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, 0)),
                p_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, 0)),
                p_3 = matrix.MultiplyPoint3x4(new Vector3(0, -.5f, 0)),
                p_4 = matrix.MultiplyPoint3x4(new Vector3(0, .5f, 0));

            return new List<Line>()
            {
                new Line(p_1, p_2),
                new Line(p_3, p_4),
            };
        }

        private static List<Line> createScreenSpaceCrossLines(Vector2 pos, float rot, Vector2 scale)
        {
            Vector2 p_1, p_2, p_3, p_4;
            float x = .5f * scale.x;
            float y = .5f * scale.y;

            if (rot == 0)
            {
                p_1 = new Vector2(-x, 0) + pos;
                p_2 = new Vector2(x, 0) + pos;
                p_3 = new Vector2(0, -y) + pos;
                p_4 = new Vector2(0, y) + pos;
            }
            else
            {
                var cos = Mathf.Cos(rot * Mathf.Deg2Rad);
                var sin = Mathf.Sin(rot * Mathf.Deg2Rad);

                p_1 = new Vector2(-x * cos, -x * sin) + pos;
                p_2 = new Vector2(x * cos, x * sin) + pos;
                p_3 = new Vector2(y * sin, -y * cos) + pos;
                p_4 = new Vector2(-y * sin, y * cos) + pos;
            }

            return new List<Line>()
            {
                new Line(p_1, p_2),
                new Line(p_3, p_4),
            };
        }

        private static List<Line> createArrowLines(Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20)
        {
            var dir = end - start;
            Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;

            return new List<Line>()
            {
                new Line(start, end),
                new Line(end, end + right * arrowHeadLength),
                new Line(end, end + left * arrowHeadLength)
            };
        }

        private static List<Line> createSquareLines(Matrix4x4 matrix)
        {
            Vector3
                p_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, .5f)),
                p_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, -.5f)),
                p_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, -.5f)),
                p_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, .5f));

            return new List<Line>()
            {
                new Line(p_1, p_2),
                new Line(p_2, p_3),
                new Line(p_3, p_4),
                new Line(p_4, p_1)
            };
        }

        private static List<Line> createScreenSpaceSquareLines(Vector2 pos, float rot, Vector2 scale)
        {
            Vector2 p_1, p_2, p_3, p_4;
            float x = .5f * scale.x;
            float y = .5f * scale.y;

            if (rot == 0)
            {
                p_1 = new Vector2(x, y) + pos;
                p_2 = new Vector2(x, -y) + pos;
                p_3 = new Vector2(-x, -y) + pos;
                p_4 = new Vector2(-x, y) + pos;
            }
            else
            {
                var cos = Mathf.Cos(rot * Mathf.Deg2Rad);
                var sin = Mathf.Sin(rot * Mathf.Deg2Rad);

                p_1 = new Vector2(x * cos - y * sin, x * sin + y * cos) + pos;
                p_2 = new Vector2(x * cos + y * sin, x * sin - y * cos) + pos;
                p_3 = new Vector2(-x * cos + y * sin, -x * sin - y * cos) + pos;
                p_4 = new Vector2(-x * cos - y * sin, -x * sin + y * cos) + pos;
            }

            return new List<Line>()
            {
                new Line(p_1, p_2),
                new Line(p_2, p_3),
                new Line(p_3, p_4),
                new Line(p_4, p_1)
            };
        }

        private static List<Line> createCubeLines(Matrix4x4 matrix)
        {
            Vector3
                down_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, -.5f, .5f)),
                down_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, -.5f, -.5f)),
                down_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, -.5f, -.5f)),
                down_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, -.5f, .5f)),
                up_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, .5f, .5f)),
                up_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, .5f, -.5f)),
                up_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, .5f, -.5f)),
                up_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, .5f, .5f));

            return new List<Line>()
            {
                new Line(down_1, down_2),
                new Line(down_2, down_3),
                new Line(down_3, down_4),
                new Line(down_4, down_1),

                new Line(down_1, up_1),
                new Line(down_2, up_2),
                new Line(down_3, up_3),
                new Line(down_4, up_4),

                new Line(up_1, up_2),
                new Line(up_2, up_3),
                new Line(up_3, up_4),
                new Line(up_4, up_1)
            };
        }

        #endregion

        #endregion

        #region Structs

        private struct Figure
        {
            public int Id;
            public Color Color;
            public float Duration;
            public List<Line> Lines;

            public Figure(int id, List<Line> lines, Color color, float duration)
            {
                Id = id;
                Color = color;
                Duration = duration;
                Lines = lines;
            }

            public float Draw()
            {
                GL.Color(Color);
                for (var i = 0; i < Lines.Count; i++)
                {
                    Lines[i].Draw();
                }
                return Duration - Time.deltaTime;
            }
        }

        private struct Line
        {
            public Vector3 start;
            public Vector3 end;

            public Line(Vector3 start, Vector3 end)
            {
                this.start = start;
                this.end = end;
            }

            public void Draw()
            {
                GL.Vertex(start);
                GL.Vertex(end);
            }
        }

        #endregion

    }
}

#endif