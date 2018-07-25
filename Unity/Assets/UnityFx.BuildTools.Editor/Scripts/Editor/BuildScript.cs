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

namespace UnityFx.BuildTools
{
	/// <summary>
	/// The main build sript for the project.
	/// </summary>
	public static class BuildScript
	{
		#region data
		#endregion

		#region interface

		/// <summary>
		/// Loads combined config for the specified target.
		/// </summary>
		public static IBuildConfig LoadConfig()
		{
			var configType = GetBuildConfigType();
			return LoadConfig(configType);
		}

		/// <summary>
		/// Loads combined config for the specified target.
		/// </summary>
		public static IBuildConfig LoadConfig(Type configType)
		{
			var config = BuildConfig.FromEnvironment(configType);
			return LoadConfig(config);
		}

#if UNITY_PURCHASING

		/// <summary>
		/// Loads combined config for the specified target.
		/// </summary>
		public static IBuildConfig LoadConfig(BuildTarget target, AppStore store)
		{
			var configType = GetBuildConfigType();
			return LoadConfig(target, store, false, configType);
		}

		/// <summary>
		/// Loads combined config for the specified target.
		/// </summary>
		public static IBuildConfig LoadConfig(BuildTarget target, AppStore store, bool developmentBuild)
		{
			var configType = GetBuildConfigType();
			return LoadConfig(target, store, developmentBuild, configType);
		}

		/// <summary>
		/// Loads combined config for the specified target.
		/// </summary>
		public static IBuildConfig LoadConfig(BuildTarget target, AppStore store, bool developmentBuild, Type configType)
		{
			var config = BuildConfig.FromEnvironment(configType);

			config.BuildTarget = target;
			config.Store = store;

			if (developmentBuild)
			{
				config.BuildOptions |= BuildOptions.AllowDebugging | BuildOptions.Development;
			}

			return LoadConfig(config);
		}

#endif

		/// <summary>
		/// Loads combined config for the specified target.
		/// </summary>
		public static IBuildConfig LoadConfig(BuildTarget target)
		{
			var configType = GetBuildConfigType();
			return LoadConfig(target, false, configType);
		}

		/// <summary>
		/// Loads combined config for the specified target.
		/// </summary>
		public static IBuildConfig LoadConfig(BuildTarget target, bool developmentBuild)
		{
			var configType = GetBuildConfigType();
			return LoadConfig(target, developmentBuild, configType);
		}

		/// <summary>
		/// Loads combined config for the specified target.
		/// </summary>
		public static IBuildConfig LoadConfig(BuildTarget target, bool developmentBuild, Type configType)
		{
#if UNITY_PURCHASING

			return LoadConfig(target, BuildUtility.GetDefaultStore(target), developmentBuild, configtype);

#else

			var config = BuildConfig.FromEnvironment(configType);

			config.BuildTarget = target;

			if (developmentBuild)
			{
				config.BuildOptions |= BuildOptions.AllowDebugging | BuildOptions.Development;
			}

			return LoadConfig(config);
#endif
		}

		/// <summary>
		/// Loads combined config for the specified target/type.
		/// </summary>
		public static IBuildConfig LoadConfig(IBuildConfig config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}

#if UNITY_PURCHASING

			var platformConfigPath = GetConfigPath(config.BuildConfigPath, config.BuildConfigName, config.BuildTarget, config.Store);

			if (!File.Exists(platformConfigPath))
			{
				platformConfigPath = GetConfigPath(config.BuildConfigPath, config.BuildConfigName, config.BuildTarget);
			}

#else

			var platformConfigPath = GetConfigPath(config.BuildConfigPath, config.BuildConfigName, config.BuildTarget);

#endif

			var sharedConfigPath = GetConfigPath(config.BuildConfigPath, config.BuildConfigName);

			if (File.Exists(platformConfigPath))
			{
				var platformConfig = BuildConfig.FromXml(config.GetType(), platformConfigPath);
				config = config.Combine(platformConfig);
			}

