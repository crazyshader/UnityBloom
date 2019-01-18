// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using System;

namespace AmplifyBloom
{
	[Serializable]
	public class AmplifyPassCache
	{
		[SerializeField]
		internal Vector4[] Offsets;

		[SerializeField]
		internal Vector4[] Weights;

		public AmplifyPassCache()
		{
			Offsets = new Vector4[ 16 ];
			Weights = new Vector4[ 16 ];
		}

		public void Destroy()
		{
			Offsets = null;
			Weights = null;
		}
	}

	[Serializable]
	public class AmplifyStarlineCache
	{
		[SerializeField]
		internal AmplifyPassCache[] Passes;
		public AmplifyStarlineCache()
		{
			Passes = new AmplifyPassCache[ AmplifyGlare.MaxPasses ];
			for ( int i = 0; i < AmplifyGlare.MaxPasses; i++ )
			{
				Passes[ i ] = new AmplifyPassCache();
			}
		}

		public void Destroy()
		{
			for ( int i = 0; i < AmplifyGlare.MaxPasses; i++ )
			{
				Passes[ i ].Destroy();
			}
			Passes = null;
		}
	}

	[Serializable]
	public class AmplifyGlareCache
	{
		[SerializeField]
		internal AmplifyStarlineCache[] Starlines;

		[SerializeField]
		internal Vector4 AverageWeight;

		[SerializeField]
		internal Vector4[,] CromaticAberrationMat;

		[SerializeField]
		internal int TotalRT;

		[SerializeField]
		internal GlareDefData GlareDef;

		[SerializeField]
		internal StarDefData StarDef;

		[SerializeField]
		internal int CurrentPassCount;

		public AmplifyGlareCache()
		{
			Starlines = new AmplifyStarlineCache[ AmplifyGlare.MaxStarLines ];
			CromaticAberrationMat = new Vector4[ AmplifyGlare.MaxPasses, AmplifyGlare.MaxLineSamples ];
			for ( int i = 0; i < AmplifyGlare.MaxStarLines; i++ )
			{
				Starlines[ i ] = new AmplifyStarlineCache();
			}
		}

		public void Destroy()
		{
			for ( int i = 0; i < AmplifyGlare.MaxStarLines; i++ )
			{
				Starlines[ i ].Destroy();
			}
			Starlines = null;
			CromaticAberrationMat = null;
		}
	}
}
