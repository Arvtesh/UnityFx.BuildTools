// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

namespace UnityFx.BuildTools
{
	/// <summary>
	/// A generic app configuration.
	/// </summary>
	public interface IAppConfig
	{
		/// <summary>
		/// Gets the product identifier.
		/// </summary>
		/// <seealso cref="ProductName"/>
		string ProductId { get; }

		/// <summary>
		/// Gets the user-friendly product name.
		/// </summary>
		/// <seealso cref="ProductId"/>
		/// <seealso cref="Application.productName"/>
		string ProductName { get; }

		/// <summary>
		/// Gets the company identifier.
		/// </summary>
		/// <seealso cref="CompanyName"/>
		string CompanyId { get; }

		/// <summary>
		/// Gets the user-friendly company name.
		/// </summary>
		/// <seealso cref="CompanyId"/>
		/// <seealso cref="Application.companyName"/>
		string CompanyName { get; }

		/// <summary>
		/// Gets app bundle identifier.
		/// </summary>
		/// <seealso cref="Application.identifier"/>
		string BundleIdentifier { get; }

#if UNITY_PURCHASING

		/// <summary>
		/// Gets app store identifier.
		/// </summary>
		AppStore Store { get; }

#endif
	}
}
