// Shader created with Shader Forge v1.29 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.29;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0.4632353,fgcg:0.4632353,fgcb:0.4632353,fgca:1,fgde:0.05,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True;n:type:ShaderForge.SFN_Final,id:4795,x:33636,y:32463,varname:node_4795,prsc:2|emission-3636-OUT;n:type:ShaderForge.SFN_Multiply,id:2393,x:33173,y:32522,varname:node_2393,prsc:2|A-2430-OUT,B-3929-RGB;n:type:ShaderForge.SFN_TexCoord,id:3455,x:31443,y:32483,varname:node_3455,prsc:2,uv:0;n:type:ShaderForge.SFN_RemapRange,id:1912,x:31871,y:32508,varname:node_1912,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-8208-OUT;n:type:ShaderForge.SFN_ComponentMask,id:935,x:32040,y:32508,varname:node_935,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-1912-OUT;n:type:ShaderForge.SFN_OneMinus,id:8021,x:32392,y:32508,varname:node_8021,prsc:2|IN-4801-OUT;n:type:ShaderForge.SFN_Power,id:7927,x:32770,y:32512,varname:node_7927,prsc:2|VAL-1669-OUT,EXP-9062-OUT;n:type:ShaderForge.SFN_Clamp01,id:2430,x:32961,y:32522,varname:node_2430,prsc:2|IN-7927-OUT;n:type:ShaderForge.SFN_Slider,id:9062,x:32380,y:32666,ptovrint:False,ptlb:WidthPower,ptin:_WidthPower,varname:node_9062,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:3.148908,max:10;n:type:ShaderForge.SFN_Color,id:3929,x:32961,y:32704,ptovrint:False,ptlb:Colour,ptin:_Colour,varname:node_3929,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0.5586207,c3:0,c4:0;n:type:ShaderForge.SFN_ComponentMask,id:8208,x:31680,y:32508,varname:node_8208,prsc:2,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-3455-UVOUT;n:type:ShaderForge.SFN_Slider,id:4544,x:32961,y:32894,ptovrint:False,ptlb:Intensity,ptin:_Intensity,varname:node_4544,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Multiply,id:3636,x:33361,y:32657,varname:node_3636,prsc:2|A-2393-OUT,B-4544-OUT;n:type:ShaderForge.SFN_Multiply,id:4801,x:32212,y:32508,varname:node_4801,prsc:2|A-935-OUT,B-935-OUT;n:type:ShaderForge.SFN_Clamp01,id:1669,x:32583,y:32512,varname:node_1669,prsc:2|IN-8021-OUT;proporder:9062-3929-4544;pass:END;sub:END;*/

Shader "Shader Forge/Beam" {
    Properties {
        _WidthPower ("WidthPower", Range(0, 10)) = 3.148908
        [HDR]_Colour ("Colour", Color) = (1,0.5586207,0,0)
        _Intensity ("Intensity", Range(0, 1)) = 1
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float _WidthPower;
            uniform float4 _Colour;
            uniform float _Intensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float node_935 = (i.uv0.g*2.0+-1.0).r;
                float3 emissive = ((saturate(pow(saturate((1.0 - (node_935*node_935))),_WidthPower))*_Colour.rgb)*_Intensity);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0.4632353,0.4632353,0.4632353,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
