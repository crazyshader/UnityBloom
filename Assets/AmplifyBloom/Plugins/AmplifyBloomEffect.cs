// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
using System;
using UnityEngine;



namespace AmplifyBloom
{
#if UNITY_5_4_OR_NEWER
	[ImageEffectAllowedInSceneView]
#endif
	[ExecuteInEditMode]
	[System.Serializable]
	[RequireComponent( typeof( Camera ) )]
	[AddComponentMenu( "Image Effects/Amplify Bloom")]
	public sealed class AmplifyBloomEffect : AmplifyBloomBase { }
}
