//Shader "Mobile/StyleGrass"
//{
//    Properties
//    {
//        _MainTex ("Grass Texture", 2D) = "white" {}
//        _FlowerTex ("Flower Texture", 2D) = "white" {}
//        _FlowerDensity ("Flower Density", Range(1, 10)) = 6 // Further reduced max
//        _FlowerSize ("Flower Size", Range(0.01, 0.5)) = 0.1
//        _FlowerSizeVariation ("Size Variation", Range(0, 1)) = 0.3
//        _RandomSeed ("Random Seed", Float) = 1.0
//        _SwaySpeed ("Sway Speed", Range(0.1, 5.0)) = 1.0
//        _SwayAmount ("Sway Amount", Range(0.001, 0.05)) = 0.01
//        _FlowerRotate ("Flower Rotation (Degrees)", Range(0, 360)) = 0
//    }
//    SubShader
//    {
//        Tags
//        {
//            "RenderType"="Transparent"
//            "Queue"="Transparent"
//            "IgnoreProjector"="True"
//        }
//        LOD 100
//
//        Blend SrcAlpha OneMinusSrcAlpha
//
//        Pass
//        {
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #pragma multi_compile_fog
//            #pragma target 2.0
//            #pragma fragmentoption ARB_precision_hint_fastest
//
//            // Explicitly unroll the loop to avoid the Metal compiler error
//            #pragma shader_feature_local _MAX_FLOWERS_4 _MAX_FLOWERS_6 _MAX_FLOWERS_8 _MAX_FLOWERS_10
//            #define MAX_FLOWERS 6 // Default value, overridden by keywords
//
//            #include "UnityCG.cginc"
//
//            struct appdata
//            {
//                float4 vertex : POSITION;
//                float2 uv : TEXCOORD0;
//            };
//
//            struct v2f
//            {
//                float2 uv : TEXCOORD0;
//                float2 rawUV : TEXCOORD1;
//                float4 vertex : SV_POSITION;
//                float2 sinCos : TEXCOORD3;
//            };
//
//            sampler2D _MainTex;
//            float4 _MainTex_ST;
//            sampler2D _FlowerTex;
//            float _FlowerDensity;
//            float _FlowerSize;
//            float _FlowerSizeVariation;
//            float _RandomSeed;
//            float _SwaySpeed;
//            float _SwayAmount;
//            float _FlowerRotate;
//
//            // Optimized random function
//            float random(float2 st)
//            {
//                return frac(sin(dot(st, float2(12.9898, 78.233)) + _RandomSeed) * 43758.5453);
//            }
//
//            v2f vert(appdata v)
//            {
//                v2f o;
//                o.vertex = UnityObjectToClipPos(v.vertex);
//                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                UNITY_TRANSFER_FOG(o, o.vertex);
//
//                // Pre-calculate sin/cos of rotation angle in vertex shader
//                float angle = _FlowerRotate * 0.01745329;
//                o.sinCos = float2(sin(angle), cos(angle));
//                o.uv = TRANSFORM_TEX(v.uv, _MainTex); 
//                o.rawUV = v.uv;
//                return o;
//            }
//
//            fixed4 frag(v2f i) : SV_Target
//            {
//                // Sample the base grass texture
//                fixed4 finalColor = tex2D(_MainTex, i.uv);
//
//                // Time value for animation (simplified)
//                float time = _Time.y * _SwaySpeed;
//
//                // Fixed iteration count with density controlling actual visible flowers
//                int flowers = min(10, ceil(_FlowerDensity));
//
//                // Pre-compute commonly used values
//                float2 sinCos = i.sinCos;
//
//                // Manually unrolled loop for 10 possible flowers
//                // Only process as many as needed based on density
//                [unroll(10)]
//                for (int f = 0; f < 10; f++)
//                {
//                    // Skip if beyond the desired density
//                    if (f >= flowers) break;
//
//                    // Generate position hash with minimal calculations
//                    float2 hash = float2(f * 0.1 + 0.05, f * 0.1 + 0.07);
//                    float2 flowerPos = float2(
//                        random(hash),
//                        random(hash.yx + 10.0)
//                    );
//
//                    // Random size with minimal calculations
//                    float sizeRand = random(hash + 5.0);
//                    float flowerSize = _FlowerSize * (1.0 - _FlowerSizeVariation * sizeRand);
//
//                    // Distance check (squared for optimization)
//                   // float2 delta = i.rawUV - flowerPos;
//                        float2 uvNormalized = frac(i.uv);
//                    float2 delta = uvNormalized - flowerPos;
//                    float distSq = dot(delta, delta);
//                    float sizeSq = flowerSize * flowerSize;
//
//                    // Skip if pixel is outside flower bounds (using squared distance)
//                    if (distSq >= sizeSq) continue;
//
//                    // Simple phase offset
//                    float phaseOffset = random(flowerPos) * 6.283;
//
//                    // Simplified sway calculation
//                    float sway = sin(time + phaseOffset) * _SwayAmount;
//                    float verticalFactor = saturate((delta.y / flowerSize) + 0.5);
//                    delta.x += sway * verticalFactor;
//
//                    // Apply rotation using pre-calculated values
//                    float2 rotated = float2(
//                        delta.x * sinCos.y - delta.y * sinCos.x,
//                        delta.x * sinCos.x + delta.y * sinCos.y
//                    );
//                
//
//                    
//                    // Convert to UV space
//                    float2 flowerUV = (rotated / flowerSize) * 0.5 + 0.5;
//
//                    // Skip out-of-bounds UVs
//                    if (flowerUV.x < 0 || flowerUV.x > 1 || flowerUV.y < 0 || flowerUV.y > 1)
//                        continue;
//
//                    // Sample flower texture
//                    fixed4 flowerColor = tex2D(_FlowerTex, flowerUV);
//
//                    // Only blend if alpha is significant (avoid branching)
//                    float alphaThreshold = step(0.1, flowerColor.a);
//                    finalColor = lerp(finalColor, flowerColor, flowerColor.a * alphaThreshold);
//                }
//
//                UNITY_APPLY_FOG(i.fogCoord, finalColor);
//                return finalColor;
//            }
//            ENDCG
//        }
//    }
//    Fallback "Mobile/Unlit (Supports Lightmap)"
//
//    CustomEditor "StyleGrassShaderGUI" // Optional: for setting flower count via keywords
//}
Shader "Mobile/StyleGrass"
{
    Properties
    {
        _MainTex ("Grass Texture", 2D) = "white" {}
        _FlowerTex ("Flower Texture", 2D) = "white" {}
        _FlowerDensity ("Flower Density", Range(1, 10)) = 6
        _FlowerSize ("Flower Size", Range(0.01, 0.5)) = 0.1
        _FlowerSizeVariation ("Size Variation", Range(0, 1)) = 0.3
        _RandomSeed ("Random Seed", Float) = 1.0
        _SwaySpeed ("Sway Speed", Range(0.1, 5.0)) = 1.0
        _SwayAmount ("Sway Amount", Range(0.001, 0.05)) = 0.01
        _FlowerRotate ("Flower Rotation (Degrees)", Range(0, 360)) = 0
        _FlowerScale ("Flower Distribution Scale", Range(0.1, 10.0)) = 1.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 2.0
            #pragma fragmentoption ARB_precision_hint_fastest

            #pragma shader_feature_local _MAX_FLOWERS_4 _MAX_FLOWERS_6 _MAX_FLOWERS_8 _MAX_FLOWERS_10
            #define MAX_FLOWERS 6

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float2 sinCos : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _FlowerTex;
            float _FlowerDensity;
            float _FlowerSize;
            float _FlowerSizeVariation;
            float _RandomSeed;
            float _SwaySpeed;
            float _SwayAmount;
            float _FlowerRotate;
            float _FlowerScale;

            float random(float2 st)
            {
                return frac(sin(dot(st, float2(12.9898, 78.233)) + _RandomSeed) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                float4 worldPosition = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPosition.xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);

                float angle = _FlowerRotate * 0.01745329;
                o.sinCos = float2(sin(angle), cos(angle));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the base grass texture with tiling
                fixed4 finalColor = tex2D(_MainTex, i.uv);

                // Time value for animation
                float time = _Time.y * _SwaySpeed;

                // Fixed iteration count with density controlling actual visible flowers
                int flowers = min(10, ceil(_FlowerDensity));
                
                // Scale the world position for flower distribution
                float2 scaledPos = i.worldPos.xz * _FlowerScale;
                
                // Pre-compute commonly used values
                float2 sinCos = i.sinCos;

                // Manually unrolled loop for flowers
                [unroll(10)]
                for (int f = 0; f < 10; f++)
                {
                    if (f >= flowers) break;

                    // Generate position hash
                    float2 hash = float2(f * 0.1 + 0.05, f * 0.1 + 0.07);
                    
                    // Generate cell position based on world coordinates
                    float2 cellID = floor(scaledPos + hash);
                    float2 cellUV = frac(scaledPos + hash);
                    float2 flowerPos = float2(
                        random(cellID),
                        random(cellID.yx + 10.0)
                    );
                    
                    // Random size
                    float sizeRand = random(cellID + 5.0);
                    float flowerSize = _FlowerSize * (1.0 - _FlowerSizeVariation * sizeRand);

                    // Distance check (squared for optimization)
                    float2 delta = cellUV - flowerPos;
                    float distSq = dot(delta, delta);
                    float sizeSq = flowerSize * flowerSize;

                    // Skip if pixel is outside flower bounds
                    if (distSq >= sizeSq) continue;

                    // Simple phase offset
                    float phaseOffset = random(flowerPos + cellID) * 6.283;

                    // Simplified sway calculation
                    float sway = sin(time + phaseOffset) * _SwayAmount;
                    float verticalFactor = saturate((delta.y / flowerSize) + 0.5);
                    delta.x += sway * verticalFactor;

                    // Apply rotation using pre-calculated values
                    float2 rotated = float2(
                        delta.x * sinCos.y - delta.y * sinCos.x,
                        delta.x * sinCos.x + delta.y * sinCos.y
                    );
                    
                    // Convert to UV space
                    float2 flowerUV = (rotated / flowerSize) * 0.5 + 0.5;

                    // Skip out-of-bounds UVs
                    if (flowerUV.x < 0 || flowerUV.x > 1 || flowerUV.y < 0 || flowerUV.y > 1)
                        continue;

                    // Sample flower texture
                    fixed4 flowerColor = tex2D(_FlowerTex, flowerUV);

                    // Only blend if alpha is significant (avoid branching)
                    float alphaThreshold = step(0.1, flowerColor.a);
                    finalColor = lerp(finalColor, flowerColor, flowerColor.a * alphaThreshold);
                }

                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
    Fallback "Mobile/Unlit (Supports Lightmap)"

    CustomEditor "StyleGrassShaderGUI"
}