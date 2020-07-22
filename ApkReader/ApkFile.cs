﻿using System;
using System.Collections.Generic;
using System.IO;
using AlphaOmega.Debug.Manifest;
using ICSharpCode.SharpZipLib.Zip;

namespace AlphaOmega.Debug
{
	/// <summary>Android Package description</summary>
	public class ApkFile : IDisposable
	{
		private ZipFile _apk;
		private AxmlFile _xml;
		private ArscFile _res;
		private AndroidManifest _androidManifest;
		private Boolean _isXmlExists = true;
		private Boolean _isArscExists = true;

		/// <summary>Raw AndroidManifest.xml</summary>
		public AxmlFile XmlFile
		{
			get
			{
				if(this._xml == null && _isXmlExists)
				{
					const String FileName = "AndroidManifest.xml";
					Byte[] payload = this.GetFile(FileName);
					if(payload == null)
						_isXmlExists = false;
					else
						this._xml = new AxmlFile(StreamLoader.FromMemory(payload, FileName));
				}
				return this._xml;
			}
		}

		/// <summary>Raw Resources.arsc</summary>
		public ArscFile Resources
		{
			get
			{
				if(this._res == null && _isArscExists)
				{
					Byte[] payload = this.GetFile("resources.arsc");
					if(payload == null)
						_isArscExists = false;
					else
						this._res = new ArscFile(payload);
				}
				return this._res;
			}
		}

		/// <summary>Проверка на валидность Android Package</summary>
		public Boolean IsValid
		{
			get
			{
				if(this.XmlFile != null && this.Resources != null)
					return true;
				foreach(String filePath in this.GetHeaderFiles())
					if(Path.GetExtension(filePath).ToLowerInvariant() == ".apk")
						return true;

				return false;
			}
		}

		/// <summary>Android manifest</summary>
		public AndroidManifest AndroidManifest
		{
			get
			{
				return this._androidManifest == null
					? this._androidManifest = AndroidManifest.Load(this.XmlFile, this.Resources)
					: this._androidManifest;
			}
		}

		/// <summary>Specifies a system permission that the user must grant in order for the app to operate correctly</summary>
		public IEnumerable<String> UsesPermission
		{
			get
			{
				if(this.AndroidManifest != null)
					foreach(ApkUsesPermission permission in this.AndroidManifest.UsesPermission)
						yield return permission.Name;
			}
		}

		/// <summary>Specifies a single hardware or software feature used by the application, as a descriptor string</summary>
		public IEnumerable<String> UsesFeature
		{
			get
			{
				if(this.AndroidManifest != null)
					foreach(ApkUsesFeature feature in this.AndroidManifest.UsesFeature)
						yield return feature.Name;
			}
		}

		/// <summary>The name of the class that implements the broadcast receiver, a subclass of BroadcastReceiver</summary>
		public IEnumerable<String> Receiver
		{
			get
			{
				if(this.AndroidManifest != null)
					foreach(ApkReceiver receiver in this.AndroidManifest.Application.Reciever)
						yield return receiver.Name;
			}
		}

		/// <summary>The name of the class that implements the content provider, a subclass of ContentProvider</summary>
		public IEnumerable<String> Provider
		{
			get
			{
				if(this.AndroidManifest != null)
					foreach(ApkProvider provider in this.AndroidManifest.Application.Provider)
						yield return provider.Name;
			}
		}

		/// <summary>The name of the Service subclass that implements the service</summary>
		public IEnumerable<String> Service
		{
			get
			{
				if(this.AndroidManifest != null)
					foreach(ApkService service in this.AndroidManifest.Application.Service)
						yield return service.Name;
			}
		}

		/// <summary>Create instance of android package description</summary>
		/// <param name="filePath">Physical file path</param>
		public ApkFile(String filePath)
		{
			if(String.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("filePath");
			if(!File.Exists(filePath))
				throw new FileNotFoundException("File not found", filePath);

			this._apk = new ZipFile(filePath);
		}

		/// <summary>Create instance of android package desctiption</summary>
		/// <param name="buffer">Raw file bytes</param>
		public ApkFile(Byte[] buffer)
		{
			if(buffer == null || buffer.Length == 0)
				throw new ArgumentNullException("buffer");

			this._apk = new ZipFile(new MemoryStream(buffer));
		}

		/// <summary>Create instance of android package desctiption</summary>
		/// <param name="stream">File stream</param>
		public ApkFile(Stream stream)
		{
			if(stream == null)
				throw new ArgumentNullException("stream");

			this._apk = new ZipFile(stream);
		}

		/// <summary>GetPackage contents</summary>
		/// <returns></returns>
		public IEnumerable<String> GetFiles()
		{
			foreach(ZipEntry entry in this._apk)
				if(entry.IsFile)
					yield return entry.Name;
		}

		/// <summary>Get header files</summary>
		/// <returns>Header APK files</returns>
		public IEnumerable<String> GetHeaderFiles()
		{
			foreach(String filePath in this.GetFiles())
				switch(Path.GetExtension(filePath).ToLowerInvariant())
				{
				case ".apk":
				case ".xapk":
				case ".dex":
				case ".arsc":
					yield return filePath;
					break;
				case ".xml":
					yield return filePath;
					break;
				}
		}

		/// <summary>Получить файл в виде потока байт</summary>
		/// <param name="filePath">Путь к файлу в архиве</param>
		/// <returns>Поток файла из архива</returns>
		public Stream GetFileStream(String filePath)
		{
			ZipEntry entry = this._apk.GetEntry(filePath);
			if(entry == null)
				return null;

			return this._apk.GetInputStream(entry);
		}

		/// <summary>Получить файл в виде массива байт</summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public Byte[] GetFile(String fileName)
		{
			ZipEntry entry = this._apk.GetEntry(fileName);
			if(entry == null)
				return null;

			using(BinaryReader reader = new BinaryReader(this._apk.GetInputStream(entry)))
				return reader.ReadBytes((Int32)entry.Size);
		}

		/// <summary>Clears base apk file</summary>
		public void Dispose()
		{
			if(this._apk != null)
			{
				this._apk.Close();
				this._apk = null;
			}
		}
	}
}