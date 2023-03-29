using Silk.NET.OpenGL;

namespace OpenGLPlayer.Helper;

public class RgbTexture
{
    private readonly GL _gl;
    public readonly uint Handle;

    public RgbTexture(GL gl)
    {
        this._gl = gl;
        this.Handle = gl.GenTexture();
        this._gl.ActiveTexture(TextureUnit.Texture0);
        this._gl.BindTexture(TextureTarget.Texture2D, this.Handle);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        this._gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public unsafe void Bind(FFmpegPlayerSharp.Core.Models.FrameCacheModel frame)
    {
        this._gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)frame.Width, (uint)frame.Height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, frame.Data[0]);
    }

    public void Use()
    {
        this._gl.ActiveTexture(TextureUnit.Texture0);
        this._gl.BindTexture(TextureTarget.Texture2D, this.Handle);
    }

    public void Dispose()
    {
        this._gl.DeleteTexture(this.Handle);
    }
}