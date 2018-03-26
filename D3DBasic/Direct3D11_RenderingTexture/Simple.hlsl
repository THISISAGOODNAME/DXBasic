Texture2D ShaderTexture : register(t0);
SamplerState Sampler : register(s0);

cbuffer PerObject : register(b0)
{
    float4x4 WorldViewProj;
};

struct VertexShaderInput
{
    float4 Position : SV_Position;
    float2 TextureUV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float2 TextureUV : TEXCOORD0;
};

VertexShaderOutput VSMain(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProj);
    output.TextureUV = input.TextureUV;

    return output;
}

float4 PSMain(VertexShaderOutput input) : SV_Target
{
    return ShaderTexture.Sample(Sampler, input.TextureUV);
}