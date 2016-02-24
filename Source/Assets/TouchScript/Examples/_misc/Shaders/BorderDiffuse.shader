Shader "TouchScript/BorderDiffuse" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Border ("Border Size", Float) = 0
}

SubShader {
	Tags {"RenderType"="Opaque" "IgnoreProjector"="True"}
	LOD 200

CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
float _Border;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c;
	if (IN.uv_MainTex.x < _Border || IN.uv_MainTex.x > 1 - _Border || IN.uv_MainTex.y < _Border || IN.uv_MainTex.y > 1 - _Border)
		c = float4(1, 1, 1, 1);
	else
		c = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = c.rgb;
	o.Alpha = 1;
}
ENDCG
}
}
