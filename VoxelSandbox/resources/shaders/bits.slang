// The bits a broken block throws, each a square of a quarter of the block's picture turned toward
// the eye, lit by the engine's light as the square's face toward the eye takes it, but with the
// shadow and the probes read at the bit's middle, which its body keeps in the open, where a part of
// the square clipped into the wall beside it read the sun's and the sky's light from inside the
// wall, and no less than the terrain's floor of the sky's light at the sky level of the bit's cell,
// which its material's occlusion strength carries.
import modelpass;
// The sky's light from all around, which modelpass keeps to itself.
import lights;

[shader("vertex")]
ModelStreamsOutput vertexMain(float3 position : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0, ModelInstance instance,
    [[vk::location(9)]] float4 color : COLOR0, [[vk::location(10)]] float2 uv2 : TEXCOORD1)
{
    ModelStreamsOutput output = transformModelVertex(position, normal, uv, color, uv2, instance);
    // The bit's middle, its square's translation, carried in the vertex color, which a bit leaves white.
    output.vertexColor = float4(instance.worldX.w, instance.worldY.w, instance.worldZ.w, 1.0);
    return output;
}

// The source's brightness of a sky light level by day, as terrain.slang's skyBrightness gives it.
float skyBrightness(float level)
{
    float f = level / 15.0;
    float curve = f / (4.0 - 3.0 * f);
    float left = 1.0 - curve;
    return lerp(curve, 1.0 - left * left * left * left, 0.5);
}

// The least light a bit takes, as terrain.slang's skyFloor gives a face.
float3 skyFloor(float3 albedo, ModelVertexOutput input, float level)
{
    if (lighting.environment.z <= 0.0 || level <= 0.0 || level >= 15.0)
        return float3(0.0);
    float3 sky = environmentIrradiance(surfaceNormal(input)) * lighting.environment.x * lighting.environmentLight.rgb * screenOcclusion(input);
    return albedo * sky * srgbToLinear(float3(skyBrightness(level))) * saturate(15.0 - level);
}

[shader("fragment")]
float4 fragmentMain(ModelStreamsOutput input) : SV_Target
{
    float4 color = alphaTested(baseColor(input.base), input.base);
    ModelVertexOutput middle = input.base;
    middle.toEye += middle.world - input.vertexColor.xyz;
    middle.world = input.vertexColor.xyz;
    if (drawnUnlit(middle))
        return float4(lit(color.rgb, middle), color.a);
    LitLight light = litLight(color.rgb, middle);
    float3 shown = light.diffuse + light.specular + emitted(middle);
    float sky = materialOcclusionStrength(input.base) * 15.0;
    return float4(toDisplay(max(shown, skyFloor(color.rgb, middle, sky))), color.a);
}
