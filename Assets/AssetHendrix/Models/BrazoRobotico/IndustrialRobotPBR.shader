Shader "AssetHendrix/Industrial Robot PBR"
{
 Properties {
 [MainTexture] _BaseMap("Base Color",2D)="white"{}
 [MainColor] _BaseColor("Tint",Color)=(1,1,1,1)
 [Normal] _BumpMap("Normal Map",2D)="bump"{}
 _BumpScale("Normal Strength",Range(0,2))=1
 _MetallicGlossMap("Metallic (R) / Smoothness (A)",2D)="white"{}
 _Metallic("Metallic",Range(0,1))=0.8
 _Smoothness("Smoothness",Range(0,1))=0.65
 [HideInInspector] _Cull("Cull",Float)=2
 [HideInInspector] _Cutoff("Cutoff",Float)=0.5
 }
 SubShader {
 Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
 Pass {
 Name "ForwardLit"
 Tags{"LightMode"="UniversalForwardOnly"}
 Cull Back ZWrite On
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #pragma target 3.5
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
 #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
 #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 TEXTURE2D(_BumpMap);SAMPLER(sampler_BumpMap);
 TEXTURE2D(_MetallicGlossMap);SAMPLER(sampler_MetallicGlossMap);
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseMap_ST;half4 _BaseColor;half _Metallic;half _Smoothness;half _BumpScale;
 CBUFFER_END
 struct Attributes{float4 positionOS:POSITION;float3 normalOS:NORMAL;float4 tangentOS:TANGENT;float2 uv:TEXCOORD0;};
 struct Varyings{float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;half4 tangentWS:TEXCOORD2;float2 uv:TEXCOORD3;half fog:TEXCOORD4;};
 Varyings Vert(Attributes v){
 Varyings o=(Varyings)0;VertexPositionInputs p=GetVertexPositionInputs(v.positionOS.xyz);VertexNormalInputs n=GetVertexNormalInputs(v.normalOS,v.tangentOS);o.positionCS=p.positionCS;o.positionWS=p.positionWS;o.normalWS=n.normalWS;o.tangentWS=half4(n.tangentWS,v.tangentOS.w*GetOddNegativeScale());o.uv=TRANSFORM_TEX(v.uv,_BaseMap);o.fog=ComputeFogFactor(p.positionCS.z);return o;
 }
 half4 Frag(Varyings i):SV_Target {
 half4 base=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv)*_BaseColor;
 half4 metal=SAMPLE_TEXTURE2D(_MetallicGlossMap,sampler_MetallicGlossMap,i.uv);
 half3 normalTS=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,i.uv),_BumpScale);
 half3 n=normalize(i.normalWS);half3 t=normalize(i.tangentWS.xyz);half3 b=cross(n,t)*i.tangentWS.w;
 InputData input=(InputData)0;input.positionWS=i.positionWS;input.positionCS=i.positionCS;input.normalWS=NormalizeNormalPerPixel(TransformTangentToWorld(normalTS,half3x3(t,b,n)));input.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);input.shadowCoord=TransformWorldToShadowCoord(i.positionWS);input.bakedGI=SampleSH(input.normalWS);input.vertexLighting=VertexLighting(i.positionWS,input.normalWS);input.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);input.shadowMask=half4(1,1,1,1);
 SurfaceData surface=(SurfaceData)0;surface.albedo=base.rgb;surface.alpha=1;surface.metallic=saturate(metal.r*_Metallic);surface.smoothness=saturate(metal.a*_Smoothness);surface.normalTS=normalTS;surface.occlusion=1;
 half4 color=UniversalFragmentPBR(input,surface);color.rgb=MixFog(color.rgb,i.fog);return color;
 }
 ENDHLSL
 }
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
 FallBack "Hidden/Universal Render Pipeline/FallbackError"
}