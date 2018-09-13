// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityFx.BuildTools
{
	/// <summary>
	/// Defines a generic build configuration.
	/// </summary>
	public interface IBuildConfig : IVersionConfig, IAppConfig
	{
		/// <summary>
		/// Gets player build target user for <see cref="BuildPipeline.BuildPlayer(BuildPlayerOptions)"/>.
		/// </summary>
		BuildTarget BuildTarget { get; }

		/// <summary>
		/// Gets player build options user for <see cref="BuildPipeline.BuildPlayer(BuildPlayerOptions)"/>.
		/// </summary>
		BuildOptions BuildOptions { get; }

		/// <summary>
		/// Gets relative path to the GitVersion executable.
		/// </summary>
		string GitVersionPath { get; }

		/// <summary>
		/// Gets relative path to a folder containing build results.
		/// </summary>
		string BuildPathBase { get; }

		/// <summary>
		/// Gets relative path to a build executable.
		/// </summary>
		string BuildPath { get; }

		/// <summary>
		/// Gets relative path to a folder containing build configuration.
		/// </summary>
		string BuildConfigPath { get; }

		/// <summary>
		/// Gets name prefix for configuration file.
		/// </summary>
		string BuildConfigName { get; }

		/// <summary>
		/// Gets value for <see cref="PlayerSettings.Android.keystoreName"/>.
		/// </summary>
		string KeystoreName { get; }

		/// <summary>
		/// Gets value for <see cref="PlayerSettings.Android.keystorePass"/>.
		/// </summary>
		string KeystorePass { get; }

		/// <summary>
		/// Gets value for <see cref="PlayerSettings.Android.keyaliasName"/>.
		/// </summary>
		string KeyaliasName { get; }

		/// <summary>
		/// Gets value for <see cref="PlayerSettings.Android.keyaliasPass"/>.
		/// </summary>
		string KeyaliasPass { get; }

		/// <summary>
		/// Gets scripting define symbols.
		/// </summary>
		string[] Defines { get; }

		/// <summary>
		/// Gets scenes user for <see cref="BuildPipeline.BuildPlayer(BuildPlayerOptions)"/>.
		/// </summary>
		string[] Scenes { get; }

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
		/// Validates the config settings.
		/// </summary>
		void Validate();

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
