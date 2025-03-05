#version 450 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec4 color;

out vec4 fragColor;

uniform mat4 viewProjection;
uniform float scalingParameter; 

void main()
{
    // Transform the position to clip space
    vec4 clipPos = viewProjection * vec4(position, 1.0);

    // depth in view space
    float z = clipPos.w;

    
    float scale = scalingParameter / z;
    gl_PointSize = 2.0 * scale; // 2s/z
    
    fragColor = color;

    // final position
    gl_Position = clipPos;
}