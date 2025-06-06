Shader "Custom/GridSystemUnlit"
{
    Properties
    {
        _Color ("Main Color", Color) = (0.1, 0.1, 0.1, 0.6)
        _GridColor ("Grid Color", Color) = (1, 1, 1, 1)
        _GridSize ("Grid Size", Float) = 1.0
        _LineThickness ("Line Thickness", Range(0.0, 0.1)) = 0.02
        _OccupiedColor ("Occupied Cell Color", Color) = (0.9, 0.2, 0.2, 0.7)
        _UnoccupiedColor ("Unoccupied Cell Color", Color) = (0.2, 0.9, 0.2, 0.7)
        _HighlightIntensity ("Highlight Intensity", Range(0, 1)) = 0.5
        _GridOriginX ("Grid Origin X", Float) = 0
        _GridOriginZ ("Grid Origin Z", Float) = 0
        _GridWidth ("Grid Width", Float) = 50
        _GridHeight ("Grid Height", Float) = 50
        _UseOccupancyData ("Use Occupancy Data", Range(0, 1)) = 0
        _ShowGrid ("Show Grid", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };

            fixed4 _Color;
            fixed4 _GridColor;
            fixed4 _OccupiedColor;
            fixed4 _UnoccupiedColor;
            float _GridSize;
            float _LineThickness;
            float _HighlightIntensity;
            float _GridOriginX;
            float _GridOriginZ;
            float _GridWidth;
            float _GridHeight;
            float _UseOccupancyData;
            float _ShowGrid;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // If ShowGrid is 0, make the fragment completely transparent
                if (_ShowGrid < 0.5)
                {
                    return _Color;
                }

                // Adjust world position relative to grid origin
                float3 adjustedPos = float3(i.worldPos.x - _GridOriginX, i.worldPos.y, i.worldPos.z - _GridOriginZ);

                // Calculate grid position
                float2 gridPos = floor(adjustedPos.xz / _GridSize);

                // Check if we're inside grid bounds
                bool insideGrid = gridPos.x >= 0 && gridPos.x < _GridWidth &&
                                  gridPos.y >= 0 && gridPos.y < _GridHeight;

                // Calculate cell center and fractional position within cell
                float2 cellCenter = (gridPos + 0.5) * _GridSize;
                float2 localPos = frac(adjustedPos.xz / _GridSize);

                // Create grid lines
                float2 gridLines = step(localPos, float2(_LineThickness, _LineThickness)) +
                                  step(1.0 - _LineThickness, localPos);

                // Final color setup
                fixed4 finalColor = _Color;
                float alpha = _Color.a;

                // If on grid line and inside grid bounds
                if (insideGrid && (gridLines.x > 0 || gridLines.y > 0))
                {
                    finalColor = _GridColor;
                    alpha = _GridColor.a;
                }
                // If using occupancy visualization
                else if (_UseOccupancyData > 0.5 && insideGrid)
                {
                    // Simple checkerboard pattern to demonstrate cell coloring
                    bool isOdd = (int(gridPos.x) + int(gridPos.y)) % 2 == 1;

                    if (isOdd)
                    {
                        finalColor = lerp(_Color, _OccupiedColor, _HighlightIntensity);
                        alpha = lerp(_Color.a, _OccupiedColor.a, _HighlightIntensity);
                    }
                    else
                    {
                        finalColor = lerp(_Color, _UnoccupiedColor, _HighlightIntensity);
                        alpha = lerp(_Color.a, _UnoccupiedColor.a, _HighlightIntensity);
                    }
                }

                // Apply fog
                fixed4 col = fixed4(finalColor.rgb, alpha);
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}