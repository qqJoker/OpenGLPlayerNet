using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Animation;
using Avalonia.OpenGL;
using Avalonia.Threading;
using OpenGLPlayer.Helper;
using Silk.NET.OpenGL;

namespace OpenGLPlayer;

public class RgbaPlayer : Avalonia.OpenGL.Controls.OpenGlControlBase
{
    private GL _gl;
    private FFmpegPlayerSharp.Base.PlayerBase _playerBase;

    private RgbTexture _texture;
    private MyShader _shader;
    private uint _vbo;
    private uint _vao;
    private uint _ebo;

    private static readonly float[] Vertices =
    {
        //X    Y      Z    
        1.00f, 1.0f, 0.0f, 1.0f, 1.0f,
        1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
        -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
        -1.0f, 1.0f, 0.0f, 0.0f, 1.0f
    };

    private static readonly uint[] Indices =
    {
        0, 1, 3,
        1, 2, 3
    };

    protected override unsafe void OnOpenGlInit(GlInterface gl, int fb)
    {
        base.OnOpenGlInit(gl, fb);

        this._gl = GL.GetApi(gl.GetProcAddress);
        this._gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        this._vao = this._gl.GenVertexArray();
        this._gl.BindVertexArray(this._vao);

        this._vbo = this._gl.GenBuffer();
        this._gl.BindBuffer(BufferTargetARB.ArrayBuffer, this._vbo);
        this._gl.BufferData<float>(BufferTargetARB.ArrayBuffer, (uint)(Vertices.Length * sizeof(float)), Vertices,
            BufferUsageARB.StaticDraw);

        this._ebo = this._gl.GenBuffer();
        this._gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, this._ebo);
        this._gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer, (uint)(Indices.Length * sizeof(uint)), Indices,
            BufferUsageARB.StaticDraw);

        this._shader = new MyShader(this._gl, "GLSL/RgbaShader.vert", "GLSL/RgbaShader.frag");
        this._shader.Use();

        var vertexLocation = (uint)this._shader.GetAttribLocation("aPosition");
        this._gl.EnableVertexAttribArray(vertexLocation);
        this._gl.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float),
            (void*)0);

        var texCoordLocation = (uint)this._shader.GetAttribLocation("aTexCoord");
        this._gl.EnableVertexAttribArray(texCoordLocation);
        this._gl.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float),
            (void*)(3 * sizeof(float)));

        this._texture = new RgbTexture(this._gl);
        this._texture.Use();

        this._playerBase =
            FFmpegPlayerSharp.PlayerCollection.Instance.GetPlayer(
                "rtsp://admin:admin123@172.16.13.28:1200/record/6.mp4?id=1006&loop=1");
    }

    protected override void OnOpenGlDeinit(GlInterface gl, int fb)
    {
        this._gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        this._gl.BindVertexArray(0);
        this._gl.UseProgram(0);
        this._gl.DeleteBuffer(this._vbo);
        this._gl.DeleteVertexArray(this._vao);

        this._shader.Dispose();
        this._texture.Dispose();
        base.OnOpenGlDeinit(gl, fb);
    }

    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        // this._gl.Clear(ClearBufferMask.ColorBufferBit);
        this._gl.Viewport(0, 0, (uint)Bounds.Width, (uint)Bounds.Height);

        if (this._playerBase.FrameQueues.Count > 0 && this._playerBase.FrameQueues.TryDequeue(out var frame))
        {
            this._gl.BindVertexArray(this._vao);
            this._texture.Bind(frame);
            this._texture.Use();
            this._shader.Use();
            this._gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
        }

        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }
}

