
Shader "Magnorama/RenderDepth"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" "RenderType" = "Geometry" }
            Pass
            {
                CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                uniform sampler2D _MainTex;
                uniform sampler2D _CameraDepthTexture;
                uniform half4 _MainTex_TexelSize;

                float nearPlane;
                float farPlane;

                struct appdata_t
                {
                    float4 vertex : POSITION;                 
                    half2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    half2 uv : TEXCOORD0;
                };


                v2f vert(appdata_t i)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(i.vertex);
                    o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, i.uv);

                    // Flip texture coordinates if necessary
                    #if UNITY_UV_STARTS_AT_TOP
                    if (_MainTex_TexelSize.y < 0)
                            o.uv.y = 1 - o.uv.y;
                    #endif

                    return o;
                }

                float4 frag(v2f i) : COLOR
                {
                    float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));
                    depth = pow(Linear01Depth(depth), 1);

                    // depth on clipping planes needs to be corrected
                    float trueDepth = depth * farPlane;
                    if (trueDepth < nearPlane) depth = 0;
                    else depth = (trueDepth - nearPlane) / (farPlane - nearPlane);

                    return float4(depth, depth, depth, 0);
                }

                ENDCG
            }
        }
}