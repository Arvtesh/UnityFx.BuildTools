// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
#if UNITY_PURCHASING
using UnityEditor.Purchasing;
#endif
using UnityEngine;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

namespace UnityFx.BuildTools
{
	/// <summary>
	/// A build configuration.
	/// </summary>
	public class BuildConfig : IBuildConfig
	{
		#region data
		#endregion

		#region interface

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
		public const string DefaultBuildConfigPath = "";

		/// <summary>
		/// 
		/// </summary>
		public const string DefaultBuildConfigName = "BuildConfig";

		/// <summary>
		/// Deserializes the class instance from XML file.
		/// </summary>
		public static BuildConfig FromXml(Type configType, string path)
		{
			if (configType == null)
			{
				throw new ArgumentNullException("configType");
			}

			using (var stream = new StreamReader(path))
			{
				var serializer = new XmlSerializer(configType);
				return (BuildConfig)serializer.Deserialize(stream);
			}
		}

		/// <summary>
		/// Deserializes the class instance from environment variables.
		/// </summary>
		public static BuildConfig FromEnvironment(Type configType)
		{
			if (configType == null)
			{
				throw new ArgumentNullException("configType");
			}

			var result = (BuildConfig)Activator.CreateInstance(configType);
			result.LoadFromEnvironment();
			return result;
		}

