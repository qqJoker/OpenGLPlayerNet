using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.OpenGL;
using Avalonia.Threading;
using OpenGLPlayer.Helper;
using Silk.NET.OpenGL;

namespace OpenGLPlayer;

public class YuvPlayer : Avalonia.OpenGL.Controls.OpenGlControlBase
{
    private GL _gl;
    private YuvTexture _texture;
    private MyShader _shader;
    private BufferObject<float> _vbo;
    private BufferObject<uint> _ebo;
    private VertexArrayObject<float, uint> _vao;
    private FFmpegPlayerSharp.Base.PlayerBase _playerBase;

    /// <summary>
    /// 顶点数组
    /// </summary>
    private static readonly float[] Vertices =
    {
        1.0f, 1.0f, 0.0f, 1.0f, 1.0f,
        1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
        -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
        -1.0f, 1.0f, 0.0f, 0.0f, 1.0f
    };

    //元素数组，就是告诉GPU怎么绘制三角形，这里用两组表示绘制一个矩形
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

        //生成顶点缓冲对象
        this._vbo = new BufferObject<float>(this._gl, Vertices, BufferTargetARB.ArrayBuffer);
        this._vbo.Bind();
        //生成元素缓冲对象
        this._ebo = new BufferObject<uint>(this._gl, Indices, BufferTargetARB.ElementArrayBuffer);
        this._ebo.Bind();
        //生成顶点数组对象
        this._vao = new VertexArrayObject<float, uint>(this._gl, this._vbo, this._ebo);
        this._vao.Bind();
        //编译GLSL文件并注入到GPU内
        this._shader = new MyShader(this._gl, "GLSL/YUVShader.vert", "GLSL/YUVShader.frag");
        this._shader.Use();

        var vertexLocation = (uint)this._shader.GetAttribLocation("aPosition");
        this._vao.VertexAttributePointer(vertexLocation, 3, VertexAttribPointerType.Float, 5, 0);

        var texCoordLocation = (uint)this._shader.GetAttribLocation("aTexCoord");
        this._vao.VertexAttributePointer(texCoordLocation, 2, VertexAttribPointerType.Float, 5, 3);

        //初始化YUV纹理，YUV渲染其实和RGB渲染差不太多，RGB只占一个纹理，YUV是占三个纹理，NV12/NV21是占两个纹理
        this._texture = new YuvTexture(this._gl);

        //开始播放
        this._playerBase =
            FFmpegPlayerSharp.PlayerCollection.Instance.GetPlayer(
                "rtsp://admin:admin123@172.16.13.28:1200/record/6.mp4?id=1006&loop=1");
    }

    protected override void OnOpenGlDeinit(GlInterface gl, int fb)
    {
        this._vbo.Dispose();
        this._ebo.Dispose();
        this._vao.Dispose();
        this._shader.Dispose();
        this._texture.Dispose();
        base.OnOpenGlDeinit(gl, fb);
    }

    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        this._gl.Viewport(0, 0, (uint)Bounds.Width, (uint)Bounds.Height);

        //缓存内拿到解码后的YUV数据
        if (this._playerBase.FrameQueues.Count > 0 && this._playerBase.FrameQueues.TryDequeue(out var frame))
        {
            this._texture.Bind(frame, this._shader);
            this._shader.Use();
            this._vao.Bind();
            this._gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
        }

        //下面这个必须要加，不然不能一直触发Render事件
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual);
    }
}