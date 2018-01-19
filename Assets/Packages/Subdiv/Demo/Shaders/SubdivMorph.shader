Shader "Subdiv/SubdivMorph" {

	Properties {
        _InColor ("Color", Color) = (1, 1, 1, 1)
        _OutColor ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Interpolation ("Interpolation", Range(0.0, 1.0)) = 0.0

        [Space]
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        [Gamma] _Metallic("Metallic", Range(0, 1)) = 0
	}

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Tags { "LightMode"="Deferred" }
            CGPROGRAM
            #pragma target 4.0
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap
            #include "SubdivMorph.cginc"
            ENDCG
        }

        Pass
        {
            Tags { "LightMode"="ShadowCaster" }
            CGPROGRAM
            #pragma target 4.0
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_shadowcaster noshadowmask nodynlightmap nodirlightmap nolightmap
            #define UNITY_PASS_SHADOWCASTER
            #include "SubdivMorph.cginc"
            ENDCG
        }
    }


}
