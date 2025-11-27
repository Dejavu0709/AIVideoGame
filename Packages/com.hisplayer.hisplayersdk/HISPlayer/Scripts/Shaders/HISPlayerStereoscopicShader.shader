Shader "HISPlayer/HISPlayerStereoscopicShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _FlipVertically("Flip Vertically", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque"}
        LOD 100

        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #pragma multi_compile __ COLOR_CORRECTION

            #include "UnityCG.cginc"

            bool _FlipVertically;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Remove this duplicate declaration of _MainTex
            // sampler2D _MainTex;

            float4 _MainTex_ST; // Declare _MainTex_ST

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Flip texture vertically
                if(_FlipVertically > 0.5)
                    o.uv.y = 1 - o.uv.y; // Flip the Y coordinate vertically

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
                // Sample the texture using UNITY_SAMPLE_SCREENSPACE_TEXTURE
                fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);

                #if !UNITY_COLORSPACE_GAMMA
                    col.rgb = GammaToLinearSpace(col.rgb); // Remove gamma correction
                #endif

                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
