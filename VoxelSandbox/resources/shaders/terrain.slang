// The voxel game's sections and clouds, each vertex its position and eight packed bytes of the
// game's own (MeshStream, read as CUSTOM0), which hold the face it lies on, which of the face's
// corners it is, its shade, its surface's tiles in the atlas of the player's resource packs and the
// color it carries; the normal comes from the table below by face, and the color is the carried one
// under the shade, sRGB as a vertex color is. A face's tile is laid across it by where each point
// lies in its block, as Minecraft lays a texture on a face of a block or of a smaller box within
// one, and tinted by the carried color, a surface no pack gives a texture having the white tile and
// its own color. The model pass's own light falls over that color, as the engine draws a mesh with
// colors at its vertices, the sun's shadow taken at the middle of the step of a grid each point lies
// in, so the sun's shadows step along it, and no less than a floor of the sky's light by the point's
// sky level, then a haze toward the color of the sky behind the point as the distance from the eye
// grows, as Minecraft's fog hides where the world ends.
import modelpass;
// The sun's light and its shadow, which modelpass keeps to itself.
import lights;

// The haze's color, as the frame shows it, and the distances in blocks it starts at and covers by.
uniform float4 fogColor;
uniform float fogStart;
uniform float fogEnd;
// The atmosphere's haze, the most of it and how fast it grows with distance, none under water, 1
// where the haze takes the sky's color behind each point and 0 where it is the haze's color alone,
// and the distance past which a point is the haze dome's.
uniform float4 atmosphere;
// The haze's thinning with height, the height it is whole at and under, the blocks over it it thins
// by e in, and 1 where it thins so and 0 where it is as thick at every height, as Haze reckons it.
uniform float4 hazeHeight;
// One more than the atlas's tile of a fire's flames, 0 for none, so a copy of the shader that sets
// nothing, as the clouds' does, draws no face as flames, and how many times their color they are
// drawn at whatever light reaches them, as the source draws fire at its full light, where the light
// a wall of them takes from the fire's own light inside would leave the walls facing out dark; and
// the same two of lava, which the source draws at its full light too.
uniform float4 flames;

// How much of the haze the dome shows at a height above the horizon in degrees, between the rings of
// HazeDome.Shown as its faces blend it.
float domeShown(float degrees)
{
    if (degrees >= 6.0) return 0.0;
    if (degrees >= 3.0) return lerp(0.35, 0.0, (degrees - 3.0) / 3.0);
    if (degrees >= 1.5) return lerp(0.75, 0.35, (degrees - 1.5) / 1.5);
    if (degrees >= 0.0) return lerp(1.0, 0.75, degrees / 1.5);
    return 1.0;
}

// The mean of the haze's density along the ray from the eye to a point, whole at and under its
// height and falling by e over each of its blocks above, as Haze.HeightShare reckons it.
float heightShare(float eyeY, float pointY)
{
    if (hazeHeight.z == 0.0) return 1.0;
    float h0 = max(eyeY - hazeHeight.x, 0.0) / hazeHeight.y;
    float h1 = max(pointY - hazeHeight.x, 0.0) / hazeHeight.y;
    float across = h1 - h0;
    return abs(across) < 1e-3 ? exp(-h0) : (exp(-h0) - exp(-h1)) / across;
}

// The color a point fades into, the sky drawn behind it as the engine's sky shader draws it along the
// ray from the eye, under the share of the haze dome's band at the ray's height, so a block high
// against the sky fades into the sky's blue and one at the horizon into the horizon's haze, as the
// modpack's shader fades its land. Under water, and on the dome itself, whose band is blended over
// the sky, it is the haze's color alone.
float3 hazeColor(float3 toEye, float distance)
{
    if (atmosphere.z == 0.0 || distance > atmosphere.w) return fogColor.rgb;
    float3 way = -toEye / distance;
    float3 sky = toDisplay(skyMap.SampleLevel(lightLinear, way, 0).rgb * lighting.environment.x);
    return lerp(sky, fogColor.rgb, domeShown(degrees(asin(clamp(way.y, -1.0, 1.0)))));
}

