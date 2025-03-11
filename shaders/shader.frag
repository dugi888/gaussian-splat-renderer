#version 450 core

in vec4 fragColor;
in vec2 texCoord;
out vec4 outputColor;

void main()
{
    // Calculate normalized distance from center of point sprite
    vec2 coord = 2.0 * gl_PointCoord - 1.0;
    float r2 = dot(coord, coord);

    // Discard fragments outside the circle
    if (r2 > 1.0) discard;

    // Gaussian function
    float sigma = 10.0; // Controls the spread of the Gaussian
    float gaussian = exp(-r2 / (2.0 * sigma * sigma));

    // Apply Gaussian falloff directly to color with alpha
    outputColor = vec4(fragColor.rgb, fragColor.a * gaussian);
}