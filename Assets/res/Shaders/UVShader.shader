// Shader 名字，表示怎么样来找到这个Shader
// Unity特有的语法: ShaderLab
// Cg:
Shader "Custom/UVShader"
{
	// 属性，描述了，这个Shader选择后绑定到编辑器的属性成员;
	Properties
	{
		// 变量名字("显示名字", 数据类型) = "默认值"
		_MainTex ("MyTexture", 2D) = "white" {}
		_SubTex ("SubTexure", 2D) = "white" {}
	}
	// end
	// 
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		// 绘制通道:3D模型数据 + 参数-->通道-->屏幕上;
		// Shader--> GPU编写的代码开发语言;
		// GPU: HLSL, GLSL, (DirectorX + Opengl) Cg;
		Pass
		{
			CGPROGRAM // 开始插入Cg代码
			#pragma vertex vert  // 制定了顶点shader的函数入口
			#pragma fragment frag // 指定了frag shader的函数入口
			
			// Unity会为Shader做好了很多库函数，方便我们的开发者来使用;
			#include "UnityCG.cginc"

			struct appdata
			{
				// 语义描述符号，有了这个，我们上一个位置就可以把上一个
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _SubTex;

			// float4 Cg 4维向量(x, y, z, w), v.x, v.y, v.z, v.w
			// float2 Cg 2维向量(x, y) 
			// fixed4 color 颜色向量(r, g, b, a)
			// sampler2D Cg纹理类型;

			float4 _MainTex_ST;
			
			// 顶点Shader代码入口
			// 参数，表示上一个位置的输入;
			// 返回: 表示返回哪些数据给下一个位置使用;
			// 顶点变化

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				return o;
			}
			// 颜色着色
			fixed4 frag (v2f i) : SV_Target
			{
				float2 offset = float2(0, 0);
				float deta = sin(2*_Time.y)- 0.5f;
				// _Time对象，能够获取时间 float4 --> (x, y, z, w) --> (运行时间/20, 运行的时间, 2倍运行时间, 3倍运行时间)
				offset.y = deta;
				fixed4 light = tex2D(_SubTex, i.uv + offset);
				

				// sample the texture
				// Cg的库函数，用来给一个坐标，纹理，返回这个位置的颜色;
				fixed4 col = tex2D(_MainTex, i.uv) + light;
				return col;
			}
			ENDCG // 结束插入;
		} // end Pass
	} // end SubShader
} // end shader
