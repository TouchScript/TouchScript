/*
 * @author Valentin Simonov / http://va.lent.in/
 * Based on http://pastebin.com/69QP1s45
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TouchScript.Utils.Graphics
{
    public class GLDebug : MonoBehaviour
    {
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
                        go.hideFlags = HideFlags.HideAndDontSave;
                        camera.hideFlags = HideFlags.HideAndDontSave;
                    }
                    _instance.hideFlags = HideFlags.HideAndDontSave;
                }
                return _instance;
            }
        }

        public KeyCode ToggleKey;
        public bool DisplayLines = true;

        private static GLDebug _instance;

        private static int nextFigureId = 1;
        private Material matZOn;
        private Material matZOff;
        private List<Figure> figuresZOn;
        private List<Figure> figuresZOff;
        private List<Figure> figuresScreenSpace;

        #region Public methods

        public static void RemoveFigure(int id)
        {
            instance.figuresZOn = instance.figuresZOn.Where(l => l.Id != id).ToList();
            instance.figuresZOff = instance.figuresZOff.Where(l => l.Id != id).ToList();
            instance.figuresScreenSpace = instance.figuresScreenSpace.Where(l => l.Id != id).ToList();
        }

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

        public static int DrawLineArrow(Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawLineArrow(null, start, end, arrowHeadLength, arrowHeadAngle, color, duration, depthTest);
        }

        public static int DrawLineArrow(int? id, Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawArrow(start, end - start, arrowHeadLength, arrowHeadAngle, color, duration, depthTest);
        }

        public static int DrawArrow(Vector3 start, Vector3 dir, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
        {
            return DrawArrow(null, start, dir, arrowHeadLength, arrowHeadAngle, color, duration, depthTest);
        }

        public static int DrawArrow(int? id, Vector3 start, Vector3 dir, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
        {
            if (dir == Vector3.zero)
                return 0;

            Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
            var sd = start + dir;

            var lines = new List<Line>()
            {
                new Line(start, sd),
                new Line(sd, sd + right * arrowHeadLength),
                new Line(sd, sd + left * arrowHeadLength)
            };
            return drawFigure(id, lines, color ?? Color.white, duration, depthTest);
        }

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
            Vector3
                p_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, .5f)),
                p_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, -.5f)),
                p_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, -.5f)),
                p_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, .5f));

            var lines = new List<Line>()
            {
                new Line(p_1, p_2),
                new Line(p_2, p_3),
                new Line(p_3, p_4),
                new Line(p_4, p_1)
            };
            return drawFigure(id, lines, color ?? Color.white, duration, depthTest);
        }

        public static int DrawSquareScreenSpace(Vector2 pos, float rot = 0, Vector2? scale = null, Color? color = null, float duration = 0)
        {
            return DrawSquareScreenSpace(null, pos, rot, scale, color, duration);
        }

        public static int DrawSquareScreenSpace(int? id, Vector2 pos, float rot = 0, Vector2? scale = null, Color? color = null, float duration = 0)
        {
            Vector2 s = scale ?? Vector2.one;

            Vector2
                p_1 = new Vector2(.5f * s.x, .5f * s.y) + pos,
                p_2 = new Vector2(.5f * s.x, -.5f * s.y) + pos,
                p_3 = new Vector2(-.5f * s.x, -.5f * s.y) + pos,
                p_4 = new Vector2(-.5f * s.x, .5f * s.y) + pos;

            var lines = new List<Line>()
            {
                new Line(p_1, p_2),
                new Line(p_2, p_3),
                new Line(p_3, p_4),
                new Line(p_4, p_1)
            };
            return drawFigureScreenSpace(id, lines, color ?? Color.white, duration);
        }

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
            Vector3
                down_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, -.5f, .5f)),
                down_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, -.5f, -.5f)),
                down_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, -.5f, -.5f)),
                down_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, -.5f, .5f)),
                up_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, .5f, .5f)),
                up_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, .5f, -.5f)),
                up_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, .5f, -.5f)),
                up_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, .5f, .5f));

            var lines = new List<Line>()
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
            return drawFigure(id, lines, color ?? Color.white, duration, depthTest);
        }

        #endregion

        #region Unity methods

        private void Awake()
        {
            if (_instance)
            {
                DestroyImmediate(this);
                return;
            }

            _instance = this;
            figuresZOn = new List<Figure>();
            figuresZOff = new List<Figure>();
            figuresScreenSpace = new List<Figure>();

            setMaterials();
        }

        private void Update()
        {
            if (Input.GetKeyDown(ToggleKey))
                DisplayLines = !DisplayLines;

            if (!DisplayLines)
            {
                figuresZOn = figuresZOn.Where(l => !l.DurationElapsed(false)).ToList();
                figuresZOff = figuresZOff.Where(l => !l.DurationElapsed(false)).ToList();
                figuresScreenSpace = figuresScreenSpace.Where(l => !l.DurationElapsed(false)).ToList();
            }
        }

        private void OnPostRender()
        {
            if (!DisplayLines) return;

            matZOn.SetPass(0);
            GL.Begin(GL.LINES);
            figuresZOn = figuresZOn.Where(l => !l.DurationElapsed(true)).ToList();
            GL.End();

            matZOff.SetPass(0);
            GL.Begin(GL.LINES);
            figuresZOff = figuresZOff.Where(l => !l.DurationElapsed(true)).ToList();
            GL.End();

            GL.PushMatrix();
            matZOff.SetPass(0);
            GL.LoadPixelMatrix();
            GL.Begin(GL.LINES);
            figuresScreenSpace = figuresScreenSpace.Where(l => !l.DurationElapsed(true)).ToList();
            GL.End();
            GL.PopMatrix();
        }

        #endregion

        #region Private functions

        private void setMaterials()
        {
            matZOn = new Material(Shader.Find("Hidden/GLLineZOn"));
            matZOn.hideFlags = HideFlags.HideAndDontSave;
            matZOff = new Material(Shader.Find("Hidden/GLLineZOff"));
            matZOff.hideFlags = HideFlags.HideAndDontSave;
        }

        private static int drawFigure(int? id, List<Line> lines, Color color, float duration = 0, bool depthTest = false)
        {
            if (duration == 0 && !instance.DisplayLines)
                return 0;

            int figureId = id ?? nextFigureId++;
            if (depthTest)
                instance.figuresZOn.Add(new Figure(figureId, lines, color, Time.time, duration));
            else
                instance.figuresZOff.Add(new Figure(figureId, lines, color, Time.time, duration));
            return figureId;
        }

        private static int drawFigureScreenSpace(int? id, List<Line> lines, Color color, float duration = 0)
        {
            if (duration == 0 && !instance.DisplayLines)
                return 0;

            int figureId = id ?? nextFigureId++;
            instance.figuresScreenSpace.Add(new Figure(figureId, lines, color, Time.time, duration));
            return figureId;
        }

        #endregion

        #region Structs

        private struct Figure
        {
            public int Id;
            public Color Color;
            public float StartTime;
            public float Duration;
            public List<Line> Lines;

            public Figure(int id, List<Line> lines, Color color, float startTime, float duration)
            {
                Id = id;
                Color = color;
                StartTime = startTime;
                Duration = duration;
                Lines = lines;
            }

            public bool DurationElapsed(bool drawLines)
            {
                if (drawLines)
                {
                    GL.Color(Color);
                    for (var i = 0; i < Lines.Count; i++)
                    {
                        Lines[i].Draw();
                    }
                }
                return Time.time - StartTime >= Duration;
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