		/// <summary>
		/// Serializes the class instance to an XML file.
		/// </summary>
		public void Serialize(string path)
		{
			using (var stream = new StreamWriter(path))
			{
				var serializer = new XmlSerializer(GetType());
				serializer.Serialize(stream, this);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected virtual void LoadFromEnvironment()
		{
			// GitVersionPath
			GitVersionPath = Environment.GetEnvironmentVariable("GITVERSION_PATH");

			if (string.IsNullOrEmpty(GitVersionPath))
			{
				GitVersionPath = DefaultGitVersionPath;
			}

			// BuildPath
			BuildPath = Environment.GetEnvironmentVariable("BUILD_PATH");

			if (string.IsNullOrEmpty(BuildPath))
			{
				// Jenkins build path (if any)
				BuildPath = Environment.GetEnvironmentVariable("BUILD_URL");

				if (string.IsNullOrEmpty(BuildPath))
				{
					BuildPath = DefaultBuildPath;
				}
			}

			// BuildConfigPath
			BuildConfigPath = Environment.GetEnvironmentVariable("BUILD_CONFIG_PATH");

			if (string.IsNullOrEmpty(BuildConfigPath))
			{
				BuildConfigPath = DefaultBuildConfigPath;
			}

			// BuildConfigName
			BuildConfigName = Environment.GetEnvironmentVariable("BUILD_CONFIG_NAME");

			if (string.IsNullOrEmpty(BuildConfigName))
			{
				BuildConfigName = DefaultBuildConfigName;
			}

			// Android keystore
			KeystoreName = Environment.GetEnvironmentVariable("KEYSTORE_NAME");
			KeystorePass = Environment.GetEnvironmentVariable("KEYSTORE_PASS");
			KeyaliasName = Environment.GetEnvironmentVariable("KEYALIAS_NAME");
			KeyaliasPass = Environment.GetEnvironmentVariable("KEYALIAS_PASS");

			// BuildTarget
			var target = Environment.GetEnvironmentVariable("BUILD_TARGET");

			if (string.IsNullOrEmpty(target))
			{
				BuildTarget = EditorUserBuildSettings.activeBuildTarget;
			}
			else
			{
				BuildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), target);
			}

			// BuildOptions
			var options = Environment.GetEnvironmentVariable("DEVELOPMENT_BUILD");

			if (!string.IsNullOrEmpty(options))
			{
				BuildOptions = BuildOptions.AllowDebugging | BuildOptions.Development;
			}

#if UNITY_PURCHASING

			// Store
			var store = Environment.GetEnvironmentVariable("STORE");

			if (string.IsNullOrEmpty(store))
			{
				Store = BuildUtility.GetDefaultStore(BuildTarget);
			}
			else
			{
				Store = (AppStore)Enum.Parse(typeof(AppStore), store);
			}

#endif

			// BundleVersion
			BundleVersion = Environment.GetEnvironmentVariable("BUNDLE_VERSION");

			// BundleVersionCode
			var bundleVersionCode = Environment.GetEnvironmentVariable("BUNDLE_VERSION_CODE");

			if (!string.IsNullOrEmpty(bundleVersionCode))
			{
				BundleVersionCode = int.Parse(bundleVersionCode);
			}

			// BuildNumber
			var buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER");

			if (!string.IsNullOrEmpty(buildNumber))
			{
				BuildNumber = int.Parse(buildNumber);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fallbackConfig"></param>
		/// <param name="result"></param>
		protected virtual void Combine(IBuildConfig fallbackConfig, BuildConfig result)
		{
			// Scenes
			if (Scenes == null)
			{
				result.Scenes = fallbackConfig.Scenes;
			}

			// Defines
			var defines = new HashSet<string>();

			if (Defines != null)
			{
				defines.UnionWith(Defines);
			}

			if (fallbackConfig.Defines != null)
			{
				defines.UnionWith(fallbackConfig.Defines);
			}

			result.Defines = defines.ToArray();

			// App/version configs
			CombineAppConfig(fallbackConfig, result);
			CombineVersionConfig(fallbackConfig, result);
		}

		/// <summary>
		/// Chooses string value.
		/// </summary>
		protected static string GetValue(string value, string value2, string defaulValue = null)
		{
			if (!string.IsNullOrEmpty(value))
			{
				return value;
			}
			else if (!string.IsNullOrEmpty(value2))
			{
				return value2;
			}

			return defaulValue;
		}

		/// <summary>
		/// Chooses integer value.
		/// </summary>
		protected static int GetValue(int value, int value2, int defaulValue = 0)
		{
			if (value > 0)
			{
				return value;
			}
			else if (value2 > 0)
			{
				return value2;
			}

			return defaulValue;
		}

		#endregion

		#region IBuildConfig

		/// <summary>
		/// Gets or sets player build target user for <see cref="BuildPipeline.BuildPlayer(BuildPlayerOptions)"/>.
		/// </summary>
		[XmlIgnore]
		public BuildTarget BuildTarget { get; set; }

		/// <summary>
		/// Gets or sets player build options user for <see cref="BuildPipeline.BuildPlayer(BuildPlayerOptions)"/>.
		/// </summary>
		[XmlIgnore]
		public BuildOptions BuildOptions { get; set; }

		/// <summary>
		/// Gets or sets relative path to the GitVersion.
		/// </summary>
		[XmlIgnore]
		public string GitVersionPath { get; set; }

		/// <summary>
		/// Gets or sets relative path to a folder that would contain build results.
		/// </summary>
		[XmlIgnore]
		public string BuildPath { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[XmlIgnore]
		public string BuildConfigPath { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[XmlIgnore]
		public string BuildConfigName { get; set; }

		/// <summary>
		/// Gets or sets value for <see cref="PlayerSettings.Android.keystoreName"/>.
		/// </summary>
		[XmlIgnore]
		public string KeystoreName { get; set; }

		/// <summary>
		/// Gets or sets value for <see cref="PlayerSettings.Android.keystorePass"/>.
		/// </summary>
		[XmlIgnore]
		public string KeystorePass { get; set; }

		/// <summary>
		/// Gets or sets value for <see cref="PlayerSettings.Android.keyaliasName"/>.
		/// </summary>
		[XmlIgnore]
		public string KeyaliasName { get; set; }

		/// <summary>
		/// Gets or sets value for <see cref="PlayerSettings.Android.keyaliasPass"/>.
		/// </summary>
		[XmlIgnore]
		public string KeyaliasPass { get; set; }

		/// <summary>
		/// Gets or sets scripting define symbols.
		/// </summary>
		[XmlArrayItem("Define")]
		public string[] Defines { get; set; }

		/// <summary>
		/// Gets or sets scene paths.
		/// </summary>
		[XmlArrayItem("Scene")]
		public string[] Scenes { get; set; }

		/// <summary>
		/// Creates a new configuration with combined settings from this config and the <paramref name="fallbackConfig"/>.
		/// </summary>
		public IBuildConfig Combine(IBuildConfig fallbackConfig)
		{
			if (fallbackConfig == null)
			{
				throw new ArgumentNullException("fallbackConfig");
			}

			var result = MemberwiseClone() as BuildConfig;
			Combine(fallbackConfig, result);
			return result;
		}

		/// <summary>
		/// Creates a new configuration with combined settings from this config and the <paramref name="fallbackConfig"/>.
		/// </summary>
		public IBuildConfig Combine(IVersionConfig fallbackConfig)
		{
			if (fallbackConfig == null)
			{
				throw new ArgumentNullException("fallbackConfig");
			}

			var result = MemberwiseClone() as BuildConfig;
			CombineVersionConfig(fallbackConfig, result);
			return result;
		}

		/// <summary>
		/// Creates a new configuration with combined settings from this config and the <paramref name="fallbackConfig"/>.
		/// </summary>
		public IBuildConfig Combine(IAppConfig fallbackConfig)
		{
			if (fallbackConfig == null)
			{
				throw new ArgumentNullException("fallbackConfig");
			}

			var result = MemberwiseClone() as BuildConfig;
			CombineAppConfig(fallbackConfig, result);
			return result;
		}

		/// <summary>
		/// Applies configuration to Unity <see cref="PlayerSettings"/>.
		/// </summary>
		public virtual void Apply()
		{
			BuildUtility.ApplyBuildConfig(this);
		}

		/// <summary>
		/// Writes the configuration summary to <see cref="Debug.Log(object)"/>.
		/// </summary>
		public void DebugLog()
		{
			var text = new StringBuilder();

			text.AppendLine("<b>BUILD CONFIG:</b>");
			text.AppendLine("  ProductId: " + (ProductId != null ? ProductId : string.Empty));
			text.AppendLine("  ProductName: " + (ProductName != null ? ProductName : string.Empty));
			text.AppendLine("  CompanyId: " + (CompanyId != null ? CompanyId : string.Empty));
			text.AppendLine("  CompanyName: " + (CompanyName != null ? CompanyName : string.Empty));
			text.AppendLine("  BundleIdentifier: " + BuildUtility.GetBundleIdentifier(this));
			text.AppendLine("  BundleVersion: " + (BundleVersion != null ? BundleVersion : string.Empty));
			text.AppendLine("  BundleVersionCode: " + BundleVersionCode);
			text.AppendLine("  BuildNumber: " + BuildNumber);
#if UNITY_PURCHASING
			text.AppendLine("  Store: " + Store);
#endif
			text.AppendLine("  KeystoreName: " + (KeystoreName != null ? KeystoreName : string.Empty));
			text.AppendLine("  KeyaliasName: " + (KeyaliasName != null ? KeyaliasName : string.Empty));
			text.AppendLine("  Scenes: " + (Scenes != null ? string.Join(";", Scenes) : string.Empty));
			text.AppendLine("  Defines: " + (Defines != null ? string.Join(";", Defines) : string.Empty));
			text.AppendLine("  GitVersionPath: " + (GitVersionPath != null ? GitVersionPath : string.Empty));
			text.AppendLine("  BuildPath: " + (BuildPath != null ? BuildPath : string.Empty));
			text.AppendLine("  BuildConfigPath: " + (BuildConfigPath != null ? BuildConfigPath : string.Empty));
			text.AppendLine("  BuildConfigName: " + (BuildConfigName != null ? BuildConfigName : string.Empty));

			UnityEngine.Debug.Log(text.ToString());
		}

		#endregion

		#region IAppConfig

		/// <summary>
		/// Gets or sets product identifier.
		/// </summary>
		public string ProductId { get; set; }

		/// <summary>
		/// Gets or sets name of the (for <see cref="PlayerSettings.productName"/>).
		/// </summary>
		public string ProductName { get; set; }

		/// <summary>
		/// Gets or sets company identifier.
		/// </summary>
		public string CompanyId { get; set; }

		/// <summary>
		/// Gets or sets name of the (for <see cref="PlayerSettings.companyName"/>).
		/// </summary>
		public string CompanyName { get; set; }

		/// <summary>
		/// Gets or sets value of the (for <see cref="Application.identifier"/>).
		/// </summary>
		public string BundleIdentifier { get; set; }

#if UNITY_PURCHASING

		/// <summary>
		/// Gets or sets app store identifier.
		/// </summary>
		[XmlIgnore]
		public AppStore Store { get; set; }

#endif

		#endregion

		#region IVersionConfig

		/// <summary>
		/// Gets or sets override for <see cref="PlayerSettings.bundleVersion"/>.
		/// </summary>
		public string BundleVersion { get; set; }

		/// <summary>
		/// Gets or sets override for <see cref="PlayerSettings.Android.bundleVersionCode"/>.
		/// </summary>
		[DefaultValue(0)]
		public int BundleVersionCode { get; set; }

		/// <summary>
		/// Gets or sets override for <see cref="PlayerSettings.iOS.buildNumber"/>.
		/// </summary>
		[DefaultValue(0)]
		public int BuildNumber { get; set; }

		#endregion

		#region Object

		public override string ToString()
		{
			return base.ToString();
		}

		#endregion

		#region implementation

		private static void CombineVersionConfig(IVersionConfig fallbackConfig, BuildConfig result)
		{
			if (string.IsNullOrEmpty(result.BundleVersion))
			{
				result.BundleVersion = fallbackConfig.BundleVersion;
			}

			if (result.BundleVersionCode <= 0)
			{
				result.BundleVersionCode = fallbackConfig.BundleVersionCode;
			}

			if (result.BuildNumber <= 0)
			{
				result.BuildNumber = fallbackConfig.BuildNumber;
			}
		}

		private static void CombineAppConfig(IAppConfig fallbackConfig, BuildConfig result)
		{
			if (string.IsNullOrEmpty(result.CompanyId))
			{
				result.CompanyId = fallbackConfig.CompanyId;
			}

			if (string.IsNullOrEmpty(result.CompanyName))
			{
				result.CompanyName = fallbackConfig.CompanyName;
			}

			if (string.IsNullOrEmpty(result.ProductId))
			{
				result.ProductId = fallbackConfig.ProductId;
			}

			if (string.IsNullOrEmpty(result.ProductName))
			{
				result.ProductName = fallbackConfig.ProductName;
			}

			if (string.IsNullOrEmpty(result.BundleIdentifier))
			{
				result.BundleIdentifier = fallbackConfig.BundleIdentifier;
			}
		}

		#endregion
	}
}
