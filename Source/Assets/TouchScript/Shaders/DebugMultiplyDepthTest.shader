Shader "Hidden/DebugMultiplyDepthTest" {
	SubShader {
		Pass {
			Blend OneMinusDstColor Zero
			ZWrite Off
			Cull Off
			BindChannels {
				Bind "vertex", vertex
				Bind "color", color
			}
		}
	}
}