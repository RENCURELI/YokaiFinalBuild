Shader "LOCAL/PBS/Human/Opaque"
{
    Properties
    {
		_WorldMapScale("World Map Scale", float) = 0

		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color (Human)", color) = (1,1,1,1)

		_NormalTex("Normal", 2D) = "bump" {}
		_NormalFactor("Normal Factor", float) = 1
		_MaskTex("Mask", 2D) = "white" {}
		_Smoothness("Smoothness", range(0.001, 1.2)) = 0.5
		_Metallic("Metallic", range(0, 1)) = 0
		_Reflectivity("Reflectivity", range(0, 1)) = 0
			 
		_Radius("Radius", float) = 5
		_Blend("Blend", float) = 1
		_Width("Width", float) = 1
		_Height("Height", float) = 1
		_Amplitude("Amplitude", float) = 1
		_Speed("Speed", float) = 1

		TessellationLimit("Tessellation Limit", Range(10, 200)) = 30
		TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1
		TessellationEdgeLength("Tessellation Edge Length", Range(5, 100)) = 50
	}
	SubShader
	{ 
		UsePass "LOCAL/PBS/All Worlds/Opaque/Human"
		UsePass "LOCAL/PBS/All Worlds/Opaque/Transition"
		UsePass "LOCAL/GEN/Diffuse/Shadow"
    }
}
