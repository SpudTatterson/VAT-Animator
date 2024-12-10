Shader "Custom/URP_VAT_Normalized"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _VATPosition ("VAT Position Texture", 2D) = "white" {}
        _TimeScale ("Time Scale", Float) = 1.0
        _FrameCount ("Frame Count", Float) = 29
        _VATTextureWidth ("VAT Texture Width", Float) = 5874
        _MinBounds ("Min Bounds", Vector) = (0,0,0)
        _MaxBounds ("Max Bounds", Vector) = (1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 vertexID : TEXCOORD1;  // Vertex ID for VAT
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION; 
                float3 worldPosition : TEXCOORD1; 
                float4 shadowCoord : TEXCOORD5;
            };

            sampler2D _VATPosition;
            float _TimeScale;
            float _FrameCount;
            float _VATTextureWidth;
            float3 _MinBounds;
            float3 _MaxBounds;

            float4 _BaseColor;

            Varyings vert(Attributes v)
            {
                Varyings o;

                // Calculate animation frame index
                float animTime = _TimeParameters.x * _TimeScale; // Unity time
                float frameIndex = fmod(animTime, _FrameCount - 1.0);
                float normalizedFrame = frameIndex / (_FrameCount - 1.0);

                // Get the column index for this vertex from vertex ID
                float vertexIndex = v.vertexID.x;
                float normalizedVertexIndex = vertexIndex / (_VATTextureWidth - 1.0);

                // Sample normalized position from VAT texture
                float3 normalizedPosition = tex2Dlod(_VATPosition, float4(normalizedVertexIndex, normalizedFrame, 0, 0)).rgb;

                // Denormalize the position to the original local space
                float3 animatedPositionOS = lerp(_MinBounds.xyz, _MaxBounds.xyz, normalizedPosition);

                // Transform to world space
                float3 worldPosition = TransformObjectToWorld(animatedPositionOS);

                // Transform to clip space
                o.positionHCS = TransformWorldToHClip(worldPosition);
                o.worldPosition = worldPosition;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                
                i.shadowCoord = TransformWorldToShadowCoord(i.positionHCS);
                
                //shadow
                float shadow = MainLightRealtimeShadow(i.shadowCoord);

                float3 diffuse = _MainLightColor.rgb * _BaseColor * shadow;

                return half4(_BaseColor.rgb, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
