// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityFx.BuildTools.Editor
{
	/// <summary>
	/// Defines a generic build configuration.
	/// </summary>
	public interface IBuildConfig : IVersionConfig, IAppConfig
	{
		/// <summary>
		/// Gets scripting define symbols.
		/// </summary>
		string[] Defines { get; }

		/// <summary>
		/// Creates a new configuration with combined settings from this config and the <paramref name="fallbackConfig"/>.
		/// </summary>
		IBuildConfig Combine(IBuildConfig fallbackConfig);

		/// <summary>
		/// Creates a new configuration with combined settings from this config and the <paramref name="fallbackConfig"/>.
		/// </summary>
		IBuildConfig Combine(IVersionConfig fallbackConfig);

		/// <summary>
		/// Creates a new configuration with combined settings from this config and the <paramref name="fallbackConfig"/>.
		/// </summary>
		IBuildConfig Combine(IAppConfig fallbackConfig);

		/// <summary>
		/// Applies configuration to Unity <see cref="PlayerSettings"/>.
		/// </summary>
		void Apply();

		/// <summary>
		/// Writes the configuration summary to <see cref="Debug.Log(object)"/>.
		/// </summary>
		void DebugLog();
	}
}