// The atlas of the packs' tiles, each twice its size in a cell of its own holding it two by two,
// its cells across and down, a tile's pixels across and how many of the table below hold an
// animated texture. The engine hands a shader's own texture over as the bytes it holds, so a tile's
// color is decoded from sRGB here.
Sampler2D atlas;
uniform float4 atlasLayout;

// The atlas's animated textures, each its first tile, which a face carries, how many tiles its
// steps take one after another, and the game ticks each is held, and the game ticks now, a
// twentieth of a second each, as Minecraft steps its textures.
uniform float4 animations[16];
uniform float ticks;

// A tile stepped to the frame its animation shows now, as TerrainAtlas.Step steps it.
uint stepped(uint tile)
{
    int count = int(atlasLayout.w);
    for (int i = 0; i < count; i++)
    {
        float4 animation = animations[i];
        if (float(tile) == animation.x)
            return tile + uint(fmod(floor(ticks / animation.z), animation.y));
    }
    return tile;
}

struct TerrainOutput
{
    ModelStreamsOutput streams;
    // The face, the surface's tile, its overlay's tile, and 1 where the tile under the overlay is
    // left untinted, as the grass block's side is, 2 where the overlay is, as a look's decor of
    // flowers on the grass is, 3 where the face's corners carry their places on the texture, the
    // same over the face, or 4 where the face is of glass and the overlay's bits say which of its
    // neighbors are the same glass, as SectionMesher.Joined packs them.
    nointerpolation uint4 surface : TEXCOORD4;
    // The corner of the block a face bounds, the same at its four corners, so a point of glass at the
    // seam between two panes, whose place may fall a hair into its neighbor's block, reads its own
    // pane's joins.
    nointerpolation float3 block : TEXCOORD5;
    // The water's depth under a point of its top in blocks, from its corners' as SectionMesher
    // carries them in the overlay's bits of water's own faces, or less than zero on every other face.
    float waterDepth : TEXCOORD6;
};

// The corners of each of the six faces of a block, in the order SectionMesher's Corners lays them,
// so a corner's place less its own gives the block's.
static const float3 FaceCorners[24] = {
    float3(1, 0, 0), float3(1, 1, 0), float3(1, 1, 1), float3(1, 0, 1),
    float3(0, 0, 0), float3(0, 0, 1), float3(0, 1, 1), float3(0, 1, 0),
    float3(0, 1, 0), float3(0, 1, 1), float3(1, 1, 1), float3(1, 1, 0),
    float3(0, 0, 0), float3(1, 0, 0), float3(1, 0, 1), float3(0, 0, 1),
    float3(0, 0, 1), float3(1, 0, 1), float3(1, 1, 1), float3(0, 1, 1),
    float3(0, 0, 0), float3(0, 1, 0), float3(1, 1, 0), float3(1, 0, 0),
};

// The alpha of the material water's own faces are drawn with, which tells them from the rest.
static const float WaterMark = 253.0 / 255.0;

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
// lowest and its alpha in the highest, an alpha from 240 up being 1 with the corner's sky level in
// its low four bits. The shade rides in the second texture coordinate, for a tile or an overlay left
// untinted, with the sky level beside it.
//
// A face with no overlay and neither flag may carry its corner's place on the texture in sixteenths
// in the overlay's bits, one more than across and seventeen times down, as a plant's wall and the
// pieces of a lantern or a torch do, so the texture lies on the face by its corners' places
// wherever the face stands, the place riding in the first texture coordinate.
[shader("vertex")]
TerrainOutput vertexMain(float3 position : POSITION, uint2 packed : CUSTOM0, ModelInstance instance)
{
    uint face = packed.x & 7;
    float shade = float((packed.x >> 5) & 127) / 127.0;
    uint top = packed.y >> 24;
    float4 tint = float4(float(packed.y & 255), float((packed.y >> 8) & 255), float((packed.y >> 16) & 255), top >= 240 ? 255.0 : float(top)) / 255.0;
    float sky = top >= 240 ? float(top & 15) : 0.0;
    TerrainOutput output;
    uint over = (packed.x >> 22) & 511;
    uint kind = (packed.x >> 31) | ((packed.x >> 12) & 1) << 1;
    // Water's own faces, drawn with a material of an alpha of 253, carry its depth in the overlay's
    // bits, which hold no overlay and no place there.
    bool water = abs(instance.color.a - WaterMark) < 0.5 / 255.0;
    output.waterDepth = water && face == 2 ? float(over) / 16.0 : -1.0;
    if (water)
        over = 0;
    bool placed = kind == 0 && over > 0;
    float2 place = placed ? float2(float((over - 1) % 17), float((over - 1) / 17)) / 16.0 : float2(0, 0);
    output.streams = transformModelVertex(position, Normals[face], place, float4(tint.rgb * shade, tint.a), float2(shade, sky), instance);
    output.surface = uint4(face, (packed.x >> 13) & 511, placed ? 0 : over, placed ? 3 : kind == 3 ? 4 : kind);
    output.block = output.streams.base.world - FaceCorners[min(face, 5u) * 4 + ((packed.x >> 3) & 3)];
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
    tile = stepped(tile);
    float2 cells = max(atlasLayout.xy, float2(1.0));
    uint columns = uint(cells.x);
    float2 cell = float2(float(tile % columns), float(tile / columns));
    float2 uv = (cell + 0.25 + frac(place) * 0.5) / cells;
    float4 texel = atlas.SampleLevel(uv, level);
    return float4(srgbToLinear(texel.rgb), texel.a);
}

