cbuffer PerObject : register(b0)
{
    float4x4 WorldViewProj : register(b0);
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
    float4 Depth : TEXCOORD0;
};

VertexShaderOutput VSMain(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProj);
    output.Color = input.Color;
    output.Depth = output.Position;

    return output;
}

float4 PSMain(VertexShaderOutput input) : SV_Target
{
    float4 output = (float4) input.Depth.z / input.Depth.w;
    output.w = 1.0;
    return output;
}
