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
using UnityEditor.Purchasing;
using UnityEngine;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

namespace UnityFx.BuildTools.Editor
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
		/// Deserializes the class instance from XML file.
		/// </summary>
		public static BuildConfig FromXml(Type configType, string path)
		{
			if (configType == null)
			{
				configType = typeof(BuildConfig);
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
		public static BuildConfig FromEnvironment(Type configType, string namePrefix)
		{
			if (configType == null)
			{
				configType = typeof(BuildConfig);
			}

			if (namePrefix == null)
			{
				namePrefix = string.Empty;
			}

			// TODO
			return new BuildConfig();
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
		/// <param name="fallbackConfig"></param>
		/// <param name="result"></param>
		protected virtual void Combine(IBuildConfig fallbackConfig, BuildConfig result)
		{
			// Defines.
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

			// App/version configs.
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
		/// Gets or sets scripting define symbols.
		/// </summary>
		[XmlArrayItem("Define")]
		public string[] Defines { get; set; }

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
			// TODO
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

		private void CombineVersionConfig(IVersionConfig fallbackConfig, BuildConfig result)
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

		private void CombineAppConfig(IAppConfig fallbackConfig, BuildConfig result)
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
