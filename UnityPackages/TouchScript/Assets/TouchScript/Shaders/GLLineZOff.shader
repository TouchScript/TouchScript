Shader "Hidden/GLLineZOff" {
	SubShader {
		Pass {
			Blend OneMinusDstColor Zero
			ZWrite Off
			ZTest Always
			Cull Off
			BindChannels {
				Bind "vertex", vertex
			}
		}
	}
}