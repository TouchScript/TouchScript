Shader "Hidden/GLLineZOn" {
	SubShader {
		Pass {
			Blend OneMinusDstColor Zero
			ZWrite Off
			Cull Off
			BindChannels {
				Bind "vertex", vertex
			}
		}
	}
}