// How many steps a block's face is across in the grid each point's shadow from the sun is taken on,
// eight, as the modpack's shader steps its shadows with the settings it is played with, half a
// texture's sixteen pixels.
static const float LightSteps = 8.0;

// A point of a face moved to the middle of the step it lies in, along the face alone, so every
// point of a step takes the same shadow and a shadow's edge steps along the grid.
float3 pixelMiddle(float3 world, float3 normal)
{
    float3 across = step(abs(normal), float3(0.5));
    return lerp(world, (floor(world * LightSteps) + 0.5) / LightSteps, across);
}

// The source's brightness of a sky light level by day at its default brightness setting, 0 at level
// 0 and 1 at 15, its curve, the level's share f over 4 less three times f, taken halfway to one less
// the fourth power of what the curve leaves, as the setting takes it, with the grey the source mixes
// into every level left out, so a face the sky never reaches takes none. Level 13 reads 0.80 of 15 on
// the screen.
float skyBrightness(float level)
{
    float f = level / 15.0;
    float curve = f / (4.0 - 3.0 * f);
    float left = 1.0 - curve;
    return lerp(curve, 1.0 - left * left * left * left, 0.5);
}

// The least light a face takes, the sky's light from all around as the engine lights an open face by
// it, the environment's at its strength and its light's share and tint, under the face's sky level on the source's brightness curve
// in linear light. The scene's field holds leaves solid, so without the floor a canopy's shade at
// sky level 13 takes next to none of the sky's light, where the source lights it at 13. A face at sky
// level 15 has nothing over it, and the field sees the sky above it itself, so the floor fades out
// from 14 to 15, which keeps the field's shade beside a wall on open ground, where the whole sky's
// light lifted it by 12 levels. Sunlit ground takes more than the floor from the engine and keeps its
// own, and at night the sky's light is next to nothing.
float3 skyFloor(float3 albedo, ModelVertexOutput input, float level)
{
    if (lighting.environment.z <= 0.0 || level <= 0.0 || level >= 15.0)
        return float3(0.0);
    float3 sky = environmentIrradiance(surfaceNormal(input)) * lighting.environment.x * lighting.environmentLight.rgb * screenOcclusion(input);
    return albedo * sky * srgbToLinear(float3(skyBrightness(level))) * saturate(15.0 - level);
}

