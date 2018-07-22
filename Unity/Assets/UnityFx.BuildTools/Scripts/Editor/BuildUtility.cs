// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using UnityEditor.Purchasing;
using UnityEngine;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

namespace UnityFx.BuildTools.Editor
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
			ApplyAppConfig(config);
			ApplyVersionConfig(config);

#if UNITY_PURCHASING

			if (config.Store != AppStore.NotSpecified)
			{
				UnityPurchasingEditor.TargetAndroidStore(config.Store);
			}

#endif

			PlayerSettings.SetScriptingDefineSymbolsForGroup(GetActiveBuildTargetGroup(), string.Join(";", config.Defines));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public static void ApplyVersionConfig(IVersionConfig config)
		{
			// Application.identifier
			if (!string.IsNullOrEmpty(config.BundleVersion))
			{
				PlayerSettings.bundleVersion = config.BundleVersion;
			}
			else
			{
				throw new InvalidOperationException();
			}

			// BuildNumber
			if (config.BuildNumber > 0)
			{
				PlayerSettings.iOS.buildNumber = config.BuildNumber.ToString(NumberFormatInfo.InvariantInfo);
				PlayerSettings.macOS.buildNumber = config.BuildNumber.ToString(NumberFormatInfo.InvariantInfo);
				PlayerSettings.tvOS.buildNumber = config.BuildNumber.ToString(NumberFormatInfo.InvariantInfo);
			}
			else
			{
				throw new InvalidOperationException();
			}

			// BundleVersionCode
			if (config.BundleVersionCode > 0)
			{
				PlayerSettings.Android.bundleVersionCode = config.BundleVersionCode;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public static void ApplyAppConfig(IAppConfig config)
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
			else
			{
				throw new InvalidOperationException();
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
			else
			{
				throw new InvalidOperationException();
			}

			// Application.identifier
			if (!string.IsNullOrEmpty(config.BundleIdentifier))
			{
				PlayerSettings.SetApplicationIdentifier(GetActiveBuildTargetGroup(), config.BundleIdentifier);
			}
			else if (!string.IsNullOrEmpty(config.ProductId) && !string.IsNullOrEmpty(config.CompanyId))
			{
				PlayerSettings.SetApplicationIdentifier(GetActiveBuildTargetGroup(), string.Format("com.{0}.{1}", config.CompanyId, config.ProductId));
			}
			else
			{
				throw new InvalidOperationException();
			}
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

		#endregion

		#region implementation
		#endregion
	}
}
