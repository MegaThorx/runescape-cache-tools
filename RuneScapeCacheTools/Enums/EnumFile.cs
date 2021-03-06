﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Enums
{
    public class EnumFile : CacheFile, IEnumerable<KeyValuePair<int, object>>
    {
        public int DefaultInteger { get; set; }
        public string DefaultString { get; set; } = "null";

        public ScriptVarType KeyType { get; set; }
        public Dictionary<int, object> Values { get; set; }
        public ScriptVarType ValueType { get; set; }

        public object this[int key] => this.Values[key];

        public bool ContainsKey(int key)
        {
            return this.Values.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<int, object>> GetEnumerator()
        {
            return this.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        protected override void Decode(byte[] data)
        {
            var dataReader = new BinaryReader(new MemoryStream(data));
            var ended = false;

            while ((dataReader.BaseStream.Position < dataReader.BaseStream.Length) && !ended)
            {
                var opcode = (EnumOpcode)dataReader.ReadByte();

                switch (opcode)
                {
                    case EnumOpcode.CharKeyType:
                        this.KeyType = ScriptVarType.FromValue(dataReader.ReadAwkwardChar());
                        break;

                    case EnumOpcode.CharValueType:
                        this.ValueType = ScriptVarType.FromValue(dataReader.ReadAwkwardChar());
                        break;

                    case EnumOpcode.DefaultString:
                        this.DefaultString = dataReader.ReadNullTerminatedString();
                        break;

                    case EnumOpcode.DefaultInteger:
                        this.DefaultInteger = dataReader.ReadInt32BigEndian();
                        break;

                    case EnumOpcode.StringDataDictionary:
                    case EnumOpcode.IntegerDataDictionary:
                        var count = dataReader.ReadUInt16BigEndian();
                        this.Values = new Dictionary<int, object>(count);

                        for (var i = 0; i < count; i++)
                        {
                            var key = dataReader.ReadInt32BigEndian();
                            object value;

                            if (opcode == EnumOpcode.StringDataDictionary)
                            {
                                value = dataReader.ReadNullTerminatedString();
                            }
                            else
                            {
                                value = dataReader.ReadInt32BigEndian();
                            }

                            this.Values[key] = value;
                        }
                        break;

                    case EnumOpcode.StringDataArray:
                    case EnumOpcode.IntegerDataArray:
                        var max = dataReader.ReadUInt16BigEndian();
                        count = dataReader.ReadUInt16BigEndian();
                        this.Values = new Dictionary<int, object>(count);

                        for (var i = 0; i < count; i++)
                        {
                            var key = dataReader.ReadUInt16BigEndian();
                            if (opcode == EnumOpcode.StringDataArray)
                            {
                                this.Values[key] = dataReader.ReadNullTerminatedString();
                            }
                            else
                            {
                                this.Values[key] = dataReader.ReadInt32BigEndian();
                            }
                        }
                        break;

                    case EnumOpcode.ByteKeyType:
                        this.KeyType = ScriptVarType.FromValue(dataReader.ReadAwkwardShort());
                        break;

                    case EnumOpcode.ByteValueType:
                        this.ValueType = ScriptVarType.FromValue(dataReader.ReadAwkwardShort());
                        break;

                    case EnumOpcode.End:
                        ended = true;
                        break;

                    default:
                        throw new DecodeException($"Invalid enum opcode \"{opcode}\".");
                }
            }

            if (this.KeyType == null)
            {
                throw new DecodeException("Enum data does not contain a key type.");
            }

            if (this.ValueType == null)
            {
                throw new DecodeException("Enum data does not contain a value type.");
            }

            if (this.Values == null)
            {
                throw new DecodeException("Enum does not contain any values.");
            }
        }

        protected override byte[] Encode()
        {
            throw new NotImplementedException("Encoding of enum files is not yet implemented.");
        }
    }
}