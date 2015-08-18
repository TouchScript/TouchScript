Shader "Hidden/DebugNoDepthTest" {
	SubShader {
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off
			BindChannels {
                Bind "vertex", vertex
                Bind "color", color
        	}
		}
	}
}