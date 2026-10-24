// The bits a broken block throws, each a square of a quarter of the block's picture turned toward
// the eye, lit as a block's top is lit at the bit's place, so a bit reads the same whichever way it
// is turned and wherever the sun stands, as Minecraft lights its bits by the light where they are.
// The face turned up is taken a sixteenth of a block out along the square's own normal and seen
// from straight above, since the engine lights a face turned from the eye as its back, turned
// over, and above the eye's height a face turned up would take the light of one turned down.
import modelpass;

[shader("vertex")]
ModelStreamsOutput vertexMain(float3 position : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0, ModelInstance instance,
    [[vk::location(9)]] float4 color : COLOR0, [[vk::location(10)]] float2 uv2 : TEXCOORD1)
{
    return transformModelVertex(position, normal, uv, color, uv2, instance);
}

[shader("fragment")]
float4 fragmentMain(ModelStreamsOutput input) : SV_Target
{
    float4 color = alphaTested(baseColor(input), input.base);
    if (drawnUnlit(input.base))
        return float4(lit(color.rgb, input.base), color.a);
    ModelVertexOutput up = input.base;
    up.normal = float3(0.0, 1.0, 0.0);
    up.world = input.base.world + normalize(input.base.normal) * 0.0625;
    up.toEye = float3(0.0, 1.0, 0.0);
    LitLight light = litLight(color.rgb, up);
    return float4(toDisplay(light.diffuse + emitted(input.base)), color.a);
}
