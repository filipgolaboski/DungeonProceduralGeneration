Shader "Unlit/TesselationUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DispTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma target 4.6
            #pragma vertex VertexProgram
            #pragma hull HullProgram
            #pragma domain DomainProgram
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "TesselationBase.cginc"

            struct Varyings{
                float4 vertex: POSITION;
                float3 normal: NORMAL;
                float4 tangent: TANGENT;
                float2 uv: TEXCOORD0;
                float2 uv1:TEXCOORD1;
                float4 uv2:TEXCOORD2;
                UNITY_FOG_COORDS(1)
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD2;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _DispTex;
            float4 _DispTex_ST;

            TessControlPoint VertexProgram(VertexData v)
            {
                TessControlPoint t;
                t.vertex = v.vertex;
                t.normal = v.normal;
                t.tangent = v.tangent;
                t.uv = v.uv;
                t.uv2 = v.vertex;
                t.uv1 = v.uv1;
                return t;
            }


            Varyings vert (TessControlPoint v)
            {
                Varyings o;
                v.normal = normalize(v.normal);
                float4 wps = mul(unity_ObjectToWorld, v.vertex);
                float3 triW = abs(v.normal);
                triW /=(triW.x+triW.y+triW.z);
                float dx = tex2Dlod(_DispTex,float4(v.vertex.xy,0,0)).b;
                float dy = tex2Dlod(_DispTex,float4(v.vertex.xz,0,0)).b;
                float dz = tex2Dlod(_DispTex,float4(v.vertex.zy,0,0)).b;
                float2 uvP = ((wps.zy*triW.x+wps.xz*triW.y+wps.xy*triW.z)/3);
                float displacement = (dx+dy+dz)/3;
                displacement = (displacement-0.5)*2;
                v.vertex.xyz += displacement*v.normal;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uv2 = v.uv2;
                o.normal =normalize(v.normal);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            [UNITY_domain("tri")]
            Varyings DomainProgram(TessFactors factors, OutputPatch<TessControlPoint,3> patch, float3 barycentricCoords: SV_DomainLocation)
            {
            TessControlPoint data;
                #define DOMAIN_INTERPOLATE(fieldName) data.fieldName = \
                patch[0].fieldName*barycentricCoords.x + \
                patch[1].fieldName*barycentricCoords.y + \
                patch[2].fieldName*barycentricCoords.z;

                DOMAIN_INTERPOLATE(vertex);
                DOMAIN_INTERPOLATE(normal);
                DOMAIN_INTERPOLATE(tangent);
                DOMAIN_INTERPOLATE(uv);
                DOMAIN_INTERPOLATE(uv1);
                DOMAIN_INTERPOLATE(uv2);
                return vert(data);
            }


            fixed4 frag (Varyings i) : SV_Target
            {

                float3 triW = abs(i.normal);
                triW /=(triW.x+triW.y+triW.z);
                float2 dx = i.uv2.xy*triW.x;
                float2 dy = i.uv2.xz*triW.y;
                float2 dz = i.uv2.zy*triW.z;
                float2 uvP = ((dx + dy + dz)/3);

                // sample the texture
                fixed4 col = (tex2D(_MainTex, i.uv2.zy)*triW.x+tex2D(_MainTex, i.uv2.xz)*triW.y+tex2D(_MainTex, i.uv2.xy)*triW.z)/3;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
