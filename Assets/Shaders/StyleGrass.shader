
Shader "Mobile/StyleGrass"
{
    Properties
    {
        _MainTex("Grass Texture", 2D) = "white" {}
        _FlowerTex("Flower Texture", 2D) = "white" {}
        _FlowerDensity("Flower Density", Range(1, 100)) = 6
        _FlowerSize("Flower Size", Range(0.01, 0.5)) = 0.1
        _FlowerSizeVariation("Size Variation", Range(0, 1)) = 0.3
        _RandomSeed("Random Seed", Float) = 1.0
        _SwaySpeed("Sway Speed", Range(0.1, 5.0)) = 1.0
        _SwayAmount("Sway Amount", Range(0.001, 0.05)) = 0.01
        _FlowerRotate("Flower Rotation", Range(0, 360)) = 0
        _FlowerScale("Flower Distribution Scale", Range(0.1, 10.0)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 2.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _FlowerTex;
            float _FlowerDensity, _FlowerSize, _FlowerSizeVariation;
            float _RandomSeed, _SwaySpeed, _SwayAmount;
            float _FlowerRotate, _FlowerScale;

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
                float2 sinCos : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7)) + _RandomSeed) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float angle = radians(_FlowerRotate);
                o.sinCos = float2(sin(angle), cos(angle));
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 finalColor = tex2D(_MainTex, i.uv);
                float time = _Time.y * _SwaySpeed;

                int count = (int)_FlowerDensity;
                float2 scaledPos = i.worldPos.xz * _FlowerScale;
                float2 sinCos = i.sinCos;

                [unroll(6)] // Limit to max 6 flowers for performance
                for (int f = 0; f < 6; f++)
                {
                    if (f >= count) break;

                    float2 hashOffset = float2(f * 0.123, f * 0.456);
                    float2 cell = floor(scaledPos + hashOffset);
                    float2 cellUV = frac(scaledPos + hashOffset);

                    float2 randPos = float2(hash(cell), hash(cell.yx + 10.0));
                    float2 delta = cellUV - randPos;

                    float sizeRnd = hash(cell + 3.14);
                    float size = _FlowerSize * (1.0 - _FlowerSizeVariation * sizeRnd);
                    float distSq = dot(delta, delta);
                    float sizeSq = size * size;
                    if (distSq >= sizeSq) continue;

                    float sway = sin(time + hash(randPos + cell) * 6.283) * _SwayAmount;
                    delta.x += sway * clamp((delta.y / size) + 0.5, 0.0, 1.0);

                    float2 rotated = float2(
                        delta.x * sinCos.y - delta.y * sinCos.x,
                        delta.x * sinCos.x + delta.y * sinCos.y
                    );

                    float2 flowerUV = rotated / size * 0.5 + 0.5;
                    if (any(flowerUV < 0.0) || any(flowerUV > 1.0)) continue;

                    fixed4 flowerCol = tex2D(_FlowerTex, flowerUV);
                    float a = step(0.1, flowerCol.a);
                    finalColor = lerp(finalColor, flowerCol, flowerCol.a * a);
                }

                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
    Fallback "Mobile/Unlit (Supports Lightmap)"
}
