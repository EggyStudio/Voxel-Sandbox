// The bits a broken block throws, each a square of a quarter of the block's picture turned toward
// the eye, lit by the engine's light as the square's face toward the eye takes it, but with the
// shadow and the probes read at the bit's middle, which its body keeps in the open, where a part of
// the square clipped into the wall beside it read the sun's and the sky's light from inside the
// wall.
import modelpass;

[shader("vertex")]
ModelStreamsOutput vertexMain(float3 position : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0, ModelInstance instance,
    [[vk::location(9)]] float4 color : COLOR0, [[vk::location(10)]] float2 uv2 : TEXCOORD1)
{
    ModelStreamsOutput output = transformModelVertex(position, normal, uv, color, uv2, instance);
    // The bit's middle, its square's translation, carried in the vertex color, which a bit leaves white.
    output.vertexColor = float4(instance.worldX.w, instance.worldY.w, instance.worldZ.w, 1.0);
    return output;
}

[shader("fragment")]
float4 fragmentMain(ModelStreamsOutput input) : SV_Target
{
    float4 color = alphaTested(baseColor(input.base), input.base);
    ModelVertexOutput middle = input.base;
    middle.toEye += middle.world - input.vertexColor.xyz;
    middle.world = input.vertexColor.xyz;
    return float4(lit(color.rgb, middle), color.a);
}
