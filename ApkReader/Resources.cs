﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace AlphaOmega.Debug
{
	/// <summary>Constant APK constants description</summary>
	public static class Resources
	{
		private static ResourceManager _permission;
		private static ResourceManager _features;
		private static ResourceManager _intent;

		private static ResourceManager Permission
		{
			get
			{
				return _permission==null
					? _permission = new ResourceManager("AlphaOmega.Debug.Permission", typeof(Resources).Assembly)
					: _permission;
			}
		}

		private static ResourceManager Features
		{
			get
			{
				return _features == null
					? _features = new ResourceManager("AlphaOmega.Debug.Features", typeof(Resources).Assembly)
					: _features;
			}
		}

		private static ResourceManager Intent
		{
			get
			{
				return _intent == null
					? _intent = new ResourceManager("AlphaOmega.Debug.Intent", typeof(Resources).Assembly)
					: _intent;
			}
		}

		/// <summary>Gets Android permission description</summary>
		/// <param name="name">Permission name</param>
		/// <returns>Android permission description or null</returns>
		public static String GetPermission(String name)
		{
			return Resources.Permission.GetString(name);
		}

		/// <summary>Gets all described permissions</summary>
		/// <returns>Android permissions</returns>
		public static IEnumerable<String> GetPermissions()
		{
			ResourceSet set = Resources.Permission.GetResourceSet(CultureInfo.InvariantCulture, true, false);
			foreach(DictionaryEntry item in set)
				yield return (String)item.Key;
		}

		/// <summary>Gets Android feature description</summary>
		/// <param name="name">Feature name</param>
		/// <returns>Android feature description or null</returns>
		public static String GetFeatures(String name)
		{
			return Resources.Features.GetString(name);
		}

		/// <summary>Gets all described features</summary>
		/// <returns>Android features</returns>
		public static IEnumerable<String> GetFeatures()
		{
			ResourceSet set = Resources.Features.GetResourceSet(CultureInfo.InvariantCulture, true, false);
			foreach(DictionaryEntry item in set)
				yield return (String)item.Key;
		}

		/// <summary>Gets Android intent description</summary>
		/// <param name="name">Intent name</param>
		/// <returns>Android intent description or null</returns>
		public static String GetIntent(String name)
		{
			return Resources.Intent.GetString(name);
		}

		/// <summary>Gets all described intents</summary>
		/// <returns>Android intents</returns>
		public static IEnumerable<String> GetIntents()
		{
			ResourceSet set = Resources.Intent.GetResourceSet(CultureInfo.InvariantCulture, true, false);
			foreach(DictionaryEntry item in set)
				yield return (String)item.Key;
		}
	}
}