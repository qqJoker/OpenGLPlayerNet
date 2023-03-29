using Silk.NET.OpenGL;

namespace OpenGLPlayer.Helper;

public class YuvTexture : IDisposable
{
     private readonly GL _gl;
    private readonly uint _handleY;
    private readonly uint _handleU;
    private readonly uint _handleV;

    public YuvTexture(GL gl)
    {
        this._gl = gl;
        
        //Y
        this._handleY = this._gl.GenTexture();
        this._gl.ActiveTexture(TextureUnit.Texture0);
        this._gl.BindTexture(TextureTarget.Texture2D, _handleY);
        this._gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        this._gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        this._gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        this._gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        this._gl.GenerateMipmap(TextureTarget.Texture2D);

        //U
        this._handleU = this._gl.GenTexture();
        this._gl.ActiveTexture(TextureUnit.Texture1);
        this._gl.BindTexture(TextureTarget.Texture2D, _handleU);
        this._gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        this._gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        this._gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        this._gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        this._gl.GenerateMipmap(TextureTarget.Texture2D);

        //V
        this._handleV = this._gl.GenTexture();
        this._gl.ActiveTexture(TextureUnit.Texture2);
        this._gl.BindTexture(TextureTarget.Texture2D, _handleV);
        this._gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        this._gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        this._gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        this._gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        this._gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public unsafe void Bind(FFmpegPlayerSharp.Core.Models.FrameCacheModel frame, MyShader shader)
    {
        this._gl.ActiveTexture(TextureUnit.Texture0);
        this._gl.BindTexture(TextureTarget.Texture2D, this._handleY);
        this._gl.PixelStore(PixelStoreParameter.UnpackRowLength, frame.LineSizeArr[0]);
        this._gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Red, (uint)frame.Width, (uint)frame.Height, 0,
            PixelFormat.Red, PixelType.UnsignedByte, frame.Data[0]);
        shader.SetUniform("tex_y", 0);

        this._gl.ActiveTexture(TextureUnit.Texture1);
        this._gl.BindTexture(TextureTarget.Texture2D, this._handleU);
        this._gl.PixelStore(PixelStoreParameter.UnpackRowLength, frame.LineSizeArr[1]);
        this._gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Red, (uint)frame.Width / 2,
            (uint)frame.Height / 2, 0,
            PixelFormat.Red, PixelType.UnsignedByte, frame.Data[1]);
        shader.SetUniform("tex_u", 1);

        this._gl.ActiveTexture(TextureUnit.Texture2);
        this._gl.BindTexture(TextureTarget.Texture2D, this._handleV);
        this._gl.PixelStore(PixelStoreParameter.UnpackRowLength, frame.LineSizeArr[2]);
        this._gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Red, (uint)frame.Width / 2,
            (uint)frame.Height / 2, 0,
            PixelFormat.Red, PixelType.UnsignedByte, frame.Data[2]);
        shader.SetUniform("tex_v", 2);

        this._gl.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
    }

    public void Dispose()
    {
        this._gl.DeleteTexture(this._handleY);
        this._gl.DeleteTexture(this._handleU);
        this._gl.DeleteTexture(this._handleV);
    }
}