// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.BuildTools
{
	/// <summary>
	/// Runtime application build info.
	/// </summary>
	public class CloudBuildInfo
	{
		#region data

		private const string _manifestPath = "UnityCloudBuildManifest.json";

#pragma warning disable 0649

		[Serializable]
		private class BuildInfo
		{
			public string scmCommitId;
			public string scmBranch;
			public int buildNumber;
			public string buildStartTime;
			public string projectId;
			public string bundleId;
			public string unityVersion;
			public string xcodeVersion;
			public string cloudBuildTargetName;
		}

#pragma warning restore 0649

		private readonly BuildInfo _data;

		#endregion

		#region interface

		/// <summary>
		/// Gets the Commit or changelist built by UCB.
		/// </summary>
		public string CommitId
		{
			get
			{
				return _data.scmCommitId;
			}
		}

		/// <summary>
		/// Gets the name of the branch that was built.
		/// </summary>
		public string Branch
		{
			get
			{
				return _data.scmBranch;
			}
		}

		/// <summary>
		/// Gets the Unity Cloud Build number corresponding to this build.
		/// </summary>
		public int BuildNumber
		{
			get
			{
				return _data.buildNumber;
			}
		}

		/// <summary>
		/// Gets the UCB project identifier.
		/// </summary>
		public string ProjectId
		{
			get
			{
				return _data.projectId;
			}
		}

		/// <summary>
		/// Gets the bundleIdentifier configured in Unity Cloud Build (iOS and Android only).
		/// </summary>
		public string BundleId
		{
			get
			{
				return _data.bundleId;
			}
		}

		/// <summary>
		/// Gets the version of Unity used by UCB to create the build.
		/// </summary>
		public string UnityVersion
		{
			get
			{
				return _data.unityVersion;
			}
		}

		/// <summary>
		/// Gets the version of XCode used to build the project (iOS only).
		/// </summary>
		public string XcodeVersion
		{
			get
			{
				return _data.xcodeVersion;
			}
		}

		/// <summary>
		/// Gets the name of the project build target that was built. Currently, this will correspond to the platform, as either "default-web”, “default-ios”, or “default-android".
		/// </summary>
		public string BuildTargetName
		{
			get
			{
				return _data.cloudBuildTargetName;
			}
		}

		/// <summary>s
		/// Initializes a new instance of the <see cref="CloudBuildInfo"/> class.
		/// </summary>
		public CloudBuildInfo(string manifestText)
		{
			if (manifestText == null)
			{
				throw new ArgumentNullException("manifestText");
			}

			_data = JsonUtility.FromJson<BuildInfo>(manifestText);
		}

		/// <summary>s
		/// Initializes a new instance of the <see cref="CloudBuildInfo"/> class.
		/// </summary>
		public CloudBuildInfo(ResourceRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			_data = JsonUtility.FromJson<BuildInfo>(((TextAsset)request.asset).text);
		}

		/// <summary>
		/// Creates and initializes the <see cref="CloudBuildInfo"/> instance.
		/// </summary>
		public static ResourceRequest LoadManifestDataAsync()
		{
			return Resources.LoadAsync(_manifestPath);
		}

		#endregion

		#region implementation
		#endregion
	}
}
