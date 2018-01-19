
#include "UnityCG.cginc"
#include "UnityGBuffer.cginc"
#include "UnityStandardUtils.cginc"

#include "Easing.hlsl"
#include "Noise/SimplexNoise3D.cginc"

#ifndef PI
#define PI 3.14159265359
#endif

#ifndef HALF_PI
#define HALF_PI 1.57079632679
#endif

#if defined(SHADOWS_CUBE) && !defined(SHADOWS_CUBE_IN_DEPTH_TEX)
#define PASS_CUBE_SHADOWCASTER
#endif

half4 _InColor, _OutColor;
sampler2D _MainTex;
float4 _MainTex_ST;

half _Glossiness;
half _Metallic;

float4 _NoiseParams;
fixed _Interpolation;

struct Attributes {
    float4 position : POSITION;
    float3 normal : NORMAL;
    float2 texcoord : TEXCOORD0;
};

// Fragment varyings
struct Varyings {
    float4 position : POSITION;

#if defined(PASS_CUBE_SHADOWCASTER)
    // Cube map shadow caster pass
    float3 shadow : TEXCOORD0;

#elif defined(UNITY_PASS_SHADOWCASTER)
    // Default shadow caster pass

#else
    // GBuffer construction pass
    float3 normal : NORMAL;
    float2 texcoord : TEXCOORD0;
    half3 ambient : TEXCOORD1;
    float3 wpos : TEXCOORD2;
    float3 color : COLOR;
#endif
};

struct MVertex {
    float3 position;
    float3 normal;
};

StructuredBuffer<MVertex> _VertexBuffer;

//
// Vertex stage
//

Varyings Vertex(in Attributes IN, uint vid : SV_VertexID)
{
    Varyings o;

    // float t = (sin(_Time.y) + 1.0) * 0.5;
    MVertex v = _VertexBuffer[vid];

    float3 normal = IN.normal;

    float t = (snoise(v.position.xyz * _NoiseParams.x + float3(0, _NoiseParams.w, 0)) + 1.0) * 0.5;
    t = saturate(t);
    t = lerp(step(0.5, t), ease_in_out_expo(t), _Interpolation);
    float3 pos = lerp(v.position, IN.position.xyz * _NoiseParams.y, t);

    float3 wnrm = UnityObjectToWorldNormal(normal);
    float3 wpos = mul(unity_ObjectToWorld, float4(pos, 1)).xyz;

#if defined(PASS_CUBE_SHADOWCASTER)
    // Cube map shadow caster pass: Transfer the shadow vector.
    o.position = UnityObjectToClipPos(float4(pos.xyz, 1));
    o.shadow = wpos - _LightPositionRange.xyz;

#elif defined(UNITY_PASS_SHADOWCASTER)
    // Default shadow caster pass: Apply the shadow bias.
    float scos = dot(wnrm, normalize(UnityWorldSpaceLightDir(wpos)));
    wpos -= wnrm * unity_LightShadowBias.z * sqrt(1 - scos * scos);
    o.position = UnityApplyLinearShadowBias(UnityWorldToClipPos(float4(wpos, 1)));

#else
    // GBuffer construction pass
    o.position = UnityWorldToClipPos(float4(wpos, 1));
    o.normal = wnrm;
    o.texcoord = IN.texcoord;
    o.ambient = ShadeSHPerVertex(wnrm, 0);
    o.wpos = wpos;
    o.color = lerp(_InColor.rgb, _OutColor.rgb, t);
#endif

    return o;
}

//
// Fragment phase
//

#if defined(PASS_CUBE_SHADOWCASTER)

// Cube map shadow caster pass
half4 Fragment(Varyings input) : SV_Target
{
    float depth = length(input.shadow) + unity_LightShadowBias.x;
    return UnityEncodeCubeShadowDepth(depth * _LightPositionRange.w);
}

#elif defined(UNITY_PASS_SHADOWCASTER)

// Default shadow caster pass
half4 Fragment() : SV_Target { return 0; }

#else

// GBuffer construction pass
void Fragment (Varyings input, out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3) {
    // Sample textures
    // half3 albedo = tex2D(_MainTex, input.texcoord).rgb * _Color.rgb;
    half3 albedo = tex2D(_MainTex, input.texcoord).rgb * input.color.rgb;

    // PBS workflow conversion (metallic -> specular)
    half3 c_diff, c_spec;
    half refl10;
    c_diff = DiffuseAndSpecularFromMetallic(
        albedo, _Metallic, // input
        c_spec, refl10 // output
    );

    float3 nx = ddx(input.wpos);
    float3 ny = ddy(input.wpos);
    float3 normal = -normalize(cross(nx, ny));

    // Update the GBuffer.
    UnityStandardData data;
    data.diffuseColor = c_diff;
    data.occlusion = 1.0;
    data.specularColor = c_spec;
    data.smoothness = _Glossiness;
    // data.normalWorld = input.normal;
    data.normalWorld = normal;
    UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

    // Calculate ambient lighting and output to the emission buffer.
    half3 sh = ShadeSHPerPixel(data.normalWorld, input.ambient, input.wpos);
    outEmission = half4(sh * c_diff, 1);
}

#endif
