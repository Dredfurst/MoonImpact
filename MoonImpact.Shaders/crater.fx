matrix WorldViewProjection;
matrix World;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = mul(input.Position, WorldViewProjection);
	output.UV = input.UV;
	return output;
}

float4 MainPS(in VertexShaderOutput input ) : SV_TARGET
{    
    float2 len = float2(0.5f, 0.5f) - input.Position;
    float x = dot(len, len);
    float y = 5.0 * x + 0.4;
    
    // Output to screen
    return float4(y,y,y, 0);
}


technique10 BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile vs_5_0 MainVS();
		PixelShader = compile ps_5_0 MainPS();
	}
};