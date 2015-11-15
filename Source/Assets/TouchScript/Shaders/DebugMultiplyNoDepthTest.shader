Shader "Hidden/DebugMultiplyNoDepthTest" {
	SubShader {
		Pass {
			Blend OneMinusDstColor Zero
			ZWrite Off
			ZTest Always
			Cull Off
			BindChannels {
				Bind "vertex", vertex
				Bind "color", color
			}
		}
	}
}