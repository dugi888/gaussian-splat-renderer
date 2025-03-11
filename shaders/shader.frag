#version 450 core

in vec4 fragColor;
out vec4 outputColor;

void main()
{
    vec3 RGBd = vec3(1.0, 1.0, 1.0); // Initial destination color
    float Ad = 1.0; // Initial destination alpha

    vec3 RGBs = fragColor.rgb * fragColor.a; // Source color with premultiplied alpha
    float As = fragColor.a; // Source alpha

    // Back-to-front compositing (formula from pdf file given)
    RGBd = (1.0 - As) * RGBd + As * RGBs;

    outputColor = vec4(RGBd, Ad);
}