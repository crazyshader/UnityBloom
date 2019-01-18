// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public class VersionInfo
	{
		public const byte Major = 1;
		public const byte Minor = 2;
		public const byte Release = 0;

		private static string StageSuffix = "_dev001";



		public static string StaticToString()
		{
			return string.Format( "{0}.{1}.{2}", Major, Minor, Release ) + StageSuffix;
		}

		public override string ToString()
		{
			return string.Format( "{0}.{1}.{2}", m_major, m_minor, m_release ) + StageSuffix;
		}

		public int Number { get { return m_major * 100 + m_minor * 10 + m_release; } }

		[SerializeField]
		private int m_major;
		[SerializeField]
		private int m_minor;
		[SerializeField]
		private int m_release;

		VersionInfo()
		{
			m_major = Major;
			m_minor = Minor;
			m_release = Release;
		}

		VersionInfo( byte major, byte minor, byte release )
		{
			m_major = major;
			m_minor = minor;
			m_release = release;
		}

		public static VersionInfo Current()
		{
			return new VersionInfo( Major, Minor, Release );
		}

		public static bool Matches( VersionInfo version )
		{
			return ( Major == version.m_major ) && ( Minor == version.m_minor ) && ( Release == version.m_release );
		}
	}
}
