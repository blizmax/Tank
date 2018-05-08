  Shader "vertexPainter/vB_Diffuse_2tex" {
    Properties {
		_Color 				("Main Color", Color) = (1,1,1,1)
		_MainTex1 			("Texture 1 (RGB)", 2D) = "white" {}
		_MainTex2 			("Texture 2 (RGB)", 2D) = "white" {}	
    }
    SubShader {
		
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert vertex:vert 
	  
		struct Input {
		  
			float2 uv_MainTex1 		: TEXCOORD0;
			float2 uv_MainTex2 		: TEXCOORD1;  
			
			float4 color			: TEXCOORD3;
			
		};
		  		
		void vert (inout appdata_full v, out Input o) {
					   			
			o.color = (v.color);
			
      }

		uniform sampler2D _MainTex1, _MainTex2;
	  
		fixed4 _Color;
		fixed _Tile;
	  
		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed4 col  = tex2D( _MainTex1,		IN.uv_MainTex1)*IN.color.r;
			col 		+= tex2D( _MainTex2,	IN.uv_MainTex2)*IN.color.g;	

			o.Albedo = col * _Color.rgb;

		}
	  
      ENDCG

	} 

FallBack "Diffuse"
  }
