  Shader "vertexPainter/vB_map_Diffuse_3tex" {
    Properties {
		_Color 			("Main Color", Color) = (1,1,1,1)
		_blendPower 	("Blend Factor", float) = 50.0	
		_cutoff 		("Cutoff", float) = 5.0
															 
		_MainTex1 		("Texture 1 (RGB) BlendMap (Alpha Channel)", 2D) = "white" {}

		_MainTex2 		("Texture 2 (RGB) ", 2D) = "white" {}

		_MainTex3 		("Texture 3 (RGB) ", 2D) = "white" {}	
    }
    SubShader {
		
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		//#pragma target 3.0
	  
		struct Input {
		  
			float2 uv_MainTex1		 	: TEXCOORD0;
			float2 uv_MainTex2 			: TEXCOORD1; 
			float2 uv_MainTex3 			: TEXCOORD2;  
			float4 color 				: TEXCOORD3;
			
		};
		
		void vert (inout appdata_full v, out Input o) {
			
			o.color = v.color;
			
      }

		uniform sampler2D _MainTex1,_MainTex2,_MainTex3;
	  
		fixed4 _Color;
		fixed  _blendVal, _blendPower , _cutoff;
	  
		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed4 mTex1 		= tex2D (_MainTex1, IN.uv_MainTex1);
			fixed3 mTex2 		= tex2D (_MainTex2, IN.uv_MainTex2).rgb;	 
			fixed3 mTex3 		= tex2D (_MainTex3, IN.uv_MainTex3).rgb; 

			fixed3 col			= mTex1*IN.color.r;	  
			col 				+= mTex2*IN.color.g;
			col 				+= mTex3*IN.color.b;   
			
			fixed vertBlend 		= IN.color.r;	 
			
			_blendVal = pow((vertBlend + (	(1 - vertBlend)	*	mTex1.a) + (mTex1.a *	vertBlend)	* _blendPower	), _cutoff);
																		 			
			_blendVal = clamp(_blendVal, 0.0, 1.0);
			
			o.Albedo = lerp(col,mTex1,_blendVal) * _Color; 
							  									
		}
	  
      ENDCG

	} 
FallBack "Diffuse"
  }
