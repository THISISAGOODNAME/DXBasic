cbuffer PreObject : register(b0)
{
    float4x4 WorldViewProj;
};

struct VertexShaderInput
{
    float4 Position : SV_Position;
    float4 Color : COLOR;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float4 Color : COLOR;
};

VertexShaderOutput VSMain(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, WorldViewProj);
    output.Color = input.Color;

    return output;
}

float4 PSMain(VertexShaderOutput input) : SV_Target
{
    return input.Color;
}