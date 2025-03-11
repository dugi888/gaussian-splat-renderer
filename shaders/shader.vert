#version 450 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec4 aColor;

uniform mat4 viewProjection;
uniform float scalingParameter;

out vec4 fragColor;
out vec2 texCoord;

void main()
{
    gl_Position = viewProjection * vec4(aPosition, 1.0);
    gl_PointSize = scalingParameter * 20.0 / gl_Position.w;
    fragColor = aColor;

    // Calculate texture coordinates for point sprite
    texCoord = vec2(-1.0, -1.0);
}