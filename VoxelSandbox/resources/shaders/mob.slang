// The voxel game's mobs, each vertex its position, normal and color as BoxMesh writes them, lit by
// the model pass's light as the engine's model_streams.slang lights a mesh with colors, but for the
// light from all around, whose color is taken out, so a mob in shade is not tinted by the blue of
// the sky its rays meet, and for a floor, each face taking at least the share of the light its top
// takes that Minecraft's flat shade gives a face turned its way, 1 on top, 0.8 facing north or
// south, 0.6 facing east or west and 0.5 underneath, as a mob's faces read in Minecraft's light.
import modelpass;
// The sun's light and its shadow, which modelpass keeps to itself.
import lights;

// The weights of red, green and blue in a linear color's luminance.
static const float3 Luminance = float3(0.2126, 0.7152, 0.0722);

[shader("vertex")]
ModelStreamsOutput vertexMain(float3 position : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0, ModelInstance instance,
    [[vk::location(9)]] float4 color : COLOR0, [[vk::location(10)]] float2 uv2 : TEXCOORD1)
{
    return transformModelVertex(position, normal, uv, color, uv2, instance);
}

// The sun's light a white face takes at a point, cut by the sun's shadow, as litLight takes it.
float3 sunlight(ModelVertexOutput input, float3 n)
{
    if (lighting.shadowLight < 0 || lighting.shadowLight >= min(lighting.count, 16))
        return float3(0.0);
    float held;
    float3 arrived = arriving(lighting.lights[lighting.shadowLight], input.world, n);
    arrived *= lightReaching(input.world, normalize(input.normal), held);
    return arrived * sunContact(input, held);
}

// Minecraft's flat shade of a face by the way it turns, between its sides' 0.8 and 0.6 by how far
// it faces along z or x, and toward its top's 1 or its underside's 0.5 as it faces up or down.
float flatShade(float3 n)
{
    float side = lerp(0.6, 0.8, abs(n.z) / max(abs(n.x) + abs(n.z), 1e-4));
    return n.y >= 0.0 ? lerp(side, 1.0, n.y) : lerp(side, 0.5, -n.y);
}

[shader("fragment")]
float4 fragmentMain(ModelStreamsOutput input) : SV_Target
{
    float4 color = alphaTested(baseColor(input), input.base);
    if (drawnUnlit(input.base))
        return float4(lit(color.rgb, input.base), color.a);
    float3 n = normalize(input.base.normal);

    // The light a white face takes here, which a face of the mob's color takes times its color,
    // the sun's share kept and the rest's color taken out.
    LitLight light = litLight(float3(1.0), input.base);
    float3 sun = sunlight(input.base, n);
    float3 diffuse = sun + dot(max(light.diffuse - sun, float3(0.0)), Luminance);

    // The light a face turned up takes a sixteenth of a block out from this face along its own
    // normal, of which this face takes at least its flat shade's share, the shade to the power 2.2
    // in this linear light, since Minecraft multiplies it into the encoded color. Taken on the face
    // itself, the sun's shadow would be looked up with the shadow map's offset running up the face
    // rather than out of it, and a side face would shadow itself in bands. It is seen from straight
    // above, since the engine lights a face turned from the eye as its back, turned over, and above
    // the eye's height the face turned up would take the light of one turned down.
    ModelVertexOutput up = input.base;
    up.normal = float3(0.0, 1.0, 0.0);
    up.world = input.base.world + n * 0.0625;
    up.toEye = float3(0.0, 1.0, 0.0);
    float top = dot(litLight(float3(1.0), up).diffuse, Luminance);
    diffuse = max(diffuse, float3(pow(flatShade(n), 2.2) * top));

    return float4(toDisplay(color.rgb * diffuse + light.specular + emitted(input.base)), color.a);
}
