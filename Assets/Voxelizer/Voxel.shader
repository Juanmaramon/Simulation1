Shader "Custom/Voxel" {
	Properties {
		[HDR] _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
        [HideInInspector] _ModTex("", 2D) = "black"{}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert fullforwardshadows addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

        #include "SimplexNoise2D.cginc"

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
        sampler2D _ModTex;
        half _Threshold;
        half2 _Extent;
        half _ZMove;
        half3 _NoiseAmp;    // (position, rotation, scale)
        half2 _NoiseParams; // (frequency, speed)

        // Quaternion multiplication
        // http://mathworld.wolfram.com/Quaternion.html
        float4 QMul(float4 q1, float4 q2)
        {
            return float4(
                q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
                q1.w * q2.w - dot(q1.xyz, q2.xyz)
            );
        }

        // Vector rotation with a quaternion
        // http://mathworld.wolfram.com/Quaternion.html
        float3 RotateVector(float3 v, float4 r)
        {
            float4 r_c = r * float4(-1, -1, -1, 1);
            return QMul(r, QMul(float4(v, 0), r_c)).xyz;
        }

        // Vertex modifier function
        void ModifyVertex(inout float3 position, inout float3 normal, float2 uv)
        {
            // Modifier amount
            half amount = tex2Dlod(_ModTex, float4(uv, 0, 0)).r;
            amount = saturate((amount - _Threshold) / (1 - _Threshold));

            // Reference point in the noise field
            /*half2 noise_pos = uv * _NoiseParams.x;
            noise_pos.y = noise_pos.y * _Extent.y / _Extent.x + _NoiseParams.y * _Time.y;

            // (noise grad x, noise grad y, noise value)
            half3 nfield = snoise(noise_pos);

            // Displacement
            half3 disp = half3(_Extent * (uv - 0.5f), _ZMove * amount);
            disp.xy += nfield.xy * (_NoiseAmp.x * (1 - amount));

            // Rotation
            float3 raxis = normalize(float3(nfield.xy, 0*nfield.z));
            float4 rot = RotationAngleAxis(_NoiseAmp.y * (1 - amount), raxis);

            // Scaling
            float scale = _Scale * amount * (1 + _NoiseAmp.z * nfield.z);*/

            // Apply modification
            //position = RotateVector(position, rot) * scale + disp;
            //normal = RotateVector(normal, rot);

            // Reference point in the noise field
            half2 noise_pos = uv * _NoiseParams.x;
            noise_pos.y = noise_pos.y * _Extent.y / _Extent.x + _NoiseParams.y * _Time.y;

            // (noise grad x, noise grad y, noise value)
            half3 nfield = snoise(noise_pos);

            half3 disp = half3(_Extent * (uv - 0.5f), _ZMove * amount);
            disp.xy += nfield.xy * (_NoiseAmp.x * (1 - amount));

            position *= amount + disp;
        }

        void vert(inout appdata_full v)
        {
            ModifyVertex(v.vertex.xyz, v.normal, v.texcoord1.xy);
        }

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = _Color.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
