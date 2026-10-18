// The voxel game's sections and clouds, each vertex its position and eight packed bytes of the
// game's own (MeshStream, read as CUSTOM0), which hold the face it lies on, which of the face's
// corners it is, its shade and its surface's color; the normal and texture coordinate come from
// the tables below by face and corner, and the color is the surface's under the shade, sRGB as a
// vertex color is. The model pass's own light falls over that color, as the engine draws a mesh with
// colors at its vertices, taken at the middle of the block's pixel each point lies in, so the sun's
// shadows step along the pixels, then a haze toward the color of the sky at the horizon as the
// distance from the eye grows, as Minecraft's fog hides where the world ends.
import modelpass;

// The haze's color, as the frame shows it, and the distances in blocks it starts at and covers by.
uniform float4 fogColor;
uniform float fogStart;
uniform float fogEnd;

// The six faces' outward normals in the order +x, -x, +y, -y, +z, -z, and each face's four corners'
// texture coordinates in the order the game's mesher writes them, as SectionMesher's Normals and
// Corners lay them.
static const float3 Normals[6] = {
    float3(1, 0, 0), float3(-1, 0, 0), float3(0, 1, 0), float3(0, -1, 0), float3(0, 0, 1), float3(0, 0, -1),
};
static const float2 Coordinates[24] = {
    float2(1, 0), float2(1, 1), float2(0, 1), float2(0, 0),
    float2(0, 0), float2(1, 0), float2(1, 1), float2(0, 1),
    float2(0, 1), float2(0, 0), float2(1, 0), float2(1, 1),
    float2(1, 1), float2(0, 1), float2(0, 0), float2(1, 0),
    float2(0, 0), float2(1, 0), float2(1, 1), float2(0, 1),
    float2(1, 0), float2(1, 1), float2(0, 1), float2(0, 0),
};

// The packed bytes as SectionMesher.Pack writes them. The first uint holds the face at its low 3
// bits, the corner at the next 2 and the shade from 0 to 255 at the 8 above, and the second the
// color's red, green and blue a byte each from the lowest and its alpha in the highest.
[shader("vertex")]
ModelStreamsOutput vertexMain(float3 position : POSITION, uint2 packed : CUSTOM0, ModelInstance instance)
{
    uint face = packed.x & 7;
    uint corner = (packed.x >> 3) & 3;
    float shade = float((packed.x >> 5) & 255) / 255.0;
    float4 tint = float4(float(packed.y & 255), float((packed.y >> 8) & 255), float((packed.y >> 16) & 255), float(packed.y >> 24)) / 255.0;
    return transformModelVertex(position, Normals[face], Coordinates[face * 4 + corner], float4(tint.rgb * shade, tint.a), float2(0, 0), instance);
}

// How many pixels a block's face is across, as Minecraft's textures have sixteen, the grid each
// point's light is taken on.
static const float Pixels = 16.0;

// A point of a face moved to the middle of the block's pixel it lies in, along the face alone, so
// every point of a pixel takes the same light and a shadow's edge steps along the pixels.
float3 pixelMiddle(float3 world, float3 normal)
{
    float3 across = step(abs(normal), float3(0.5));
    return lerp(world, (floor(world * Pixels) + 0.5) / Pixels, across);
}

[shader("fragment")]
float4 fragmentMain(ModelStreamsOutput input) : SV_Target
{
    float4 color = alphaTested(baseColor(input), input.base);
    ModelVertexOutput at = input.base;
    at.world = pixelMiddle(input.base.world, normalize(input.base.normal));
    float3 shown = lit(color.rgb, at);
    float haze = smoothstep(fogStart, fogEnd, length(input.base.toEye));
    return float4(lerp(shown, fogColor.rgb, haze), color.a);
}
