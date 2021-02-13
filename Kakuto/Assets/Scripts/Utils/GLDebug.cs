using System.Collections.Generic;
using UnityEngine;

public class GLDebug : MonoBehaviour
{
    private struct Line
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;
        public float startTime;

        public Line(Vector3 start, Vector3 end, Color color, float startTime)
        {
            this.start = start;
            this.end = end;
            this.color = color;
            this.startTime = startTime;
        }

        public void Draw()
        {
            GL.Color(color);
            GL.Vertex(start);
            GL.Vertex(end);
        }
    }

    public Material GLLineZOff;

    private static GLDebug instance;
    private static Material matZOff;

    private List<Line> linesZOff;

    void Awake()
    {
        if (instance)
        {
            DestroyImmediate(this);
            return;
        }
        instance = this;
        SetMaterial();
        linesZOff = new List<Line>();
    }

    void SetMaterial()
    {
        matZOff = new Material(GLLineZOff);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        for (int i = 0; i < linesZOff.Count; i++)
        {
            Gizmos.color = linesZOff[i].color;
            Gizmos.DrawLine(linesZOff[i].start, linesZOff[i].end);
        }
    }
#endif

    void OnPostRender()
    {
        matZOff.SetPass(0);
        GL.Begin(GL.LINES);
        for(int i = 0; i < linesZOff.Count; i++)
        {
            linesZOff[i].Draw();
        }
        linesZOff.Clear();
        GL.End();
    }

    private static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        if (start == end)
            return;
        instance.linesZOff.Add(new Line(start, end, color, Time.time));
    }

    /// <summary>
    /// Draw a line from start to end with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the line is rendered 1 frame.
    /// </summary>
    /// <param name="start">Point in world space where the line should start.</param>
    /// <param name="end">Point in world space where the line should end.</param>
    /// <param name="color">Color of the line.</param>
    public static void DrawLine(Vector3 start, Vector3 end, Color? color = null)
    {
        DrawLine(start, end, color ?? Color.white);
    }

    /// <summary>
    /// Draw a line from start to start + dir with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the ray is rendered 1 frame.
    /// </summary>
    /// <param name="start">Point in world space where the ray should start.</param>
    /// <param name="dir">Direction and length of the ray.</param>
    /// <param name="color">Color of the ray.</param>
    public static void DrawRay(Vector3 start, Vector3 dir, Color? color = null)
    {
        if (dir == Vector3.zero)
            return;
        DrawLine(start, start + dir, color);
    }

    /// <summary>
    /// Draw an arrow from start to end with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the arrow is rendered 1 frame.
    /// </summary>
    /// <param name="start">Point in world space where the arrow should start.</param>
    /// <param name="end">Point in world space where the arrow should end.</param>
    /// <param name="arrowHeadLength">Length of the 2 lines of the head.</param>
    /// <param name="arrowHeadAngle">Angle between the main line and each of the 2 smaller lines of the head.</param>
    /// <param name="color">Color of the arrow.</param>
    public static void DrawLineArrow(Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null)
    {
        DrawArrow(start, end - start, arrowHeadLength, arrowHeadAngle, color);
    }

    /// <summary>
    /// Draw an arrow from start to start + dir with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the arrow is rendered 1 frame.
    /// </summary>
    /// <param name="start">Point in world space where the arrow should start.</param>
    /// <param name="dir">Direction and length of the arrow.</param>
    /// <param name="arrowHeadLength">Length of the 2 lines of the head.</param>
    /// <param name="arrowHeadAngle">Angle between the main line and each of the 2 smaller lines of the head.</param>
    /// <param name="color">Color of the arrow.</param>
    public static void DrawArrow(Vector3 start, Vector3 dir, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null)
    {
        if (dir == Vector3.zero)
            return;
        DrawRay(start, dir, color);
        Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
        DrawRay(start + dir, right * arrowHeadLength, color);
        DrawRay(start + dir, left * arrowHeadLength, color);
    }

    /// <summary>
    /// Draw a square with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the square is renderer 1 frame.
    /// </summary>
    /// <param name="pos">Center of the square in world space.</param>
    /// <param name="rot">Rotation of the square in euler angles in world space.</param>
    /// <param name="scale">Size of the square.</param>
    /// <param name="color">Color of the square.</param>
    public static void DrawSquare(Vector3 pos, Vector3? rot = null, Vector3? scale = null, Color? color = null)
    {
        DrawSquare(Matrix4x4.TRS(pos, Quaternion.Euler(rot ?? Vector3.zero), scale ?? Vector3.one), color);
    }
    /// <summary>
    /// Draw a square with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the square is renderer 1 frame.
    /// </summary>
    /// <param name="pos">Center of the square in world space.</param>
    /// <param name="rot">Rotation of the square in world space.</param>
    /// <param name="scale">Size of the square.</param>
    /// <param name="color">Color of the square.</param>
    public static void DrawSquare(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null)
    {
        DrawSquare(Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color);
    }
    /// <summary>
    /// Draw a square with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the square is renderer 1 frame.
    /// </summary>
    /// <param name="matrix">Transformation matrix which represent the square transform.</param>
    /// <param name="color">Color of the square.</param>
    public static void DrawSquare(Matrix4x4 matrix, Color? color = null)
    {
        Vector3
            p_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, .5f)),
            p_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, -.5f)),
            p_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, -.5f)),
            p_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, .5f));

        DrawLine(p_1, p_2, color);
        DrawLine(p_2, p_3, color);
        DrawLine(p_3, p_4, color);
        DrawLine(p_4, p_1, color);
    }

    /// <summary>
    /// Draw a cube with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the square is renderer 1 frame.
    /// </summary>
    /// <param name="pos">Center of the cube in world space.</param>
    /// <param name="rot">Rotation of the cube in euler angles in world space.</param>
    /// <param name="scale">Size of the cube.</param>
    /// <param name="color">Color of the cube.</param>
    public static void DrawCube(Vector3 pos, Vector3? rot = null, Vector3? scale = null, Color? color = null)
    {
        DrawCube(Matrix4x4.TRS(pos, Quaternion.Euler(rot ?? Vector3.zero), scale ?? Vector3.one), color);
    }
    /// <summary>
    /// Draw a cube with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the square is renderer 1 frame.
    /// </summary>
    /// <param name="pos">Center of the cube in world space.</param>
    /// <param name="rot">Rotation of the cube in world space.</param>
    /// <param name="scale">Size of the cube.</param>
    /// <param name="color">Color of the cube.</param>
    public static void DrawCube(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null)
    {
        DrawCube(Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color);
    }
    /// <summary>
    /// Draw a cube with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the square is renderer 1 frame.
    /// </summary>
    /// <param name="matrix">Transformation matrix which represent the cube transform.</param>
    /// <param name="color">Color of the cube.</param>
    public static void DrawCube(Matrix4x4 matrix, Color? color = null)
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

        DrawLine(down_1, down_2, color);
        DrawLine(down_2, down_3, color);
        DrawLine(down_3, down_4, color);
        DrawLine(down_4, down_1, color);

        DrawLine(down_1, up_1, color);
        DrawLine(down_2, up_2, color);
        DrawLine(down_3, up_3, color);
        DrawLine(down_4, up_4, color);

        DrawLine(up_1, up_2, color);
        DrawLine(up_2, up_3, color);
        DrawLine(up_3, up_4, color);
        DrawLine(up_4, up_1, color);
    }
}