			if (File.Exists(sharedConfigPath))
			{
				var sharedConfig = BuildConfig.FromXml(config.GetType(), sharedConfigPath);
				config = config.Combine(sharedConfig);
			}

			if (File.Exists(config.GitVersionPath))
			{
				var versionConfig = new AppVersionInfo(config.BuildTarget, config.GitVersionPath, config.BuildNumber);
				config = config.Combine(versionConfig);
			}

			return config;
		}

		/// <summary>
		/// Applies default build settings.
		/// </summary>
		public static void ApplyBuildConfig()
		{
			var config = LoadConfig();

			config.Apply();
			config.DebugLog();
		}

		/// <summary>
		/// Builds the project for the specified target.
		/// </summary>
		public static BuildReport Build(BuildTarget target, bool developmentBuild)
		{
			var config = LoadConfig(target, developmentBuild);
			return Build(config);
		}

#if UNITY_PURCHASING

		/// <summary>
		/// Builds the project for the specified target.
		/// </summary>
		public static BuildReport Build(BuildTarget target, AppStore store, bool developmentBuild)
		{
			var settings = GetSettings();
			var config = LoadConfig(settings, target, store);
			return Build(target, config, developmentBuild);
		}

#endif

		/// <summary>
		/// Builds the project for the specified target.
		/// </summary>
		public static BuildReport Build(IBuildConfig config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}

			var buildPath = GetBuildPath(config);
			var executableName = GetExecutableName(config.ProductId, config.BuildTarget);
			var executablePath = Path.Combine(buildPath, executableName);
			var sceneNames = config.Scenes != null ? config.Scenes : EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
			var playerOptions = new BuildPlayerOptions()
			{
				locationPathName = executablePath,
				options = config.BuildOptions,
				scenes = sceneNames,
				target = config.BuildTarget
			};

			Directory.CreateDirectory(buildPath);

			config.Apply();
			config.DebugLog();

			Debug.Log(string.Format("<b>Build</b>: {0}", Path.GetFullPath(executablePath)));
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
				Debug.Log("<b>PreExport</b>:" + manifest.ToJson());

				if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BUILD_NUMBER")))
				{
					var buildNumber = manifest.GetValue<string>("buildNumber");
					Environment.SetEnvironmentVariable("BUILD_NUMBER", buildNumber);
				}

				var config = LoadConfig();

				config.Apply();
				config.DebugLog();
			}

			/// <summary>
			/// Should be set as PostExport method in Unity Cloud Build settings.
			/// </summary>
			public static void PostExport(string exportPath)
			{
				Debug.Log("<b>PostExport</b>:" + exportPath);
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

		private static string GetConfigPath(string basePath, string name)
		{
			return Path.Combine(basePath, name) + ".xml";
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

#if UNITY_PURCHASING

		private static string GetConfigPath(string basePath, string name, BuildTarget target, AppStore store)
		{
			var path = Path.Combine(basePath, name);

			if (target != BuildTarget.NoTarget)
			{
				path += '.' + target.ToString();
			}

			if (store != AppStore.NotSpecified && store != BuildUtility.GetDefaultStore(target))
			{
				path += '.' + store.ToString();
			}

			return path + ".xml";
		}

#endif

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

		private static string GetBuildPath(IBuildConfig config)
		{
			var targetStr = config.BuildTarget.ToString();

#if UNITY_PURCHASING

			if (store != AppStore.NotSpecified && store != BuildUtility.GetDefaultStore(target))
			{
				targetStr += '.' + store.ToString();
			}

#endif

			if (string.IsNullOrEmpty(config.ProductId))
			{
				return Path.Combine(Path.Combine(config.BuildPath, "v" + config.BundleVersion), targetStr);
			}
			else
			{
				return Path.Combine(Path.Combine(config.BuildPath, config.ProductId + "_v" + config.BundleVersion), targetStr);
			}
		}

		#endregion
	}
}
