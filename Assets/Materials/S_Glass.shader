Shader "PSSGame/Glass"
{
    Properties
    {
        // Ana cam rengi ve saydamlık - alpha, yansıma/kırılma karışım oranını da etkiler
        _TintColor ("Tint Color", Color) = (0.8, 0.9, 1.0, 0.15)

        // Yüzey pürüzlülüğü - düşük değer daha net yansıma/kırılma verir (temiz cam hissi)
        _Roughness ("Roughness", Range(0,1)) = 0.05

        // Fresnel gücü - kenarlardan bakışta camın daha opak/parlak görünmesini sağlar
        _FresnelPower ("Fresnel Power", Range(0.1, 8)) = 3
        _FresnelTint ("Fresnel Tint", Color) = (1,1,1,1)

        // Arkadaki sahneyi ne kadar büktüğünü kontrol eder (kırılma miktarı)
        _RefractionStrength ("Refraction Strength", Range(0, 0.2)) = 0.05

        // Normal map ile cam yüzeyine ince pürüz/dalga detayı eklenir
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpStrength ("Normal Strength", Range(0,2)) = 0.3

        // Specular highlight boyutu ve yoğunluğu
        _SpecularPower ("Specular Power", Range(1, 256)) = 64
        _SpecularIntensity ("Specular Intensity", Range(0,4)) = 1.5
    }

    SubShader
    {
        // Transparent + son sırada çizilir, çünkü arkasındaki opak sahneyi örneklemesi gerekir
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardGlass"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // Solo geliştirici kapsamı için sadece temel URP değişkenleri (ekstra varyant maliyetinden kaçınmak amacıyla)
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Unity 6'da opak sahne _CameraOpaqueTexture üzerinden örneklenir (Opaque Texture ayarı Renderer Asset'te açık olmalı)
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _TintColor;
                float4 _FresnelTint;
                float _Roughness;
                float _FresnelPower;
                float _RefractionStrength;
                float4 _BumpMap_ST;
                float _BumpStrength;
                float _SpecularPower;
                float _SpecularIntensity;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 screenPos   : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 tangentWS   : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float3 viewDirWS   : TEXCOORD4;
                float2 uv          : TEXCOORD5;
            };

            // Vertex verisini dünya uzayına taşıyıp ekran pozisyonunu hesaplar (kırılma örneklemesi için gerekli)
            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionHCS = posInputs.positionCS;
                output.screenPos = ComputeScreenPos(posInputs.positionCS);
                output.normalWS = normInputs.normalWS;
                output.tangentWS = normInputs.tangentWS;
                output.bitangentWS = normInputs.bitangentWS;
                output.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BumpMap);

                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float3 viewDirWS = normalize(input.viewDirWS);

                // Normal map'i tanjant uzayından dünya uzayına çevir - cam yüzeyine hafif dalgalanma katar
                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpStrength);
                float3x3 tbn = float3x3(normalize(input.tangentWS), normalize(input.bitangentWS), normalize(input.normalWS));
                float3 normalWS = normalize(mul(normalTS, tbn));

                // Fresnel: kenarlardan bakışta camı daha belirgin/opak yapar, gerçekçi cam hissi için önemli
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);

                // Arkadaki opak sahneyi normal yönünde hafifçe kaydırarak kırılma illüzyonu oluştur
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 refractOffset = normalWS.xy * _RefractionStrength;
                float3 sceneColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + refractOffset).rgb;

                // Ana ışık için basit Blinn-Phong specular (solo proje kapsamında tam PBR yerine hafif ve yeterli çözüm)
                Light mainLight = GetMainLight();
                float3 halfDir = normalize(mainLight.direction + viewDirWS);
                float specAngle = saturate(dot(normalWS, halfDir));
                float specular = pow(specAngle, _SpecularPower) * _SpecularIntensity;

                // Kırılan sahne rengiyle cam tintini karıştır, fresnel ve specular'ı üstüne ekle
                float3 glassColor = lerp(sceneColor, _TintColor.rgb, _TintColor.a);
                glassColor += fresnel * _FresnelTint.rgb;
                glassColor += specular * mainLight.color;

                // Roughness arttıkça sahne detayının netliğini biraz azalt (basit yaklaşım, gerçek blur maliyeti yok)
                glassColor = lerp(glassColor, _TintColor.rgb, _Roughness * 0.5);

                float alpha = saturate(_TintColor.a + fresnel * 0.5);

                return float4(glassColor, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
