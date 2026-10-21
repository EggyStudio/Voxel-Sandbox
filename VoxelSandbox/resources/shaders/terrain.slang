// The voxel game's sections and clouds, each vertex its position and eight packed bytes of the
// game's own (MeshStream, read as CUSTOM0), which hold the face it lies on, which of the face's
// corners it is, its shade, its surface's tiles in the atlas of the player's resource packs and the
// color it carries; the normal comes from the table below by face, and the color is the carried one
// under the shade, sRGB as a vertex color is. A face's tile is laid across it by where each point
// lies in its block, as Minecraft lays a texture on a face of a block or of a smaller box within
// one, and tinted by the carried color, a surface no pack gives a texture having the white tile and
// its own color. The model pass's own light falls over that color, as the engine draws a mesh with
// colors at its vertices, taken at the middle of the block's pixel each point lies in, so the sun's
// shadows step along the pixels, then a haze toward the color of the sky at the horizon as the
// distance from the eye grows, as Minecraft's fog hides where the world ends.
import modelpass;

// The haze's color, as the frame shows it, and the distances in blocks it starts at and covers by.
uniform float4 fogColor;
uniform float fogStart;
uniform float fogEnd;

// The atlas of the packs' tiles, each twice its size in a cell of its own holding it two by two,
// and its cells across and down and a tile's pixels across. The engine hands a shader's own texture
// over as the bytes it holds, so a tile's color is decoded from sRGB here.
Sampler2D atlas;
uniform float4 atlasLayout;

struct TerrainOutput
{
    ModelStreamsOutput streams;
    // The face, the surface's tile, its overlay's tile, and 1 where the tile under the overlay is
    // left untinted, as the grass block's side is, or 2 where the overlay is, as a look's decor of
    // flowers on the grass is, the same over the face.
    nointerpolation uint4 surface : TEXCOORD4;
};

// The six faces' outward normals in the order +x, -x, +y, -y, +z, -z, as SectionMesher's Normals
// lays them, then a plant's walls along x and along z, lit as if they faced up, as Minecraft lights
// its plants with no shade by the way they face.
static const float3 Normals[8] = {
    float3(1, 0, 0), float3(-1, 0, 0), float3(0, 1, 0), float3(0, -1, 0), float3(0, 0, 1), float3(0, 0, -1), float3(0, 1, 0), float3(0, 1, 0),
};

// The packed bytes as SectionMesher.Pack writes them. The first uint holds the face at its low 3
// bits, the corner at the next 2, the shade from 0 to 127 at the 7 above, the overlay's untinted
// flag at bit 12, the tile at the 9 above that, the overlay's tile at the next 9 and the base's
// untinted flag at the top bit, and the second the color's red, green and blue a byte each from the
// lowest and its alpha in the highest. The shade alone rides in the second texture coordinate, for
// a tile or an overlay left untinted.
[shader("vertex")]
TerrainOutput vertexMain(float3 position : POSITION, uint2 packed : CUSTOM0, ModelInstance instance)
{
    uint face = packed.x & 7;
    float shade = float((packed.x >> 5) & 127) / 127.0;
    float4 tint = float4(float(packed.y & 255), float((packed.y >> 8) & 255), float((packed.y >> 16) & 255), float(packed.y >> 24)) / 255.0;
    TerrainOutput output;
    // A plant's wall carries its corner's place on its texture in sixteenths where a face carries its
    // overlay tile and decor flag, across in five bits and down in four and the flag, so the texture
    // lies on the wall wherever its offset puts it; it has no overlay and no decor.
    bool wall = face >= 6;
    float2 place = float2(0, 0);
    if (wall) place = float2(float((packed.x >> 22) & 31), float(((packed.x >> 27) & 15) | ((packed.x >> 12) & 1) << 4)) / 16.0;
    output.streams = transformModelVertex(position, Normals[face], place, float4(tint.rgb * shade, tint.a), float2(shade, 0), instance);
    output.surface = wall ? uint4(face, (packed.x >> 13) & 511, 0, packed.x >> 31)
        : uint4(face, (packed.x >> 13) & 511, (packed.x >> 22) & 511, (packed.x >> 31) | ((packed.x >> 12) & 1) << 1);
    return output;
}

// A point's place across a face of each direction in blocks, u to the right and v down seen from
// outside, as Minecraft lays a texture on a face, kept whole rather than within its block so its
// rate across the screen has no step at a block's edge.
float2 across(float3 world, uint face)
{
    switch (face)
    {
    case 0: return float2(-world.z, -world.y);
    case 1: return float2(world.z, -world.y);
    case 2: return float2(world.x, world.z);
    case 3: return float2(world.x, -world.z);
    case 4: return float2(world.x, -world.y);
    case 5: return float2(-world.x, -world.y);
    case 6: return float2(world.x, -world.y);
    default: return float2(world.z, -world.y);
    }
}

// A tile's color at a place across its face, decoded to linear, at a mip level no smaller than the
// one where its cell is a pixel, below which the cell's neighbors would be read into it. A shader
// whose layout was never set, as the clouds' is, reads its one white cell.
float4 tileColor(uint tile, float2 place, float level)
{
    float2 cells = max(atlasLayout.xy, float2(1.0));
    uint columns = uint(cells.x);
    float2 cell = float2(float(tile % columns), float(tile / columns));
    float2 uv = (cell + 0.25 + frac(place) * 0.5) / cells;
    float4 texel = atlas.SampleLevel(uv, level);
    return float4(srgbToLinear(texel.rgb), texel.a);
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
float4 fragmentMain(TerrainOutput input) : SV_Target
{
    ModelStreamsOutput streams = input.streams;
    uint4 surface = input.surface;
    // The mip level from how many of a tile's pixels a pixel of the screen spans, worked out before
    // any fragment is dropped.
    float2 place = surface.x >= 6 ? min(streams.base.uv, float2(0.99999)) : across(streams.base.world, surface.x);
    float pixels = max(atlasLayout.z, 1.0);
    float2 texels = place * pixels;
    float spans = max(length(ddx(texels)), length(ddy(texels)));
    float level = clamp(log2(max(spans, 1e-6)), 0.0, log2(pixels) + 1.0);

    float4 tile = tileColor(surface.y, place, level);
    float3 tinted = tile.rgb * streams.vertexColor.rgb;
    if (surface.w == 1)
    {
        float4 over = tileColor(surface.z, place, level);
        tinted = lerp(tile.rgb * srgbToLinear(float3(streams.uv2.x)), over.rgb * streams.vertexColor.rgb, over.a);
    }
    else if (surface.w == 2)
    {
        float4 over = tileColor(surface.z, place, level);
        tinted = lerp(tinted, over.rgb * srgbToLinear(float3(streams.uv2.x)), over.a);
    }
    float4 color = alphaTested(materialColor(streams.base) * float4(tinted, tile.a * streams.vertexColor.a), streams.base);
    ModelVertexOutput at = streams.base;
    at.world = pixelMiddle(streams.base.world, normalize(streams.base.normal));
    float3 shown = lit(color.rgb, at);
    float haze = smoothstep(fogStart, fogEnd, length(streams.base.toEye));
    return float4(lerp(shown, fogColor.rgb, haze), color.a);
}
