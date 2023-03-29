#version 330
in vec2 texCoord;
uniform sampler2D tex_y;
uniform sampler2D tex_u;
uniform sampler2D tex_v;
out vec4 FragColor;
precision mediump float;

void main()
{
    vec3 yuv;
    vec3 rgb;
    yuv.x = texture2D(tex_y, texCoord).r;
    yuv.y = texture2D(tex_u, texCoord).r - 0.5;
    yuv.z = texture2D(tex_v, texCoord).r - 0.5;
    rgb = mat3(1.0, 1.0, 1.0,
    0.0, -0.39465, 2.03211,
    1.13983, -0.58060, 0.0) * yuv;
    FragColor = vec4(rgb, 1.0);
}