// A color lit at a point as the engine's lit lights it, all its light taken at the point itself but
// the sun's shadow, taken at the middle of the point's step (pixelMiddle), so the sun's shadows step
// along the grid and a lamp's light and the light from all around fall smoothly, where taking all of
// it at the step's middle laid a lantern's pool on the floor in squares. The sun's share of the
// light, which litLight cuts by its shadow at the point, is cut by the step's instead, the light
// reaching a flat face the same at both and the contact shadow, read from the screen's pixel, the
// same for both. The terrain's material has no metallic-roughness map, so its factors are its own.
// The light is no less than the sky's floor at the point's sky level (skyFloor).
float3 litStepped(float3 albedo, ModelVertexOutput input, float sky)
{
    if (drawnUnlit(input))
        return lit(albedo, input);
    LitLight light = litLight(albedo, input);
    float3 shown = light.diffuse + light.specular + emitted(input);
    int index = lighting.shadowLight;
    if (index >= 0 && index < min(lighting.count, 16))
    {
        float3 geometric = normalize(input.normal);
        float held;
        float here = lightReaching(input.world, geometric, held);
        // Past the shadow map both read 1, and the sun's share is the same.
        if (held > 0.0)
        {
            float there = lightReaching(pixelMiddle(input.world, geometric), geometric);
            Light sun = lighting.lights[index];
            float3 n = surfaceNormal(input);
            float metallic = saturate(materialMetallic(input));
            float roughness = clamp(materialRoughness(input), 0.045, 1.0);
            float3 fresnel;
            float3 specular = specularReflected(n, normalize(input.toEye), -fromLight(sun, input.world), lerp(float3(0.04), albedo, metallic), roughness * roughness, fresnel);
            float3 reflected = albedo * (1.0 - metallic) * (1.0 - fresnel) + specular;
            shown += arriving(sun, input.world, n) * reflected * sunContact(input, held) * (there - here);
        }
    }
    return toDisplay(max(shown, skyFloor(albedo, input, sky)));
}

// Whether a sample of a picture differs from another, as a point of glass's frame does from its
// pane.
bool differs(float4 a, float4 b)
{
    float4 d = abs(a - b);
    return max(max(d.r, d.g), max(d.b, d.a)) > 0.02;
}

// Glass's place on its picture with its frame left out toward its neighbors of the same glass, the
// joins' bits saying which. A point lies in the frame along the right or left edge where the
// picture across its row's place at the middle height differs from the middle, and along the lower
// or upper edge likewise down its column, the picture's pane being the same everywhere inside its
// frame. A point in the frame toward a joined neighbor takes the picture's middle across that edge,
// so it shows the pane, or in a corner the frame's other edge running on straight past the join, and
// a corner whose two neighbors are joined and the one across it not keeps its corner, so the frame
// turns inside an L of three panes.
float2 joined(float2 place, uint tile, uint joins, float3 block, uint face, float level)
{
    float2 cell = floor(across(block + 0.5, face));
    float2 at = clamp(place - cell, float2(0.0), float2(0.99999));
    float4 middle = tileColor(tile, cell + 0.5, level);
    bool acrossRow = differs(tileColor(tile, cell + float2(at.x, 0.5), level), middle);
    bool downColumn = differs(tileColor(tile, cell + float2(0.5, at.y), level), middle);
    bool joinedAcross = (joins & (at.x >= 0.5 ? 1u : 2u)) != 0;
    bool joinedDown = (joins & (at.y >= 0.5 ? 4u : 8u)) != 0;
    uint corner = at.x >= 0.5 ? (at.y >= 0.5 ? 16u : 64u) : (at.y >= 0.5 ? 32u : 128u);
    float2 shown = at;
    if (acrossRow && joinedAcross)
        shown.x = 0.5;
    if (downColumn && joinedDown)
        shown.y = 0.5;
    if (acrossRow && downColumn && joinedAcross && joinedDown && (joins & corner) == 0)
        shown = at;
    return cell + shown;
}

