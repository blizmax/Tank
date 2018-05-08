  Shader "vertexPainter/vB_map_DiffuseNormal_2tex" {
	Properties {
		_Color 			("Main Color", Color) = (1,1,1,1)
		_blendPower 	("Blend Factor", float) = 50.0	
		_cutoff 		("Cutoff", float) = 5.0
															 
		_MainTex1 		("Texture 1 (RGB) BlendMap (A)", 2D) = "white" {}	   
		_BumpMap1 		("Bump Map 1", 2D) = "white" {}

		_MainTex2 		("Texture 2 (RGB)", 2D) = "white" {}		   
		_BumpMap2 		("Bump Map 2", 2D) = "white" {}
	}
	SubShader {
		
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:vert
		#pragma target 3.0
	  
		struct Input {
		  
			float2 uv_MainTex1		 	: TEXCOORD0;
			float2 uv_MainTex2 			: TEXCOORD1; 
			float4 color 				: TEXCOORD2;
			
		};
		
		void vert (inout appdata_full v, out Input o) {
			
			o.color = v.color;
			
	  }

		uniform sampler2D _MainTex1,_MainTex2, _BumpMap1 , _BumpMap2;
	  
		fixed4 _Color;
		fixed  _blendVal, _blendPower , _cutoff;
	  
		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed4 mTex1 		= tex2D (_MainTex1, IN.uv_MainTex1);
			fixed3 mTex2 		= tex2D (_MainTex2, IN.uv_MainTex2).rgb;  

			fixed3 mNorm1 		= UnpackNormal(tex2D(_BumpMap1, IN.uv_MainTex1));
			fixed3 mNorm2 		= UnpackNormal(tex2D(_BumpMap2, IN.uv_MainTex2));
			
			fixed vertBlend 		= IN.color.r;	 
			
			_blendVal = pow((vertBlend + (	(1 - vertBlend)	*	mTex1.a) + (mTex1.a *	vertBlend)	* _blendPower	), _cutoff);
																					
			_blendVal = clamp(_blendVal, 0.0, 1.0);
			
			o.Albedo = lerp(mTex2,mTex1,_blendVal) * _Color;	

			o.Normal = lerp(mNorm2,mNorm1,_blendVal);
									
		}
	  
	  ENDCG

	} 
FallBack "Diffuse"
  }