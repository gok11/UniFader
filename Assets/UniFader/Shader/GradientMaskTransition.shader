Shader "UI/GradientMaskTransition"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		
		_GradientMaskTex ("Gradient Mask Texture", 2D) = "white" {}
		[KeywordEnum(Fade, Cutout)] _FadeMode("Fade Mode", Int) = 0
		[HideInInspector] _RegularDirection ("Use Regular Direction", Float) = 1
		_CutoutEdgeFactor("Cutout Edge Factor", Range(0, 0.1)) = 0.02
        _AlphaRate ("Alpha Rate", Range(0, 1)) = 0
	}
 
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		
		Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
 
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Fog{ Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]
 
		Pass
		{
    		Name "GradientMaskTransition"
		CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma multi_compile _FADEMODE_FADE _FADEMODE_CUTOUT
 
			struct appdata_t
			{
				float4 vertex   : POSITION;
                float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
 
			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;				
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};
 
            sampler2D _MainTex;
            sampler2D _GradientMaskTex;
			fixed _AlphaRate;
			fixed _CutoutEdgeFactor;
			fixed _RegularDirection;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _MainTex_ST;
 
			v2f vert(appdata_t v)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                
				OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				
                OUT.color = v.color;
				return OUT;
			}
 
			fixed4 frag(v2f IN) : SV_Target
			{
				half alpha = tex2D(_GradientMaskTex, IN.texcoord).a;
				half edge = 0.5;
				
                #ifdef _FADEMODE_FADE
                
                half rate = lerp(_AlphaRate, 1 - _AlphaRate, step(edge, _RegularDirection));
                rate = rate * 2 - 1;
				
				alpha = lerp(alpha + rate, 1 - alpha - rate, step(edge, _RegularDirection));
				alpha = saturate(alpha);

				#elif _FADEMODE_CUTOUT
								
				half rate = lerp(1 - _AlphaRate, _AlphaRate, step(edge, _RegularDirection));
				half directionalizedAlpha = lerp(smoothstep(0.001, 0.001 + _CutoutEdgeFactor, alpha - rate),
				                                smoothstep(alpha - rate, alpha - rate + _CutoutEdgeFactor, 0.001),
				                                step(edge, _RegularDirection));
				                                
                alpha = lerp (lerp (1, directionalizedAlpha, abs (sign(1 - _AlphaRate))),
                        lerp (0, directionalizedAlpha, abs (sign(_AlphaRate))),
                        step(edge, _RegularDirection));
				
				#endif
				
                fixed4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;                
                color.a = alpha;
                
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
			}
			ENDCG
		}
	}
}