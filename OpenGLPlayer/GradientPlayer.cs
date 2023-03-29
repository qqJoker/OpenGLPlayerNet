using Avalonia.OpenGL;
using Avalonia.Threading;
using OpenGLPlayer.Helper;
using Silk.NET.OpenGL;

namespace OpenGLPlayer;

public class GradientPlayer : Avalonia.OpenGL.Controls.OpenGlControlBase
{
    private GL _gl;
    private BufferObject<float> _vbo;
    private BufferObject<uint> _ebo;
    private VertexArrayObject<float, uint> _vao;
    // private Shader _shader;
    private MyShader _shader;
    private static readonly float[] Vertices =
    {
        //X    Y      Z     R  G  B  A
        1f, 1f, 0.0f, 1, 0, 0, 1,
        1f, -1f, 0.0f, 0, 0, 0, 1,
        -1f, -1f, 0.0f, 0, 0, 1, 1,
        -1f, 1f, 0.0f, 0, 0, 0, 1
    };

    private static readonly uint[] Indices =
    {
        0, 1, 3,
        1, 2, 3
    };

    protected override void OnOpenGlInit(GlInterface gl, int fb)
    {
        base.OnOpenGlInit(gl, fb);
        this._gl = GL.GetApi(gl.GetProcAddress);
        _gl.ClearColor(System.Drawing.Color.Black);
        _ebo = new BufferObject<uint>(_gl, Indices, BufferTargetARB.ElementArrayBuffer);
        _vbo = new BufferObject<float>(_gl, Vertices, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 7, 0);
        _vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 7, 3);

        _shader = new MyShader(_gl, "GLSL/Shader.vert", "GLSL/Shader.frag");
    }

    protected override void OnOpenGlDeinit(GlInterface gl, int fb)
    {
        _vbo.Dispose();
        _ebo.Dispose();
        _vao.Dispose();
        _shader.Dispose();
        base.OnOpenGlDeinit(gl, fb);
    }

    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        // Gl.ClearColor(System.Drawing.Color.Black);
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit));
        _gl.Viewport(0, 0, (uint)Bounds.Width, (uint)Bounds.Height);

        _ebo.Bind();
        _vbo.Bind();
        _vao.Bind();
        _shader.Use();
        _shader.SetUniform("uBlue", (float)Math.Sin(DateTime.Now.Millisecond / 1000f * Math.PI));

        _gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }
}