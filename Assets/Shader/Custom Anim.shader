Shader "Custom/LvDaiAnim"
{
	Properties
	{
		_Color("Color",Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_SSSTex("SSS (RGB)", 2D) = "white" {}
		_ILMTex("ILM (RGB)", 2D) = "white" {}
		_NormalMap("NormalMap",2D) = "Bump"{}//切线空间
		_BumpScale("Scale", Range(0,10)) = 1.0
		_Shininess("Shininess", Range(0.001, 2)) = 0.078125
		_SpecStep("_SpecStep",Range(0.1,0.3)) = 0.5
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline Width", Range(.0,0.01)) = 0.01
		_ShadowContrast("Shadow Contrast", Range(-2,1)) = 1
		_DarkenInnerLine("Darken Inner Line", Range(0, 1)) = 0.2

		_WorldLightDir("_WorldLightDir",Vector) = (1,1,1,1)
	}
		/*
		•R：判断阴影的阈值对应的Offfset。1是标准，越倾向变成影子的部分也会越暗(接近0)，0的话一定是影子。
		•G：对应到Camera的距离，轮廓线的在哪个范围膨胀的系数
		•B：轮廓线的Z Offset 值
		•A：轮廓线的粗细系数。0.5是标准，1是最粗，0的话就没有轮廓线。
		*/
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }

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


		Pass
	{

		Tags{ "LightMode" = "ForwardBase" }
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;   // 告诉Unity把模型空间下的顶点坐标填充给vertex属性
		float4 texCoord : TEXCOORD0;//返回贴图
		fixed4 color : COLOR;
		float4 tangent : TANGENT;    // tangent.w用来确定切线空间中坐标轴的方向的。
		float3 normal : NORMAL; // 不再使用模型自带的法线。保留该变量是因为切线空间是通过（模型里的）法线和（模型里的）切线确定的。
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
		float4 uv : TEXCOORD1; // xy存储MainTex的纹理坐标，zw存储NormalMap的纹理坐标
		fixed4 color : COLOR;
		float3 normal : NORMAL;
		float4 vertex : TEXCOORD2;
		float3 lightDir : TEXCOORD0;
	};

	sampler2D _MainTex,_SSSTex,_ILMTex;
	float4 _MainTex_ST;
	fixed _ShadowContrast,_DarkenInnerLine,_Shininess,_SpecStep;
	fixed4 _LightColor0;
	// fixed4 _WorldLightDir;
	sampler2D _NormalMap;
	float4 _NormalMap_ST; // 命名是固定的贴图名+后缀"_ST"，4个值前两个xy表示缩放，后两个zw表示偏移
	float _BumpScale;
	fixed4 _Color;
	v2f vert(appdata v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.normal = UnityObjectToWorldNormal(v.normal);
		o.uv.xy = v.texCoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
		o.uv.zw = v.texCoord.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
		TANGENT_SPACE_ROTATION;//调用这个后之后，会得到一个矩阵 rotation 
		//ObjSpaceLightDir(v.vertex)//得到模型空间下的平行光方向 
		o.color = v.color;
		o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex));
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 finCol;
	float2 offset = float2(0,0);
	
	fixed4 mainTex = tex2D(_MainTex,i.uv + offset);
	fixed4 sssTex = tex2D(_SSSTex,i.uv + offset);
	fixed4 ilmTex = tex2D(_ILMTex,i.uv + offset);

	finCol = mainTex;

	fixed3 brightCol = mainTex.rgb;
	fixed3 shadowCol = mainTex.rgb * sssTex.rgb;
	fixed lineCol = ilmTex.a;
	lineCol = lerp(lineCol,_DarkenInnerLine,step(lineCol,_DarkenInnerLine));

	fixed shadowThreshold = ilmTex.g;
	shadowThreshold *= i.color.r;
	shadowThreshold = 1 - shadowThreshold + _ShadowContrast;

	fixed4 normalColor = tex2D(_NormalMap, i.uv.zw+offset);
	//
	//	fixed3 tangentNormal = normalize(  normalColor.xyz * 2 - 1 ) ; //切线空间下的法线
	fixed3 tangentNormal = UnpackNormal(normalColor);
	tangentNormal.xy = tangentNormal.xy*_BumpScale;
	tangentNormal = normalize(tangentNormal);

	fixed3 lightDir = normalize(i.lightDir + _WorldSpaceLightPos0.xyz);
	//float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);/	//float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);//normalize(_WorldLightDir.xyz);
	float3 normalDir = normalize(i.normal);
	

	float3 viewDir = normalize(UnityWorldSpaceViewDir(i.vertex));
	float3 halfDir = normalize(viewDir + lightDir);

	float NdotL = dot(normalDir,lightDir);
	float NdotH = max(0,dot(normalDir,halfDir));

	fixed ilmTexR = ilmTex.r;
	fixed ilmTexB = ilmTex.b;

	float shadowContrast = step(shadowThreshold,NdotL);
	finCol.rgb = lerp(shadowCol,brightCol,shadowContrast);

	finCol.rgb += shadowCol * 0.5f*step(_SpecStep,ilmTexB*pow(NdotH,_Shininess*ilmTexR * 128)) *shadowContrast;
	finCol.rgb *= lineCol;

	finCol *= _LightColor0* _Color* max(dot(tangentNormal, lightDir) + 0.5, 0);
	finCol *= 1 +   _Color;

	finCol.a = mainTex.a;

	return finCol;
	}
		ENDCG
	}
	}
}
