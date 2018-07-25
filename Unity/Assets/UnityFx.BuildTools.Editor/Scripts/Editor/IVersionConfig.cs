// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEditor;

namespace UnityFx.BuildTools
{
	/// <summary>
	/// Defines a configuration for Unity version.
	/// </summary>
	public interface IVersionConfig
	{
		/// <summary>
		/// Gets a value for <see cref="PlayerSettings.bundleVersion"/>.
		/// </summary>
		string BundleVersion { get; }

		/// <summary>
		/// Gets a value for <see cref="PlayerSettings.Android.bundleVersionCode"/>.
		/// </summary>
		int BundleVersionCode { get; }

		/// <summary>
		/// Gets a value for <see cref="PlayerSettings.iOS.buildNumber"/>.
		/// </summary>
		int BuildNumber { get; }
	}
}
