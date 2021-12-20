Shader "Magnorama/UnlitTexture" 
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Pass 
		{

			Tags { "Queue" = "AlphaTest" "LightMode" = "ForwardBase"}

			Lighting Off
			Cull Off
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;

				// Single Instanced Pass Rendering
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				float4 posWorld : TEXCOORD1;

				// Single Instanced Pass Rendering
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			// Magnorama Settings
			float _EnableClipping;
			float _ClipScale = 1;
			float4x4 _WorldToBox;
			float4 _ScaleCenter;

			v2f vert(appdata_t v)
			{
				v2f o;

				// Single Instanced Pass Rendering
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				// Scale points depending on Magnorama variables
				if (_EnableClipping > 0.5f)
				{
					float4 vertexPos = mul(unity_ObjectToWorld, v.vertex);
					vertexPos.xyz = _ScaleCenter.xyz + (_ClipScale * (vertexPos.xyz - _ScaleCenter.xyz));
					o.vertex = mul(UNITY_MATRIX_VP, vertexPos);
				}
				else
				{
					o.vertex = UnityObjectToClipPos(v.vertex);
				}

				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				return o;
			}

			fixed4  frag(v2f i) : SV_Target
			{
				// Clip fragments outside the Magnorama
				if (_EnableClipping > 0.5f)
				{
					float3 boxPosition = mul(_WorldToBox, i.posWorld);

					clip(boxPosition + 0.5);
					clip(0.5 - boxPosition);
				}


				return tex2D(_MainTex, i.texcoord);

			}
			ENDCG
		}

		// A second pass fixes the shadow in the area that is clipped through the Magnorama.
		Pass
		{
			Tags {"LightMode" = "ShadowCaster"}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"


			struct appdata_t {
				float4 vertex : POSITION;
				float4 normal : NORMAL;

				// Single Instanced Pass Rendering
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 posWorld : TEXCOORD1;
				V2F_SHADOW_CASTER;

				// Single Instanced Pass Rendering
				UNITY_VERTEX_OUTPUT_STEREO
			};

			// Magnorama Settings
			float _EnableClipping;
			float4x4 _WorldToBox;

			v2f vert(appdata_t v)
			{
				v2f o;

				// Single Instanced Pass Rendering
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

				o.posWorld = mul(unity_ObjectToWorld, v.vertex);

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				if (_EnableClipping > 0.5f)
				{
					float3 boxPosition = mul(_WorldToBox, i.posWorld);

					clip(boxPosition + 0.5);
					clip(0.5 - boxPosition);
				}
				SHADOW_CASTER_FRAGMENT(i)

			}
			ENDCG
		}
	}
	Fallback "VertexLit"
}
