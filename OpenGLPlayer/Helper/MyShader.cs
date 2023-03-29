using Silk.NET.OpenGL;

namespace OpenGLPlayer.Helper;

public sealed class MyShader : IDisposable
{
    private bool _disposedValue;
    private readonly GL _gl;
    private readonly uint _handle;

    public MyShader(GL gl, string vertexPath, string fragPath)
    {
        this._gl = gl;
        var vertexSource = File.ReadAllText(vertexPath);
        var fragmentSource = File.ReadAllText(fragPath);

        var vertexShader = this._gl.CreateShader(ShaderType.VertexShader);
        this._gl.ShaderSource(vertexShader, vertexSource);

        var fragmentShader = this._gl.CreateShader(ShaderType.FragmentShader);
        this._gl.ShaderSource(fragmentShader, fragmentSource);

        this._gl.CompileShader(vertexShader);
        this._gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out var status);
        if (status == 0)
            throw new Exception($"{this._gl.GetShaderInfoLog(vertexShader)}");

        this._gl.CompileShader(fragmentShader);
        this._gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out status);
        if (status == 0)
            throw new Exception($"{this._gl.GetShaderInfoLog(fragmentShader)}");

        this._handle = this._gl.CreateProgram();
        this._gl.AttachShader(this._handle, vertexShader);
        this._gl.AttachShader(this._handle, fragmentShader);
        this._gl.LinkProgram(this._handle);
        this._gl.GetProgram(this._handle, ProgramPropertyARB.LinkStatus, out status);
        if (status == 0)
            throw new Exception($"{this._gl.GetProgramInfoLog(this._handle)}");


        this._gl.DetachShader(this._handle, vertexShader);
        this._gl.DetachShader(this._handle, fragmentShader);
        this._gl.DeleteShader(vertexShader);
        this._gl.DeleteShader(fragmentShader);
    }

    public void Use() => this._gl.UseProgram(this._handle);

    public int GetAttribLocation(string attribName) => this._gl.GetAttribLocation(this._handle, attribName);

    public void SetUniform(string name, int value)
    {
        var location = this._gl.GetUniformLocation(this._handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform not found on shader.");
        this._gl.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        var location = this._gl.GetUniformLocation(this._handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform not found on shader");
        this._gl.Uniform1(location, value);
    }

    public void Dispose()
    {
        if (!this._disposedValue)
        {
            this._gl.DeleteProgram(this._handle);
            this._disposedValue = true;
        }
        GC.SuppressFinalize(this);
    }

    ~MyShader()
    {
        this._gl.DeleteProgram(this._handle);
    }
}