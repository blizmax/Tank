Shader "vertexPainter/VertexColorsOnly" { 
				 
	SubShader {

	
		BindChannels {
		
			Bind "Vertex", vertex
			Bind "Color", color
		
		} 
		
		Pass {

		
			Material {
			
				Diffuse [_Color]
			
			}
			
			Lighting Off
		
		}
	
	} 
 
FallBack "Diffuse"

}


