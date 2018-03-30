
Texture2D Tex : register(t0);

sampler Sampler : register(s0){
	AddressU = Clamp;
	AddressV = Clamp;
};

matrix WorldViewProjection;
matrix World;

float HeightMultiplier;
float4 lightDirection;
float4 lightColour;
float4 lightAmbient;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 WorldPosition : POSITION1;
};

struct GeometryShaderOutput{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0; 

	float4 heightInfo = Tex.SampleLevel(Sampler, input.UV, 0);
	float4 pos = input.Position;
	pos.z = heightInfo.x * HeightMultiplier;

	output.Position = mul(pos, WorldViewProjection);
	output.WorldPosition = mul(pos, World);

	return output;
}

[maxvertexcount(3)]
void MainGS(triangle VertexShaderOutput input[3], inout TriangleStream<GeometryShaderOutput> output) 
{

	float3 normal = normalize(cross((input[0].WorldPosition - input[1].WorldPosition).xyz, (input[2].WorldPosition - input[0].WorldPosition).xyz));
	for (int i = 0; i < 3; i++) {

		GeometryShaderOutput o = (GeometryShaderOutput)0;
		o.Position = input[i].Position;
		o.Normal = normal;
		output.Append(o);
	}
}

float4 MainPS(GeometryShaderOutput input) : SV_Target
{
	input.Normal = normalize(input.Normal);
	float4 Ia = 0.1f * lightAmbient;
	// diffuse lighting only
	float4 Id = saturate(dot(input.Normal, -lightDirection));

	return Ia + Id * lightColour;
}

technique10 BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile vs_5_0 MainVS();
		GeometryShader = compile gs_5_0 MainGS();
		PixelShader = compile ps_5_0 MainPS();
	}
};