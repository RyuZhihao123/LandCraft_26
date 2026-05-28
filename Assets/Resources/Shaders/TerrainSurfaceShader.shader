
Shader "Custom/TerrainSurfaceShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _MaskTex("Albedo (RGB)", 2D) = "white" {}
        _Blend("Texture Blend", Range(0,1)) = 0.0
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows

            #pragma target 3.0

            sampler2D _MainTex;
            sampler2D _MaskTex;

            struct Input
            {
                float2 uv_MainTex;
            };

            half _Glossiness;
            half _Metallic;
            fixed4 _Color;
            half _Blend;
            uniform float _TexCount;

            uniform float4 _MaskRects[50];


            UNITY_INSTANCING_BUFFER_START(Props)

            UNITY_INSTANCING_BUFFER_END(Props)

            int GetMaskLabel(float2 uvcoord)
            {
                float original_mask_val = (tex2D(_MaskTex, uvcoord).x) * _TexCount; //
                int label = floor(original_mask_val + 0.5f);

                return label;
            }

            float4 GetColorAt(float2 __uv, int local_mask_val, float2 __centerUV)
            {
                //int mask_val = (tex2D(_MaskTex, __uv).x) * _TexCount; //
                int mask_val = GetMaskLabel(__uv);
                //int mask_val = 3;
                int label = mask_val;
                float2 real_uv = __uv;
                if (mask_val == local_mask_val)
                    real_uv = __centerUV;

                float ratio = 5.0f;  //
                float precision = 1000000;
                int tmpZ = _MaskRects[label].z * precision;
                int tmpW = _MaskRects[label].w * precision;
                real_uv.x = ((int)(ratio * real_uv.x * precision) % tmpZ) / precision;  // with scaling ratio.
                real_uv.y = ((int)(ratio * real_uv.y * precision) % tmpW) / precision;

                //real_uv.x = real_uv.x * _MaskRects[label].z;  // no scaling ratio.
                //real_uv.y = real_uv.y * _MaskRects[label].w;
                real_uv.x += _MaskRects[label].x;
                real_uv.y += _MaskRects[label].y;


                return tex2D(_MainTex, real_uv);
            }

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                //int local_mask_val = (tex2D(_MaskTex, IN.uv_MainTex).x) * _TexCount;
                int local_mask_val = GetMaskLabel(IN.uv_MainTex);
                //int local_mask_val = 3;
                // Albedo comes from a texture tinted by color
                float4 color1 = GetColorAt(IN.uv_MainTex, local_mask_val, IN.uv_MainTex);
                float precision = 0.005;
                float4 color2 = GetColorAt(IN.uv_MainTex + float2(-precision, precision), local_mask_val, IN.uv_MainTex);
                float4 color3 = GetColorAt(IN.uv_MainTex + float2(-precision, -precision), local_mask_val, IN.uv_MainTex);
                float4 color4 = GetColorAt(IN.uv_MainTex + float2(precision, -precision), local_mask_val, IN.uv_MainTex);
                float4 color5 = GetColorAt(IN.uv_MainTex + float2(precision, precision), local_mask_val, IN.uv_MainTex);
                float4 avg_color = (color1 + color2 + color3 + color4 + color5) / 5.0f;
                fixed4 c = avg_color * _Color;
                //c = color1 * _Color;

                //float4 colors[8] = { float4(1,0,0,1),float4(0,1,0,1), float4(0,0,1,1), float4(1,1,0,1),
                //float4(1,0,1,1), float4(0,1,1,1), float4(1,0.5,0.5,1), float4(0.5,0.5,1,1) };

                o.Albedo = c.rgb;
                //o.Albedo = colors[local_mask_val].rgb;
                // Metallic and smoothness come from slider variables
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = c.a;
            }

            ENDCG
        }
            FallBack "Diffuse"
}