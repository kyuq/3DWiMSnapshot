Shader "Magnorama/MeshFromComputeBuffer"{

	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
	}
	
	SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }

		Pass
		{
			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex; 

			//Buffer interface for compute buffers
			StructuredBuffer<float3> vertices;
			StructuredBuffer<float2> uv;
			StructuredBuffer<int> triangles;

			// Magnorama Settings
			float _EnableClipping;
			float _ClipScale = 1;
			float4x4 _WorldToBox;
			float4x4 _RoI2Mag;
			float4 _ScaleCenter;

			struct appdata
			{
				uint vertex_id: SV_VertexID;

				// Single Instanced Pass Rendering
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 posWorld : TEXCOORD1;

				// Single Instanced Pass Rendering
				UNITY_VERTEX_OUTPUT_STEREO
			};

			//the vertex shader function
			v2f vert(appdata v){
				v2f o;

				// Single Instanced Pass Rendering
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				// Read triangle from compute buffer
				int positionID = triangles[v.vertex_id];

				if (positionID >= 0)
				{
					o.uv = uv[positionID];

					float4 vertex = mul(_RoI2Mag, float4(vertices[positionID], 1));
	
					o.posWorld = mul(unity_ObjectToWorld, vertex);
					// Scale points depending on Magnorama variables
					if (_EnableClipping > 0.5f)
					{
						float4 vertexPos = mul(unity_ObjectToWorld, vertex);
						vertexPos.xyz = _ScaleCenter.xyz + (_ClipScale * (vertexPos.xyz - _ScaleCenter.xyz));
						o.vertex = mul(UNITY_MATRIX_VP, vertexPos);
					}
					else
					{
						o.vertex = UnityObjectToClipPos(vertex);
					}
				}


				return o;
			}


			fixed4 frag(v2f i) : SV_TARGET
			{
				// Clip fragments outside the Magnorama
				if (_EnableClipping > 0.5f)
				{
					float3 boxPosition = mul(_WorldToBox, i.posWorld);

					clip(boxPosition + 0.5);
					clip(0.5 - boxPosition);
				}
			  return tex2D(_MainTex, i.uv);
			}

			ENDCG
		}


	}
		Fallback "Diffuse"
}
