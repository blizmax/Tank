  Shader "vertexPainter/vB_Diffuse_unLit_3tex" {
	Properties {
		_Color 				("Main Color", Color) = (1,1,1,1)
		_MainTex1 			("Texture 1 (RGB)", 2D) = "white" {}
		_MainTex2 			("Texture 2 (RGB)", 2D) = "white" {}	
		_MainTex3 			("Texture 2 (RGB)", 2D) = "white" {}
	}

	SubShader {
		
	  Tags { "RenderType" = "Opaque" }
	  CGPROGRAM
	  #pragma surface surf Lambert vertex:vert 
	  
		struct Input {
		  
			float2 uv_MainTex1 		: TEXCOORD0;
			float2 uv_MainTex2 		: TEXCOORD1;   
			float2 uv_MainTex3 		: TEXCOORD2;
			
			float4 color			: TEXCOORD3;
			
		};
		
		void vert (inout appdata_full v, out Input o) {
			
			o.color = (v.color);
			
		}

		uniform sampler2D _MainTex1, _MainTex2, _MainTex3;
	  
		fixed4 _Color;
		fixed _Tile;
	  
		void surf (Input IN, inout SurfaceOutput o) {
			
			fixed4 col	= tex2D( _MainTex1,		IN.uv_MainTex1)*IN.color.r;
			col 		+= tex2D( _MainTex2,	IN.uv_MainTex2)*IN.color.g;	   
			col 		+= tex2D( _MainTex3,	IN.uv_MainTex3)*IN.color.b;	 	

			o.Albedo = col * _Color.rgb;

		//	o.Albedo = (0,0,0,0);

		}
	  
	  ENDCG

	}  

	SubShader{
			
		Tags { "Queue" = "Geometry" }

		Pass{  

		Tags {"LightMode"="Vertex" }
		  
			GLSLPROGRAM  		

				uniform sampler2D _MainTex1, _MainTex2, _MainTex3;

				uniform lowp vec4 _Color;

				varying lowp vec4 color;

				#ifdef VERTEX
				void main(){ 
 
					gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
													
					gl_TexCoord[0] = gl_MultiTexCoord0;   

					color = gl_Color;

				}
				#endif
			
				#ifdef VERTEX
				void main(){ 	
									   
					vec4 col = texture2D( _MainTex1,gl_TexCoord[0].st) * color.r;
  
					col += texture2D( _MainTex2,gl_TexCoord[0].st) * color.g;
	   
					col += texture2D( _MainTex3,gl_TexCoord[0].st) * color.b; 

					col *= _Color;

					gl_FragColor = col;

				} 
				#endif

			#endif   
					   
			ENDGLSL

		} 
	
	Pass {
		Tags {"LightMode"="VertexLM" }
		GLSLPROGRAM
		uniform lowp mat4 unity_LightmapMatrix;
		varying lowp vec2 uv, uv2; 
		varying lowp vec4 color;  
				
		#ifdef VERTEX
		void main() {
			gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
			uv = gl_MultiTexCoord0.xy;	 
			color = gl_Color;
			uv2 = vec2(unity_LightmapMatrix[0].x, unity_LightmapMatrix[1].y)
				* gl_MultiTexCoord1.xy + unity_LightmapMatrix[3].xy;
		}
		#endif
		
		#ifdef FRAGMENT
		uniform lowp sampler2D _MainTex1, _MainTex2, _MainTex3, unity_Lightmap;
		void main() {										   	 
			gl_FragColor = vec4((
				  (texture2D(_MainTex1, uv).rgb * color.r) 
				+ (texture2D(_MainTex2, uv).rgb * color.g)  
				+ (texture2D(_MainTex3, uv).rgb * color.b))
				* (texture2D(unity_Lightmap, uv2).rgb * 2.), 1);
		}
		#endif		
		ENDGLSL
	}
	
	Pass {	// Editor only - Graphics Emulation uses the wrong lightmap type
		Tags {"LightMode"="VertexLMRGBM" }
		GLSLPROGRAM
		uniform lowp mat4 unity_LightmapMatrix;
		varying lowp vec2 uv, uv2;	
		varying lowp vec4 color;  
				
		#ifdef VERTEX
		void main() {
			gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
			uv = gl_MultiTexCoord0.xy; 
			color = gl_Color;
			uv2 = vec2(unity_LightmapMatrix[0].x, unity_LightmapMatrix[1].y)
				* gl_MultiTexCoord1.xy + unity_LightmapMatrix[3].xy;
		}
		#endif
		
		#ifdef FRAGMENT												
		uniform lowp sampler2D _MainTex1, _MainTex2, _MainTex3, unity_Lightmap;
		void main() {

			gl_FragColor = vec4((
				  (texture2D(_MainTex1, uv).rgb * color.r) 
				+ (texture2D(_MainTex2, uv).rgb * color.g)  
				+ (texture2D(_MainTex3, uv).rgb * color.b))
				* (texture2D(unity_Lightmap, uv2).rgb * 2.), 1);

		}
		#endif		
		ENDGLSL
	}
	}

}
