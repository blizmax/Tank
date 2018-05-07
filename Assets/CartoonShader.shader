// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/CartoonShader" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 0.03)) = .001
	}

		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 100
		Pass
	{
		Name "OUTLINE"
		Tags{ "LightMode" = "Always" }
		Cull Front
		ZWrite On
		ColorMask RGB
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		fixed4 color : COLOR;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
	};

	fixed _Outline;
	fixed4 _OutlineColor;

	v2f vert(appdata v)
	{
		v2f o;

		float4 vPos = float4(UnityObjectToViewPos(v.vertex),1.0f);
		float cameraDis = length(vPos.xyz);
		vPos.xyz += normalize(normalize(vPos.xyz)) * v.color.b;
		float3 vNormal = mul((float3x3)UNITY_MATRIX_IT_MV,v.normal);
		o.pos = mul(UNITY_MATRIX_P,vPos);
		float2 offset = TransformViewToProjection(vNormal).xy;
		offset += offset * cameraDis  * v.color.g;
		o.pos.xy += offset * _Outline* v.color.a;

		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 col = _OutlineColor;
	return col;
	}
		ENDCG
	}
		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 2.0
#pragma multi_compile_fog

#include "UnityCG.cginc"

		struct appdata_t {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		float2 texcoord : TEXCOORD0;
		UNITY_FOG_COORDS(1)
			UNITY_VERTEX_OUTPUT_STEREO
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;

	v2f vert(appdata_t v)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		UNITY_TRANSFER_FOG(o,o.vertex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 col = tex2D(_MainTex, i.texcoord);
	UNITY_APPLY_FOG(i.fogCoord, col);
	UNITY_OPAQUE_ALPHA(col.a);
	return col;
	}
		ENDCG
	}
	}
}
