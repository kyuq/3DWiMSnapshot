Shader "Magnorama/WireframeEdge"
{
    Properties
    {
        [PowerSlider(3.0)]
        _WireframeVal("Wireframe width", Range(0., 0.5)) = 0.05
        _FrontColor("Front color", color) = (1., 1., 1., 1.)
        _BackColor("Back color", color) = (1., 1., 1., 1.)
        [Toggle] _RemoveDiag("Remove diagonals?", Float) = 0.
    }
        SubShader
        {
            Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }

            Pass
            {
                Cull Front
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma geometry geom

            // Change "shader_feature" with "pragma_compile" if you want set this keyword from c# code
            #pragma shader_feature __ _REMOVEDIAG_ON

            #include "UnityCG.cginc"

            struct v2g {
                float4 worldPos : SV_POSITION;

                // Single Instanced Pass Rendering
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct g2f {
                float4 pos : SV_POSITION;
                float3 bary : TEXCOORD0;

                // Single Instanced Pass Rendering
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2g vert(appdata_base v) {
                v2g o;

                // Single Instanced Pass Rendering
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.worldPos =  mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) 
            {

                // Single Instanced Pass Rendering
                UNITY_SETUP_INSTANCE_ID(IN[0]);
                UNITY_SETUP_INSTANCE_ID(IN[1]);
                UNITY_SETUP_INSTANCE_ID(IN[2]);

                float3 param = float3(0., 0., 0.);

                #if _REMOVEDIAG_ON
                float EdgeA = length(IN[0].worldPos - IN[1].worldPos);
                float EdgeB = length(IN[1].worldPos - IN[2].worldPos);
                float EdgeC = length(IN[2].worldPos - IN[0].worldPos);

                if (EdgeA > EdgeB && EdgeA > EdgeC)
                    param.y = 1.;
                else if (EdgeB > EdgeC && EdgeB > EdgeA)
                    param.x = 1.;
                else
                    param.z = 1.;
                #endif

                g2f o;

                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
                o.bary = float3(1., 0., 0.) + param;
                triStream.Append(o);

                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
                o.bary = float3(0., 0., 1.) + param;
                triStream.Append(o);

                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
                o.bary = float3(0., 1., 0.) + param;
                triStream.Append(o);
            }

            float _WireframeVal;
            fixed4 _BackColor;

            fixed4 frag(g2f i) : SV_Target 
            {

                if (!any(bool3(i.bary.x < _WireframeVal, i.bary.y < _WireframeVal, i.bary.z < _WireframeVal)))
                     discard;

                    return _BackColor;
                }

                ENDCG
            }

        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

                // Change "shader_feature" with "pragma_compile" if you want set this keyword from c# code
                #pragma shader_feature __ _REMOVEDIAG_ON

                #include "UnityCG.cginc"

                struct v2g {
                    float4 worldPos : SV_POSITION;

                    // Single Instanced Pass Rendering
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct g2f {
                    float4 pos : SV_POSITION;
                    float3 bary : TEXCOORD0;

                    // Single Instanced Pass Rendering
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                v2g vert(appdata_base v) {
                    v2g o;

                    // Single Instanced Pass Rendering
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    return o;
                }

                [maxvertexcount(3)]
                void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {

                    // Single Instanced Pass Rendering
                    UNITY_SETUP_INSTANCE_ID(IN[0]);
                    UNITY_SETUP_INSTANCE_ID(IN[1]);
                    UNITY_SETUP_INSTANCE_ID(IN[2]);

                    float3 param = float3(0., 0., 0.);

                    #if _REMOVEDIAG_ON
                    float EdgeA = length(IN[0].worldPos - IN[1].worldPos);
                    float EdgeB = length(IN[1].worldPos - IN[2].worldPos);
                    float EdgeC = length(IN[2].worldPos - IN[0].worldPos);

                    if (EdgeA > EdgeB && EdgeA > EdgeC)
                        param.y = 1.;
                    else if (EdgeB > EdgeC && EdgeB > EdgeA)
                        param.x = 1.;
                    else
                        param.z = 1.;
                    #endif

                    g2f o;

                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
                    o.bary = float3(1., 0., 0.) + param;
                    triStream.Append(o);

                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
                    o.bary = float3(0., 0., 1.) + param;
                    triStream.Append(o);

                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
                    o.bary = float3(0., 1., 0.) + param;
                    triStream.Append(o);
                }

                float _WireframeVal;
                fixed4 _FrontColor;

                fixed4 frag(g2f i) : SV_Target {
                if (!any(bool3(i.bary.x <= _WireframeVal, i.bary.y <= _WireframeVal, i.bary.z <= _WireframeVal)))
                     discard;

                    return _FrontColor;
                }

                ENDCG
            }
        }
}