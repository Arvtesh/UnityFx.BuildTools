// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_CLOUD_BUILD
using UnityEngine.CloudBuild;
#endif

namespace UnityFx.BuildTools.Editor
{
	/// <summary>
	/// The main build sript for the project.
	/// </summary>
	public static class BuildScript
	{
		#region data

		private static Settings _settings;

		#endregion

		#region interface

		/// <summary>
		/// 
		/// </summary>
		public sealed class Settings
		{
			/// <summary>
			/// 
			/// </summary>
			public const string DefaultGitVersionPath = "../../Tools/GitVersion/GitVersion.exe";

			/// <summary>
			/// 
			/// </summary>
			public const string DefaultBuildPath = "../../Builds";

			/// <summary>
			/// 
			/// </summary>
			public const string DefaultEnvPrefix = "ENV_";

			/// <summary>
			/// 
			/// </summary>
			public const string DefaultBuildConfigPath = "../";

			/// <summary>
			/// 
			/// </summary>
			public const string DefaultBuildConfigName = "BuildConfig";

			/// <summary>
			/// 
			/// </summary>
			public static Settings FromEnvironment()
			{
				var result = new Settings();

				result.GitVersionPath = Environment.GetEnvironmentVariable("GITVERSION_PATH");
				result.BuildConfigPath = Environment.GetEnvironmentVariable("BUILD_CONFIG_PATH");
				result.BuildConfigName = Environment.GetEnvironmentVariable("BUILD_CONFIG_NAME");
				result.EnvNamePrefix = string.Empty;

				// BUILD_PATH
				result.BuildPath = Environment.GetEnvironmentVariable("BUILD_PATH");

				if (string.IsNullOrEmpty(result.BuildPath))
				{
					// Jenkins build path (if any)
					result.BuildPath = Environment.GetEnvironmentVariable("BUILD_URL");
				}

				// BUILD_NUMBER
				var buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER");

				if (!string.IsNullOrEmpty(buildNumber))
				{
					result.BuildNumber = int.Parse(buildNumber);
				}

				result.Validate();
				return result;
			}

			/// <summary>
			/// 
			/// </summary>
			public void Validate()
			{
				if (EnvNamePrefix == null)
				{
					EnvNamePrefix = DefaultEnvPrefix;
				}

				if (string.IsNullOrEmpty(BuildConfigPath))
				{
					BuildConfigPath = DefaultBuildConfigPath;
				}

				if (string.IsNullOrEmpty(BuildConfigName))
				{
					BuildConfigName = DefaultBuildConfigName;
				}

				if (string.IsNullOrEmpty(GitVersionPath))
				{
					GitVersionPath = DefaultGitVersionPath;
				}

				if (string.IsNullOrEmpty(BuildPath))
				{
					BuildPath = DefaultBuildPath;
				}
			}

			/// <summary>
			/// Gets or sets relative path to the GitVersion.
			/// </summary>
			public string GitVersionPath { get; set; }

