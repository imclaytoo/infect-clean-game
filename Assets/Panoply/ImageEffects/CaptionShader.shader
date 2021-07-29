// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CaptionShader"
{
    Properties
    {
        _MainTex ("Albedo Texture", 2D) = "white" {}
        _TintColor("Tint Color", Color) = (1,1,1,1)
        _BackgroundColor("Background Color", Color) = (0,0,0,1)
        
        _BorderColor ("Border Color", Color) = (1,0,0,0)
        _BorderTop ("Top Border", Float) = 10
        _BorderRight ("Right Border", Float) = 10
        _BorderBottom ("Bottom Border", Float) = 10
        _BorderLeft ("Left Border", Float) = 10
        _OffsetLeft ("Left Offset", Float) = 0
        _OffsetTop ("Top Offset", Float) = 0
        _OffsetBottom ("Bottom Offset", Float) = 0
        _OffsetTopInverted ("Top Offset Inverted", Float) = 0
        _OffsetBottomInverted ("Bottom Offset Inverted", Float) = 0
    }

    SubShader
    {
        LOD 100

        ZWrite Off
        Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _TintColor;
            float4 _BackgroundColor;
            float _Blend;
            
            uniform float4 _MainTex_TexelSize;
            float4 _BorderColor;
            float _BorderTop;
            float _BorderRight;
            float _BorderBottom;
            float _BorderLeft;
            float _OffsetLeft;
            float _OffsetTop;
            float _OffsetBottom;
            float _OffsetTopInverted;
            float _OffsetBottomInverted;
            
            float _ScreenLeft;
            float _ScreenRight;
            float _ScreenTop;
            float _ScreenBottom;
            float _ScreenTopInverted;
            float _ScreenBottomInverted;
            
            v2f_img vert( appdata v )
            {
                v2f_img o;
                o.pos = UnityObjectToClipPos (v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f_img i) : SV_Target
            {
                fixed4 originalColor = tex2D(_MainTex, i.uv);
                
                // add the matte
                fixed4 matteBlendedColor = lerp(originalColor, _TintColor, _TintColor[3]);
                matteBlendedColor[3] = 1;
                
                // add the border
                fixed4 borderBlendedColor = lerp(matteBlendedColor, _BorderColor, _BorderColor[3]);
                borderBlendedColor[3] = 1;
                #if UNITY_UV_STARTS_AT_TOP
                borderBlendedColor = (i.pos.x < (_OffsetLeft + _BorderLeft) || i.pos.x > (_OffsetLeft + _MainTex_TexelSize.z - _BorderRight) || i.pos.y < (_OffsetTopInverted + _BorderTop) || i.pos.y > (_OffsetBottomInverted - _BorderBottom)) ? borderBlendedColor : matteBlendedColor;
                #else
                borderBlendedColor = (i.pos.x < (_OffsetLeft + _BorderLeft) || i.pos.x > (_OffsetLeft + _MainTex_TexelSize.z - _BorderRight) || i.pos.y > (_OffsetTop - _BorderTop) || i.pos.y < (_OffsetBottom + _BorderBottom)) ? borderBlendedColor : matteBlendedColor;
                #endif
                
                // letter/pillarboxing
                #if UNITY_UV_STARTS_AT_TOP
                return i.pos.x < _ScreenLeft || i.pos.x > _ScreenRight || i.pos.y < _ScreenTopInverted || i.pos.y > _ScreenBottomInverted ? _BackgroundColor : borderBlendedColor;
                #else
                return i.pos.x < _ScreenLeft || i.pos.x > _ScreenRight || i.pos.y > _ScreenTop || i.pos.y < _ScreenBottom ? _BackgroundColor : borderBlendedColor;
                #endif
            }
            ENDCG
        }
    }
}