  Shader "vertexPainter/vB_DiffuseNormal_4tex" {
    Properties {
		_Color 				("Main Color", Color) = (1,1,1,1)
		_SpecColor 		("Specular Color", Color) = (0.5, 0.5, 0.5, 0)
		_Shininess 		("Shininess", float) = 1.0
		_MainTex1 		("Texture 1 (RGB), Spec (A)", 2D) = "white" {}
		_BumpMap1 		("Bumpmap 1 (RGB)", 2D) = "bump" {}
		_MainTex2 		("Texture 2 (RGB), Spec (A)", 2D) = "white" {}
		_BumpMap2 		("Bumpmap 2 (RGB)", 2D) = "bump" {}
		_MainTex3 		("Texture 3 (RGB), Spec (A)", 2D) = "white" {}
		_BumpMap3 		("Bumpmap 3 (RGB)", 2D) = "bump" {}
		_MainTex4 		("Texture 4 (RGB), Spec (A)", 2D) = "white" {}
		_BumpMap4 		("Bumpmap 4 (RGB)", 2D) = "bump" {}
    }
    SubShader {
		
      Tags { "RenderType" = "Opaque" }
	  LOD 600
      CGPROGRAM
      #pragma surface surf BlinnPhong vertex:vert 
	  #pragma target 3.0
	  
		struct Input {
		  
			float2 uv_MainTex1 			: TEXCOORD0;
			float2 uv_MainTex2 			: TEXCOORD1;
			float2 uv_MainTex3			: TEXCOORD2; 
			float2 uv_MainTex4			: TEXCOORD3;
			
			float4 color				: TEXCOORD4;
			
		};
		
		void vert (inout appdata_full v, out Input o) {
			
			o.color = (v.color);
			
      }

		uniform sampler2D _MainTex1, _MainTex2, _MainTex3, _MainTex4;
		uniform sampler2D _BumpMap1, _BumpMap2, _BumpMap3, _BumpMap4;
	  
		fixed4 _Color;
		fixed _Shininess, _Tile;
	  
		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed4 col = tex2D( _MainTex1,		IN.uv_MainTex1)*IN.color.r;
			col 		+= tex2D( _MainTex2,	IN.uv_MainTex2)*IN.color.g;
			col 		+= tex2D( _MainTex3,	IN.uv_MainTex3)*IN.color.b;
			col 		+= tex2D( _MainTex4,	IN.uv_MainTex4)*IN.color.a;
			
			fixed3 normal 	  = UnpackNormal(tex2D(_BumpMap1, 	IN.uv_MainTex1))*IN.color.r;
			normal 			+= UnpackNormal(tex2D(_BumpMap2, 	IN.uv_MainTex2))*IN.color.g;
			normal 			+= UnpackNormal(tex2D(_BumpMap3, 	IN.uv_MainTex3))*IN.color.b;
			normal 			+= UnpackNormal(tex2D(_BumpMap4, 	IN.uv_MainTex4))*IN.color.a;

			o.Albedo 	= col * _Color.rgb;
			o.Normal 	= normal;

		}
	  
      ENDCG

	} 
FallBack "Diffuse"
  }
