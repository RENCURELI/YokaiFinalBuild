﻿Shader "LOCAL/PBS/Spirit/Emit"
{
    Properties
    {
		_WorldMapScale("World Map Scale", float) = 0

		_EmitTex("Emit", 2D) = "black" {}
		_EmitColor("Emit Color", color) = (1,1,1,1)
		_EmitFactor("Emit Factor", float) = 1
		_EmitFlicker("Emit Flicker", range(0,1)) = 0.2

		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color (Human)", color) = (1,1,1,1)
		_NormalTex("Normal", 2D) = "bump" {}
		_NormalFactor("Normal Factor", float) = 1
		_MaskTex("Mask", 2D) = "white" {}

		_SpiritTex("Spirit Texture", 2D) = "white" {}
		_SpiritColor("Color (Spirit)", color) = (1,1,1,1)
		_SpiritNormalTex("Spirit Normal", 2D) = "bump" {}
		_SpiritMaskTex("Spirit Mask", 2D) = "white" {}

		_Smoothness("Smoothness", range(0.001, 1.2)) = 0.5
		_Metallic("Metallic", range(0, 1)) = 0
		_Reflectivity("Reflectivity", range(0, 1)) = 0
	}
	SubShader
	{ 
		UsePass "LOCAL/PBS/All Worlds/Emit/Spirit"
		UsePass "LOCAL/PBS/All Worlds/Opaque/Transition"
		UsePass "LOCAL/GEN/Diffuse/Shadow"
    }
}