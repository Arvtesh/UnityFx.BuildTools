// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;

namespace UnityFx.BuildTools.Editor
{
	/// <summary>
	/// The class provides easy access to repository version information (supplied by GitVersion).
	/// </summary>
	public class AppVersionInfo : IVersionConfig
	{
		#region data

		private readonly string _gitVersionResult = string.Empty;
		private readonly string _bundleVersion;
		private readonly string _bundleVersionMmp;
		private readonly string _branch;
		private readonly Version _version;
		private readonly int _major;
		private readonly int _minor;
		private readonly int _patch;
		private readonly int _buildCode;
		private readonly int _commitsSinceVersionSource;
		private readonly int _buildNumber;

		#endregion

		#region interface

		/// <summary>
		/// Gets application version string.
		/// </summary>
		public string BundleVersionFull
		{
			get
			{
				return _bundleVersion;
			}
		}

		/// <summary>
		/// Gets application version string.
		/// </summary>
		public Version Version
		{
			get
			{
				return _version;
			}
		}

		/// <summary>
		/// Gets the build number.
		/// </summary>
		public int NumberOfCommitsSinceVersionSource
		{
			get
			{
				return _commitsSinceVersionSource;
			}
		}

		/// <summary>
		/// Gets the source Git branch name.
		/// </summary>
		public string Branch
		{
			get
			{
				return _branch;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this is a release branch.
		/// </summary>
		public bool IsRelease
		{
			get
			{
				return _branch.Contains("release") || _branch == "master";
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AppVersionInfo"/> class.
		/// </summary>
		public AppVersionInfo(string gitVersionPath, int buildNumber = 0)
		{
#if UNITY_EDITOR_OSX

			var monoPath = Path.Combine(EditorApplication.applicationContentsPath, "MonoBleedingEdge/bin/cli");

			var proc = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = monoPath,
					Arguments = gitVersionPath,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};

#elif UNITY_EDITOR_WIN

			var proc = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = gitVersionPath,
					Arguments = string.Empty,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};

#else

			throw new PlatformNotSupportedException("Only WIN and OSX platforms are supported.");

#endif

			proc.Start();

			_gitVersionResult = proc.StandardOutput.ReadToEnd();

			var major = GetJsonNumber(_gitVersionResult, "Major");
			var minor = GetJsonNumber(_gitVersionResult, "Minor");
			var patch = GetJsonNumber(_gitVersionResult, "Patch");
			var n = GetJsonNumber(_gitVersionResult, "CommitsSinceVersionSource");

			_buildNumber = buildNumber;
			_major = int.Parse(major);
			_minor = int.Parse(minor);
			_patch = int.Parse(patch);
			_commitsSinceVersionSource = int.Parse(n);
			_buildCode = GetBundleVersionCode(_major, _minor, _patch, _commitsSinceVersionSource);
			_branch = GetJsonValue(_gitVersionResult, "BranchName");
			_bundleVersion = GetJsonValue(_gitVersionResult, "SemVer");
			_bundleVersionMmp = GetJsonValue(_gitVersionResult, "MajorMinorPatch");
			_version = new Version(_major, _minor, _commitsSinceVersionSource, _patch);
		}

		/// <summary>
		/// Applies the version information to <see cref="PlayerSettings"/>.
		/// </summary>
		public void Apply()
		{
			BuildUtility.ApplyVersionConfig(this);
		}

		/// <summary>
		/// Logs the version info to Unity <see cref="UnityEngine.Debug"/>.
		/// </summary>
		public void DebugLog()
		{
			var text = new StringBuilder();

			text.AppendLine("<b>GITVERSION CONFIG:</b>");
			text.AppendLine("  GitVersion output: " + _gitVersionResult);
			text.AppendLine("  BundleVersion: " + BundleVersion);
			text.AppendLine("  BundleVersionFull: " + BundleVersionFull);
			text.AppendLine("  BundleVersionCode: " + BundleVersionCode);
			text.AppendLine("  BuildNumber: " + BuildNumber);
			text.AppendLine("  .NET version: " + Version);

			UnityEngine.Debug.Log(text.ToString());
		}

		/// <summary>
		/// Calculates version code.
		/// </summary>
		public static int GetBundleVersionCode(int major, int minor, int patch, int build)
		{
			return 100000000 * major + 1000000 * minor + 10000 * patch + build;
		}

		#endregion

		#region IVersionConfig

		/// <summary>
		/// Gets application version string.
		/// </summary>
		public string BundleVersion
		{
			get
			{
				if (IsRelease)
				{
#if UNITY_IOS
					return _bundleVersionMmp;
#else
					return _bundleVersionMmp + '.' + _commitsSinceVersionSource.ToString(NumberFormatInfo.InvariantInfo);
#endif
				}

				return _bundleVersion;
			}
		}

		/// <summary>
		/// Gets the build code.
		/// </summary>
		public int BundleVersionCode
		{
			get
			{
				return _buildCode;
			}
		}

		/// <summary>
		/// Gets the build number.
		/// </summary>
		public int BuildNumber
		{
			get
			{
				return _buildNumber > 0 ? _buildNumber : _commitsSinceVersionSource;
			}
		}

		#endregion

		#region Object

		public override string ToString()
		{
			return _gitVersionResult;
		}

		#endregion

		#region implementation

		private static string GetJsonNumber(string text, string key)
		{
			var keyPos = text.IndexOf(key, StringComparison.Ordinal);
			var valuePos = keyPos + key.Length + 2;
			var valueEndPos = text.IndexOf(',', valuePos);
			return text.Substring(valuePos, valueEndPos - valuePos);
		}

		private static string GetJsonValue(string text, string key)
		{
			var keyPos = text.IndexOf(key, StringComparison.Ordinal);
			var valuePos = keyPos + key.Length + 3;
			var valueEndPos = text.IndexOf('\"', valuePos);
			return text.Substring(valuePos, valueEndPos - valuePos);
		}

		#endregion
	}
}
