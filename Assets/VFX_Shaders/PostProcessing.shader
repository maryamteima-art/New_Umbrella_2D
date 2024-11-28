Shader "Custom/PostProcessing"
{
    Properties
    {
        //LUT texture
        _LUT ("Look-Up Texture (LUT)", 2D) = "white" {}
        //Blend strength
        _LUTStrength ("LUT Strength", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            //Texture samplers
            //The screen texture automatically passed by Unity
            sampler2D _MainTex;    
            //LUT texture
            sampler2D _LUT;        

            //Properties
            //Strength of LUT blending
            float _LUTStrength;    

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            //LUT Sampling Function
            fixed3 SampleLUT(sampler2D lut, fixed3 color)
            {
                //Calculate the blue channel offset for LUT sampling
                float blueOffset = color.b * 31.0; // For a 32x32 LUT grid
                float2 lutUV = float2(color.r, color.g / 32.0 + blueOffset / 32.0);

                //Sample LUT with higher precision using tex2Dlod
                return tex2Dlod(lut, float4(lutUV, 0.0, 0.0)).rgb;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //Sample the screen texture
                fixed4 col = tex2D(_MainTex, i.uv);

                //Map the color to the LUT
                fixed3 mappedColor = SampleLUT(_LUT, col.rgb);

                //Blend the LUT effect with the original color
                col.rgb = lerp(col.rgb, mappedColor, _LUTStrength);

                return col;
            }
            ENDCG
        }
    }
}