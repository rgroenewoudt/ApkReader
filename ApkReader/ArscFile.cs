﻿using System;
using System.Collections.Generic;
using System.IO;
using AlphaOmega.Debug.Arsc;

namespace AlphaOmega.Debug
{
	/// <summary>Android resources.arsc file reader</summary>
	public class ArscFile
	{
		private const Int64 HEADER_START = 0;
		private const Int16 RES_STRING_POOL_TYPE = 0x0001;
		private const Int16 RES_TABLE_PACKAGE_TYPE = 0x0200;

		private StringPool _valueStringPool;
		private ArscApi.ResTable_Header _header;
		private List<ResourcePackage> _package = new List<ResourcePackage>();

		private Dictionary<Int32, List<ResourceRow>> _resourceMap = new Dictionary<Int32, List<ResourceRow>>();

		/// <summary>Resource table</summary>
		public Dictionary<Int32, List<ResourceRow>> ResourceMap { get { return this._resourceMap; } }

		/// <summary>Resource file header</summary>
		public ArscApi.ResTable_Header Header { get { return this._header; } }

		/// <summary>Value string pool</summary>
		public StringPool ValueStringPool { get { return this._valueStringPool; } }

		/// <summary>Create instance of resource file decompilator</summary>
		/// <param name="stream">Payload</param>
		public ArscFile(Stream stream)
		{
			if(stream == null)
				throw new ArgumentNullException("stream");

			using(BinaryReader reader = new BinaryReader(stream))
				this.Initialize(reader);
		}

		/// <summary>Create instance of resource file decompilator</summary>
		/// <param name="buffer">Payload</param>
		public ArscFile(Byte[] buffer)
		{
			if(buffer == null)
				throw new ArgumentNullException("resourceBytes");

			using(MemoryStream stream = new MemoryStream(buffer))
			using(BinaryReader reader = new BinaryReader(stream))
				this.Initialize(reader);
		}

		private void Initialize(BinaryReader reader)
		{
			this._header = Utils.PtrToStructure<ArscApi.ResTable_Header>(reader);

			if(!this._header.IsValid)
				throw new ArgumentException("No RES_TABLE_TYPE found!");
			if(this._header.header.size != reader.BaseStream.Length)
				throw new OverflowException("The buffer size not matches to the resource table size.");

			while(true)
			{
				Int64 pos = reader.BaseStream.Position;
				ArscApi.ResChunk_Header chunk = Utils.PtrToStructure<ArscApi.ResChunk_Header>(reader);

				reader.BaseStream.Seek(pos, SeekOrigin.Begin);
				Byte[] buffer = reader.ReadBytes(chunk.size);

				switch(chunk.type)
				{
				case RES_STRING_POOL_TYPE:
					if(this._valueStringPool != null)
						throw new InvalidOperationException("String pool already defined");

					this._valueStringPool = new StringPool(buffer);
					break;
				case RES_TABLE_PACKAGE_TYPE:
					this._package.Add(new ResourcePackage(buffer));
					break;
				default:
					throw new InvalidOperationException("Unsupported Type");
				}

				reader.BaseStream.Seek(pos + (Int64)chunk.size, SeekOrigin.Begin);
				if(reader.BaseStream.Position == reader.BaseStream.Length)
					break;
			}

			if(this._header.packageCount != this._package.Count)
				throw new InvalidOperationException(String.Format("Expecting {0} packages. Collected {1} packages", this._header.packageCount, this._package.Count));

			//====
			this.CreateResourceMap();
		}

		private void CreateResourceMap()
		{
			foreach(var p in this._package)
				foreach(var t in p.TypeTable)
				{
					Dictionary<Int32, ArscApi.Res_value> refKeys1 = new Dictionary<Int32, ArscApi.Res_value>();
					foreach(var simple in t.Simple)
					{
						ResourceRow row = null;
						switch(simple.Value.Value.dataType)
						{
						case ArscApi.DATA_TYPE.STRING:
							row = new ResourceRow(simple.Value.Value, this._valueStringPool.Strings[simple.Value.Value.data]);
							break;
						case ArscApi.DATA_TYPE.REFERENCE:
							refKeys1.Add(simple.Key, simple.Value.Value);
							break;
						default:
							row = new ResourceRow(simple.Value.Value, null);
							break;
						}

						if(row != null)
							this.AddToMap(simple.Key, row);
					}

					List<ResourceRow> values;
					foreach(var refResource in refKeys1)
						if(this._resourceMap.TryGetValue(refResource.Value.data, out values))
							this.AddToMap(refResource.Key, values.ToArray());
				}
		}

		private void AddToMap(Int32 resId, params ResourceRow[] values)
		{
			Utils.AppendToDictionary<Int32, List<ResourceRow>>(this._resourceMap, resId, delegate(List<ResourceRow> list) { list.AddRange(values); });
			/*List<ResourceRow> valueList;
			if(!this._resourceMap.TryGetValue(resId, out valueList))
				this._resourceMap.Add(resId, valueList = new List<ResourceRow>());

			valueList.AddRange(values);
			this._resourceMap[resId] = valueList;*/
		}
	}
}