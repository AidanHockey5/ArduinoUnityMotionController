//basic CG shader with a texture and an alpha - Double Sided

Shader "CG Shaders/Unlit/Unlit Texture Alpha DS"
{
	Properties
	{
		_diffuseColor("Shader Color", Color) = (1,1,1,1)
		_diffuseMap("Shader Texture", 2D) = "white" {}
	_alphaPower("Alpha Power", Float) = 1

	}
		SubShader
	{
		Tags{ "Queue" = "Transparent" }
		//unfortunately turning culling off rarely works right. It has a lot of sorting issues
		//Instead we will use 2 passes to composite the full view	

		Pass
	{
		ZWrite Off
		//AlphaTesting
		AlphaTest Greater 0.3
		//Alpha Blending 
		//Blend SrcAlpha OneMinusSrcAlpha 
		//Additive Alpha Blending 
		//Blend SrcAlpha One 

		//Cull front faces first
		Cull Front

		CGPROGRAM
#pragma vertex vShader
#pragma fragment pShader
#include "UnityCG.cginc"

		uniform fixed4 _diffuseColor;
	uniform sampler2D _diffuseMap;
	uniform half4 _diffuseMap_ST;
	uniform half _alphaPower;

	struct app2vert {
		float4 vertex : POSITION;
		fixed2 texCoord : TEXCOORD0;
	};
	struct vert2Pixel
	{
		float4 pos 						: 	SV_POSITION;
		fixed2 uvs : TEXCOORD0;
	};
	vert2Pixel vShader(app2vert IN)
	{
		vert2Pixel OUT;
		OUT.pos = mul(UNITY_MATRIX_MVP, IN.vertex);
		OUT.uvs = IN.texCoord;
		return OUT;
	}
	fixed4 pShader(vert2Pixel IN) : COLOR
	{
		fixed4 outColor;
	half2 diffuseUVs = TRANSFORM_TEX(IN.uvs, _diffuseMap);
	fixed4 texSample = tex2D(_diffuseMap, diffuseUVs);
	texSample.w = pow(texSample.w, _alphaPower);
	outColor = texSample * _diffuseColor;
	return outColor;
	}

		ENDCG
	}

		Pass
	{
		ZWrite Off
		//AlphaTesting
		AlphaTest Greater 0.3
		//Alpha Blending 
		//Blend SrcAlpha OneMinusSrcAlpha 
		//Additive Alpha Blending 
		//Blend SrcAlpha One 

		//Cull back faces so we can write front faces over the last pass
		Cull Back

		CGPROGRAM
#pragma vertex vShader
#pragma fragment pShader
#include "UnityCG.cginc"

		uniform fixed4 _diffuseColor;
	uniform sampler2D _diffuseMap;
	uniform half4 _diffuseMap_ST;
	uniform half _alphaPower;

	struct app2vert {
		float4 vertex : POSITION;
		fixed2 texCoord : TEXCOORD0;
	};
	struct vert2Pixel
	{
		float4 pos 						: 	SV_POSITION;
		fixed2 uvs : TEXCOORD0;
	};
	vert2Pixel vShader(app2vert IN)
	{
		vert2Pixel OUT;
		OUT.pos = mul(UNITY_MATRIX_MVP, IN.vertex);
		OUT.uvs = IN.texCoord;
		return OUT;
	}
	fixed4 pShader(vert2Pixel IN) : COLOR
	{
		fixed4 outColor;
	half2 diffuseUVs = TRANSFORM_TEX(IN.uvs, _diffuseMap);
	fixed4 texSample = tex2D(_diffuseMap, diffuseUVs);
	texSample.w = pow(texSample.w, _alphaPower);
	outColor = texSample * _diffuseColor;
	return outColor;
	}

		ENDCG
	}
	}
}