Shader "LOCAL/GEN/Diffuse Spirit Only"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color (Human)", color) = (1,1,1,1)
		_SpiritColor("Color (Spirit)", color) = (1,1,1,1)

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
	
		UsePass "LOCAL/GEN/Diffuse/Spirit"
		UsePass "LOCAL/GEN/Diffuse/Transition"
		UsePass "LOCAL/GEN/Diffuse/Shadow"
    }
}