			/// <summary>
			/// Gets or sets relative path to a folder that would contain build results.
			/// </summary>
			public string BuildPath { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string EnvNamePrefix { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string BuildConfigPath { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public string BuildConfigName { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public int BuildNumber { get; set; }
		}

		/// <summary>
		/// 
		/// </summary>
		public static Settings GetSettings()
		{
			var result = _settings;

			if (result == null)
			{
				result = Settings.FromEnvironment();
			}
			else
			{
				result.Validate();
			}

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		public static void SetSettings(Settings settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			_settings = settings;
		}

		/// <summary>
		/// Loads combined config for the specified target.
		/// </summary>
		public static IBuildConfig LoadConfig(Settings settings, IBuildConfig config = null)
		{
			var configType = GetBuildConfigType();
			var target = EditorUserBuildSettings.activeBuildTarget;
			return LoadConfig(settings, target, configType, config);
		}

		/// <summary>
		/// Loads combined config for the specified target.
		/// </summary>
		public static IBuildConfig LoadConfig(Settings settings, BuildTarget target, IBuildConfig config = null)
		{
			var configType = GetBuildConfigType();
			return LoadConfig(settings, target, configType, config);
		}

		/// <summary>
		/// Loads combined config for the specified target/type.
		/// </summary>
		public static IBuildConfig LoadConfig(Settings settings, BuildTarget target, Type configType, IBuildConfig config = null)
		{
			if (target == BuildTarget.NoTarget)
			{
				throw new ArgumentException("Invalid build target", "target");
			}

			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			if (configType == null)
			{
				throw new ArgumentNullException("configType");
			}

			var envConfig = BuildConfig.FromEnvironment(configType, settings.EnvNamePrefix);
			var platformConfigPath = GetConfigPath(settings.BuildConfigPath, settings.BuildConfigName, target);
			var sharedConfigPath = GetConfigPath(settings.BuildConfigPath, settings.BuildConfigName, BuildTarget.NoTarget);

			if (config != null)
			{
				config = config.Combine(envConfig);
			}
			else
			{
				config = envConfig;
			}

			if (File.Exists(platformConfigPath))
			{
				var platformConfig = BuildConfig.FromXml(configType, platformConfigPath);
				config = config.Combine(platformConfig);
			}

			if (File.Exists(sharedConfigPath))
			{
				var sharedConfig = BuildConfig.FromXml(configType, sharedConfigPath);
				config = config.Combine(sharedConfig);
			}

			if (File.Exists(settings.GitVersionPath))
			{
				var versionConfig = new AppVersionInfo(settings.GitVersionPath, settings.BuildNumber);
				config = config.Combine(versionConfig);
			}

			return config;
		}

		/// <summary>
		/// Applies default build settings.
		/// </summary>
		public static void ApplyBuildConfig()
		{
			var settings = GetSettings();
			var config = LoadConfig(settings);

			config.Apply();
			config.DebugLog();
		}

		/// <summary>
		/// Builds the project for the specified target.
		/// </summary>
		public static BuildReport Build(BuildTarget target, bool developmentBuild)
		{
			var settings = GetSettings();
			var config = LoadConfig(settings, target);
			return Build(target, config, settings, developmentBuild);
		}

		/// <summary>
		/// Builds the project for the specified target.
		/// </summary>
		public static BuildReport Build(BuildTarget target, IBuildConfig config, Settings settings, bool developmentBuild)
		{
			if (target == BuildTarget.NoTarget)
			{
				throw new ArgumentException("Invalid build target", "target");
			}

			if (config == null)
			{
				throw new ArgumentNullException("config");
			}

			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			var buildPath = GetBuildPath(config.ProductName, config.BundleVersion, settings.BuildPath, target);
			var executableName = GetExecutableName(config.ProductName, target);
			var executablePath = Path.Combine(buildPath, executableName);
			var sceneNames = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
			var buildOptions = GetBuildOptions(developmentBuild);
			var playerOptions = new BuildPlayerOptions()
			{
				locationPathName = executablePath,
				options = buildOptions,
				scenes = sceneNames,
				target = target
			};

			Directory.CreateDirectory(buildPath);

			config.Apply();
			config.DebugLog();

			return BuildPipeline.BuildPlayer(playerOptions);
		}

#if UNITY_CLOUD_BUILD

		/// <summary>
		/// Unity Cloud Build helpers.
		/// </summary>
		public static class CloudBuild
		{
			/// <summary>
			/// Should be set as PreExport method in Unity Cloud Build settings.
			/// </summary>
			public static void PreExport(BuildManifestObject manifest)
			{
				if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BUILD_NUMBER")))
				{
					var buildNumber = manifest.GetValue<string>("buildNumber");
					Environment.SetEnvironmentVariable("BUILD_NUMBER", buildNumber);
				}

				var settings = GetSettings();
				var config = LoadConfig(settings);

				config.Apply();
				config.DebugLog();
			}

			/// <summary>
			/// Should be set as PostExport method in Unity Cloud Build settings.
			/// </summary>
			public static void PostExport(string exportPath)
			{
			}
		}

#endif

		#endregion

		#region implementation

		private static Type GetBuildConfigType()
		{
			// Try find the actual config DTO (should be marked with BuildConfigAttribute).
			// If not found fall back to default one.
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var assemblyName = assembly.GetName().Name;

				// Skip system assemblies.
				if (assemblyName.StartsWith("UnityEngine") || assemblyName.StartsWith("UnityEditor") || assemblyName.StartsWith("System"))
				{
					continue;
				}

				foreach (var type in assembly.GetTypes())
				{
					if (type.IsSubclassOf(typeof(BuildConfig)))
					{
						return type;
					}
				}
			}

			return typeof(BuildConfig);
		}

		private static string GetConfigPath(string basePath, string name, BuildTarget target)
		{
			var path = Path.Combine(basePath, name);

			if (target != BuildTarget.NoTarget)
			{
				path += '.' + target.ToString();
			}

			return path + ".xml";
		}

		private static BuildOptions GetBuildOptions(bool debug)
		{
			if (debug)
			{
				return BuildOptions.Development | BuildOptions.AllowDebugging;
			}

			return BuildOptions.None;
		}

		private static string GetExecutableName(string appName, BuildTarget target)
		{
			var ext = ".exe";

			if (target == BuildTarget.Android)
			{
				ext = ".apk";
			}
			else if (target == BuildTarget.iOS)
			{
				ext = ".app";
			}

			if (string.IsNullOrEmpty(appName))
			{
				return "app" + ext;
			}
			else
			{
				return appName + ext;
			}
		}

		private static string GetBuildPath(string appName, string appVersion, string buildFolderPath, BuildTarget target)
		{
			if (string.IsNullOrEmpty(appName))
			{
				return Path.Combine(Path.Combine(buildFolderPath, "v" + appVersion), target.ToString());
			}
			else
			{
				return Path.Combine(Path.Combine(buildFolderPath, appName + "_v" + appVersion), target.ToString());
			}
		}

		#endregion
	}
}
