// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Globalization;
using System.IO;
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
	/// Build utilities.
	/// </summary>
	public static class BuildUtility
	{
		#region interface

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public static void ApplyBuildConfig(IBuildConfig config)
		{
			ApplyAppConfig(config, config.BuildTarget);
			ApplyVersionConfig(config);

			if (!string.IsNullOrEmpty(config.KeystoreName))
			{
				PlayerSettings.Android.keystoreName = config.KeystoreName;
			}

			if (config.KeystorePass != null)
			{
				PlayerSettings.Android.keystorePass = config.KeystorePass;
			}

			if (!string.IsNullOrEmpty(config.KeyaliasName))
			{
				PlayerSettings.Android.keyaliasName = config.KeyaliasName;
			}

			if (config.KeyaliasPass != null)
			{
				PlayerSettings.Android.keyaliasPass = config.KeyaliasPass;
			}

#if UNITY_PURCHASING

			if (config.Store != AppStore.NotSpecified)
			{
				UnityPurchasingEditor.TargetAndroidStore(config.Store);
			}

#endif

			if (config.Defines != null)
			{
				PlayerSettings.SetScriptingDefineSymbolsForGroup(GetBuildTargetGroup(config.BuildTarget), string.Join(";", config.Defines));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void ApplyVersionConfig(IVersionConfig config)
		{
			// Application.identifier
			if (!string.IsNullOrEmpty(config.BundleVersion))
			{
				PlayerSettings.bundleVersion = config.BundleVersion;
			}

			// BuildNumber
			if (config.BuildNumber > 0)
			{
				PlayerSettings.iOS.buildNumber = config.BuildNumber.ToString(NumberFormatInfo.InvariantInfo);
				PlayerSettings.macOS.buildNumber = config.BuildNumber.ToString(NumberFormatInfo.InvariantInfo);
				PlayerSettings.tvOS.buildNumber = config.BuildNumber.ToString(NumberFormatInfo.InvariantInfo);
			}

			// BundleVersionCode
			if (config.BundleVersionCode > 0)
			{
				PlayerSettings.Android.bundleVersionCode = config.BundleVersionCode;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public static void ApplyAppConfig(IAppConfig config, BuildTarget target)
		{
			// Application.productName
			if (!string.IsNullOrEmpty(config.ProductName))
			{
				PlayerSettings.productName = config.ProductName;
			}
			else if (!string.IsNullOrEmpty(config.ProductId))
			{
				PlayerSettings.productName = config.ProductId;
			}

			// Application.companyName
			if (!string.IsNullOrEmpty(config.CompanyName))
			{
				PlayerSettings.companyName = config.CompanyName;
			}
			else if (!string.IsNullOrEmpty(config.CompanyId))
			{
				PlayerSettings.companyName = config.CompanyId;
			}

			// Application.identifier
			var bundleIdentifier = GetBundleIdentifier(config);

			if (!string.IsNullOrEmpty(bundleIdentifier))
			{
				PlayerSettings.SetApplicationIdentifier(GetBuildTargetGroup(target), bundleIdentifier);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static string GetBundleIdentifier(IAppConfig config)
		{
			if (!string.IsNullOrEmpty(config.BundleIdentifier))
			{
				return config.BundleIdentifier;
			}
			else if (!string.IsNullOrEmpty(config.ProductId) && !string.IsNullOrEmpty(config.CompanyId))
			{
				return string.Format("com.{0}.{1}", config.CompanyId, config.ProductId);
			}

			return string.Empty;
		}

		/// <summary>
		/// Returns build target group for the <paramref name="target"/> specified.
		/// </summary>
		public static BuildTargetGroup GetBuildTargetGroup(BuildTarget target)
		{
			switch (target)
			{
				case BuildTarget.Android:
					return BuildTargetGroup.Android;

				case BuildTarget.iOS:
					return BuildTargetGroup.iOS;

				case BuildTarget.tvOS:
					return BuildTargetGroup.tvOS;

				case BuildTarget.WebGL:
					return BuildTargetGroup.WebGL;

				default:
					return BuildTargetGroup.Standalone;
			}
		}

		/// <summary>
		/// Returns active build target.
		/// </summary>
		public static BuildTargetGroup GetActiveBuildTargetGroup()
		{
			return GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
		}

#if UNITY_PURCHASING

		/// <summary>
		/// Returns default store for a build target.
		/// </summary>
		public static AppStore GetDefaultStore(BuildTarget target)
		{
			switch (target)
			{
				case BuildTarget.Android:
					return AppStore.GooglePlay;
				case BuildTarget.iOS:
				case BuildTarget.tvOS:
					return AppStore.AppleAppStore;
				case BuildTarget.StandaloneOSXIntel:
				case BuildTarget.StandaloneOSXIntel64:
				case BuildTarget.StandaloneOSXUniversal:
					return AppStore.MacAppStore;
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return AppStore.WinRT;
			}

			return AppStore.NotSpecified;
		}

#endif

		#endregion

		#region implementation
		#endregion
	}
}