[shader("fragment")]
float4 fragmentMain(TerrainOutput input) : SV_Target
{
    ModelStreamsOutput streams = input.streams;
    uint4 surface = input.surface;
    // The mip level from how many of a tile's pixels a pixel of the screen spans, worked out before
    // any fragment is dropped.
    // A carried place is kept within its tile at both ends, since a place a hair under 0 at a face's
    // edge would wrap to the tile's far side, as the bottom row of a plant's picture onto its top.
    float2 place = surface.w == 3 ? clamp(streams.base.uv, float2(0.0), float2(0.99999)) : across(streams.base.world, surface.x);
    float pixels = max(atlasLayout.z, 1.0);
    float2 texels = place * pixels;
    float spans = max(length(ddx(texels)), length(ddy(texels)));
    float level = clamp(log2(max(spans, 1e-6)), 0.0, log2(pixels) + 1.0);
    // A carried place, a plant's or a flame's picture, which does not tile, is kept half a texel of
    // the coarser level sampled inside its tile, since a texel at its edge reaches into the copy of
    // the tile beside it in its cell, its far edge's rows, which drew a line along a fire's top.
    if (surface.w == 3)
        place = clamp(place, float2(0.5 * exp2(ceil(level)) / pixels), float2(1.0 - 0.5 * exp2(ceil(level)) / pixels));

    if (surface.w == 4)
        place = joined(place, surface.y, surface.z, input.block, surface.x, level);
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
    // A plant's wall is lit as if it faced up and seen from straight above, since the engine lights a
    // face turned from the eye as its back, turned over, and a wall above the eye's height facing up
    // would take the light of one facing down, nearly black at noon.
    ModelVertexOutput facing = streams.base;
    if (surface.x >= 6)
        facing.toEye = float3(0.0, length(streams.base.toEye), 0.0);
    float3 shown = int(surface.y) + 1 == int(flames.x) ? color.rgb * flames.y
        : int(surface.y) + 1 == int(flames.z) ? color.rgb * flames.w
        : litStepped(color.rgb, facing, streams.uv2.y);
    // Water's top shows its bed through it darkened and hidden by the way the view takes through the
    // water, longer the lower the eye looks across it, clears toward its shore so it meets the bank
    // with no hard line, and shows the sky along the view's mirror by Schlick's fresnel, more of it
    // the lower the eye looks, as calm water shows the sky.
    if (input.waterDepth >= 0.0)
    {
        float3 v = normalize(streams.base.toEye);
        float through = input.waterDepth / max(abs(v.y), 0.35);
        float clear = exp(-0.1 * through);
        shown *= lerp(0.6, 1.0, clear);
        float alpha = lerp(1.0, min(color.a, 0.4) * lerp(0.65, 1.0, smoothstep(0.0, 1.0, input.waterDepth)), clear);
        float fresnel = 0.02 + 0.98 * pow(1.0 - saturate(abs(v.y)), 5.0);
        // The sky as the frame draws it along the view's mirror, its haze over the horizon too, by as
        // much of the sky as reaches the water, its sky level where the corner carries one and its
        // light otherwise, so a pool under the ground mirrors none of it.
        float open = streams.uv2.y > 0.0 ? streams.uv2.y / 15.0 : streams.uv2.x;
        float3 sky = hazeColor(-reflect(-v, float3(0.0, 1.0, 0.0)), 1.0) * open;
        float covered = fresnel + (1.0 - fresnel) * alpha;
        shown = (fresnel * sky + (1.0 - fresnel) * alpha * shown) / max(covered, 1e-4);
        color.a = covered;
    }
    // The land's edge fades into the haze, and the atmosphere hazes it from near on as the modpack's
    // shader's does, its share growing as one less e to the minus the cube of the distance, by the
    // haze's density along the ray, thinner over the land high above the sea.
    float distance = length(streams.base.toEye);
    float density = heightShare(streams.base.world.y + streams.base.toEye.y, streams.base.world.y);
    float aerial = atmosphere.x * (1.0 - exp(-distance * distance * distance * 1e-6 * atmosphere.y * density));
    float haze = 1.0 - (1.0 - smoothstep(fogStart, fogEnd, distance)) * (1.0 - aerial);
    return float4(lerp(shown, hazeColor(streams.base.toEye, distance), haze), color.a);
}

// A masked face's shadow, cut where its texture is clear as fragmentMain cuts the face, so a blade of
// grass casts the blade's shadow rather than its wall's. Only a face whose corners carry their places
// on the texture is cut, a plant's wall or a piece showing a part of its texture, and a face whose
// texture is laid by where it lies, a pack's leaves or a door's, casts the whole face's shadow.
[shader("fragment")]
void shadowMain(TerrainOutput input)
{
    uint4 surface = input.surface;
    if (surface.w != 3)
        return;
    float2 place = clamp(input.streams.base.uv, float2(0.0), float2(0.99999));
    float4 tile = tileColor(surface.y, place, 0.0);
    alphaTested(float4(1.0, 1.0, 1.0, tile.a * input.streams.vertexColor.a), input.streams.base);
}
