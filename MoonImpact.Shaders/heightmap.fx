
Texture2D Tex : register(t0);

sampler Sampler : register(s0){
	AddressU = Clamp;
	AddressV = Clamp;
	Filter = MIN_MAG_MIP_LINEAR;
	MinLOD = 0;
	MaxLOD = 0;
};

matrix WorldViewProjection;
matrix World;

float HeightMultiplier;
float4 lightDirection;
float4 lightColour;
float4 lightAmbient;

float3 Resolution;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL;
};

float Height(float2 uv)
{
	return Tex.SampleLevel(Sampler, uv, 0).x * -HeightMultiplier;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0; 
	
	float4 pos = input.Position;
	pos.z = Height(input.UV);

	float left = Height(input.UV - Resolution.xz) - Height(input.UV + Resolution.xz);
	float up = Height(input.UV - Resolution.zy) - Height(input.UV + Resolution.zy);

	output.Position = mul(pos, WorldViewProjection);
	output.Normal.x = left;
	output.Normal.y = up;
	output.Normal.z = 2;
	output.Normal = normalize(output.Normal);

	return output;
}

float4 MainPS(VertexShaderOutput input) : SV_Target
{
	input.Normal = normalize(input.Normal);
	//float4 Ia = 0.1f * lightAmbient;
	// diffuse lighting only
	float4 Id = saturate(dot(input.Normal, lightDirection));
	
	return Id * lightColour;
	return float4(input.Normal, 1);
}

technique10 BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile vs_5_0 MainVS();
		//GeometryShader = compile gs_5_0 MainGS();
		PixelShader = compile ps_5_0 MainPS();
	}
};