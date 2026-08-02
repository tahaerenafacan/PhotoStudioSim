Shader "PSSGame/LedPanel"
{
    Properties
    {
        // Panel üzerine yazılacak yazıyı (OPEN/CLOSED vb.) beyaz-siyah maske olarak tutan doku
        // Beyaz = LED yanar, siyah = LED söner. İçerik dışarıdan bir RenderTexture veya elle çizilmiş texture olabilir.
        _TextMask ("Text Mask (White = On)", 2D) = "black" {}

        // Panelin fiziksel LED çözünürlüğü (yatay x dikey nokta sayısı) - dot-matrix ızgarasını belirler
        _GridResolution ("Grid Resolution (X,Y)", Vector) = (32, 16, 0, 0)

        // Tek bir LED noktasının ızgara hücresi içindeki kapladığı alan oranı (1'e yakın = nokta aralığı sık)
        _DotSize ("Dot Fill Ratio", Range(0.1, 1)) = 0.65

        // Yanık LED rengi ve parlaklığı - Bloom ile birlikte gerçekçi ışıma için HDR yoğunluk
        _OnColor ("On Color (Emission)", Color) = (0, 3, 0.2, 1)

        // Sönük LED rengi - gerçek panellerde kapalıyken bile diyotlar hafif renkli görünür (ör. kırmızı diyot camı)
        _OffColor ("Off Color (Idle Diode Tint)", Color) = (0.05, 0.01, 0.01, 1)

        // Panel gövdesi / PCB zemin rengi (LED noktalarının arasındaki alan)
        _PanelColor ("Panel Body Color", Color) = (0.05, 0.05, 0.05, 1)

        // Hafif titreme/tarama efekti için opsiyonel yoğunluk (0 = kapalı, gerçekçi panel hissi için düşük tutulmalı)
        _FlickerAmount ("Scan Flicker Amount", Range(0, 0.3)) = 0.05
        _FlickerSpeed ("Scan Flicker Speed", Range(0, 20)) = 6
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLED"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_TextMask);
            SAMPLER(sampler_TextMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _TextMask_ST;
                float4 _GridResolution;
                float _DotSize;
                float4 _OnColor;
                float4 _OffColor;
                float4 _PanelColor;
                float _FlickerAmount;
                float _FlickerSpeed;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionHCS = TransformObjectToHClip(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _TextMask);
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                // UV'yi ızgara hücrelerine böl - her hücre bir fiziksel LED'i temsil eder
                float2 gridUV = input.uv * _GridResolution.xy;
                float2 cellIndex = floor(gridUV);
                float2 cellLocal = frac(gridUV) - 0.5; // hücre merkezine göre -0.5..0.5 aralığı

                // Yazı maskesini her LED hücresinin tam merkezinden örnekle, aksi halde nokta kenarlarında bulanık okunur
                float2 sampleUV = (cellIndex + 0.5) / _GridResolution.xy;
                float mask = SAMPLE_TEXTURE2D(_TextMask, sampler_TextMask, sampleUV).r;

                // Hücre içinde dairesel bir LED noktası çiz (dot-matrix görünümü için)
                float dist = length(cellLocal);
                float dot = 1.0 - smoothstep(_DotSize * 0.5 - 0.02, _DotSize * 0.5, dist);

                // Gerçek panellerde tarama sırasında hafif parlaklık dalgalanması olur, isteğe bağlı ince detay
                float flicker = 1.0 - _FlickerAmount * 0.5 + _FlickerAmount * sin(_Time.y * _FlickerSpeed + cellIndex.x * 0.3);

                float3 ledColor = lerp(_OffColor.rgb, _OnColor.rgb * flicker, mask);
                float3 finalColor = lerp(_PanelColor.rgb, ledColor, dot);

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
