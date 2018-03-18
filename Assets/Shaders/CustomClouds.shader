Shader "FX/CustomClouds" {
	Properties{
		_Color("Tint", Color) = (1,1,1,1)
		_Noise("Noise (RGB)", 2D) = "gray" {}
	_TColor("Cloud Top Color", Color) = (1,0.6,1,1)
		_CloudColor("Cloud Base Color", Color) = (0.6,1,1,1)
		_RimColor("Rim Color", Color) = (0.6,1,1,1)
		_RimPower("Rim Power", Range(0,40)) = 20
		_Scale("World Scale", Range(0,0.1)) = 0.004
		_AnimSpeedX("Animation Speed X", Range(-2,2)) = 1
		_AnimSpeedY("Animation Speed Y", Range(-2,2)) = 1
		_AnimSpeedZ("Animation Speed Z", Range(-2,2)) = 1
		_Height("Noise Height", Range(0,2)) = 0.8
		_Strength("Noise Emission Strength", Range(0,2)) = 0.3
	}

		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200
		Cull Off
		CGPROGRAM

#pragma surface surf Lambert vertex:disp addshadow

		sampler2D _Noise;
	float4 _Color, _CloudColor, _TColor, _RimColor;
	float _Scale, _Strength, _RimPower, _Height;
	float _AnimSpeedX, _AnimSpeedY, _AnimSpeedZ;

	struct Input {
		float3 viewDir;
		float4 noiseComb;
		float4 col;
	};

	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	void disp(inout appdata_full v, out Input o)
	{
		UNITY_INITIALIZE_OUTPUT(Input, o);

		float3 worldSpaceNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal.xyz));// worldspace normal for blending
		float3 worldNormalS = saturate(pow(worldSpaceNormal * 1.4, 4)); // corrected blend value
		float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;// world position for projecting

		float movementSpeedX = _Time.x * _AnimSpeedX * 0.1; //movement over X
		float movementSpeedY = _Time.x * _AnimSpeedY * 0.1; //movement over Y
		float movementSpeedZ = _Time.x * _AnimSpeedZ * 0.1; //movement over Z

		float4 xy = float4((worldPos.x * _Scale) - movementSpeedX, (worldPos.y * _Scale) - movementSpeedY, 0, 0); // xy texture projection over worldpos * scale and movement
		float4 xz = float4((worldPos.x * _Scale) - movementSpeedX, (worldPos.z * _Scale) - movementSpeedZ, 0, 0); // same with xz
		float4 yz = float4((worldPos.y * _Scale) - movementSpeedY, (worldPos.z * _Scale) - movementSpeedZ, 0, 0); // same with yz

		float4 noiseXY = tex2Dlod(_Noise, xy);// textures projected
		float4 noiseXZ = tex2Dlod(_Noise, xz);
		float4 noiseYZ = tex2Dlod(_Noise, yz);

		o.noiseComb = noiseXY; // combining the texture sides
		o.noiseComb = lerp(o.noiseComb, noiseXZ, worldNormalS.y);
		o.noiseComb = lerp(o.noiseComb, noiseYZ, worldNormalS.x);

		v.vertex.xyz += (v.normal *(o.noiseComb * _Height)); // displacement

		o.col = lerp(_CloudColor, _TColor, v.vertex.y);// gradient over vertex
	}

	void surf(Input IN, inout SurfaceOutput o) {
		half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal * (IN.noiseComb* _Strength))); // rimlight using normal and noise
		o.Emission = _RimColor.rgb *pow(rim, _RimPower); // add glow rimlight to the clouds
		o.Albedo = IN.col *_Color;// gradient * color tint
	}
	ENDCG

	}

		Fallback "Diffuse"
}