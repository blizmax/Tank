  Shader "vertexPainter/vB_DiffuseNormal_2tex" {
    Properties {
		_Color 				("Main Color", Color) = (1,1,1,1)
		_MainTex1 			("Texture 1 (RGB)", 2D) = "white" {} 
		_BumpMap1 			("Bumpmap 1 (RGB)", 2D) = "bump" {}
		_MainTex2 			("Texture 2 (RGB)", 2D) = "white" {}  
		_BumpMap2 			("Bumpmap 1 (RGB)", 2D) = "bump" {}
    }
    SubShader {
		
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf BlinnPhong vertex:vert 
	  #pragma target 3.0	    				   
	  #pragma target 3.0	    
	  
		struct Input {
		  
			float2 uv_MainTex1 		: TEXCOORD0;
			float2 uv_MainTex2 		: TEXCOORD1;
			
			float4 color			: TEXCOORD3;
			
		};
		
		void vert (inout appdata_full v, out Input o) {
			
			o.color = (v.color);
			
      }

		uniform sampler2D _MainTex1, _MainTex2;
		uniform sampler2D _BumpMap1, _BumpMap2;
	  
		fixed4 _Color;
		fixed _Tile;
	  
		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed4 col  = tex2D( _MainTex1,		IN.uv_MainTex1)*IN.color.r;
			col 		+= tex2D( _MainTex2,	IN.uv_MainTex2)*IN.color.g;
			
			fixed3 normal 	  = UnpackNormal(tex2D(_BumpMap1, 	IN.uv_MainTex1))*IN.color.r;
			normal 			+= UnpackNormal(tex2D(_BumpMap2, 	IN.uv_MainTex2))*IN.color.g;

			o.Albedo = col * _Color.rgb;
			o.Normal = normal;

		}
	  
      ENDCG

	} 
FallBack "Diffuse"
  }
