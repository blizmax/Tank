  Shader "vertexPainter/vB_DiffuseNormal_3tex" {
    Properties {
		_Color 				("Main Color", Color) = (1,1,1,1)
		_MainTex1 			("Texture 1 (RGB)", 2D) = "white" {}
		_BumpMap1 			("Bumpmap 1 (RGB)", 2D) = "bump" {}
		_MainTex2 			("Texture 2 (RGB)", 2D) = "white" {}
		_BumpMap2 			("Bumpmap 2 (RGB)", 2D) = "bump" {}
		_MainTex3 			("Texture 3 (RGB)", 2D) = "white" {}
		_BumpMap3 			("Bumpmap 3 (RGB)", 2D) = "bump" {}
    }
    SubShader {
		
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert vertex:vert 
	  #pragma target 3.0	    
	  
		struct Input {
		  
			float2 uv_MainTex1 		: TEXCOORD0;
			float2 uv_MainTex2 		: TEXCOORD1;
			float2 uv_MainTex3		: TEXCOORD2;
			
			float4 color			: TEXCOORD3;
			
		};
		
		void vert (inout appdata_full v, out Input o) {
			
			o.color = (v.color);
			
      }

		uniform sampler2D _MainTex1, _MainTex2, _MainTex3;
		uniform sampler2D _BumpMap1, _BumpMap2, _BumpMap3;
	  
		fixed4 _Color;
		fixed _Tile;
	  
		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed4 col  = tex2D( _MainTex1,		IN.uv_MainTex1)*IN.color.r;
			col 		+= tex2D( _MainTex2,	IN.uv_MainTex2)*IN.color.g;
			col 		+= tex2D( _MainTex3,	IN.uv_MainTex3)*IN.color.b;
			
			fixed3 normal 	  = UnpackNormal(tex2D(_BumpMap1, 	IN.uv_MainTex1))*IN.color.r;
			normal 			+= UnpackNormal(tex2D(_BumpMap2, 	IN.uv_MainTex2))*IN.color.g;
			normal 			+= UnpackNormal(tex2D(_BumpMap3, 	IN.uv_MainTex3))*IN.color.b;

			o.Albedo = col * _Color.rgb;
			o.Normal = normal;

		}
	  
      ENDCG

	} 
FallBack "Diffuse"
  }
