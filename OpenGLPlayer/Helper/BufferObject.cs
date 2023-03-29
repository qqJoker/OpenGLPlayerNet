﻿using Silk.NET.OpenGL;

namespace OpenGLPlayer.Helper;

public class BufferObject<T> : IDisposable
where T : unmanaged
{
    private uint _handle;
    private BufferTargetARB _bufferType;
    private GL _gl;
    
    public unsafe BufferObject(GL gl, Span<T> data, BufferTargetARB bufferType)
    {
        _gl = gl;
        _bufferType = bufferType;

        _handle = _gl.GenBuffer();
        Bind();
        fixed (void* d = data)
        {
            _gl.BufferData(bufferType, (nuint) (data.Length * sizeof(T)), d, BufferUsageARB.StaticDraw);
        }
    }

    public void Bind()
    {
        _gl.BindBuffer(_bufferType, _handle);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_handle);
    }
}