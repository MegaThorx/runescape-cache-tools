﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
    /// <summary>
    ///     A file store holds multiple files inside a "virtual" file system made up of several index files and a single data
    ///     file.
    /// </summary>
    /// <author>Graham</author>
    /// <author>`Discardedx2</author>
    /// <author>Villermen</author>
    /// <todo>Reading of meta data.</todo>
    public class FileStore
    {
        /// <summary>
        ///     Opens the file store in the specified directory.
        /// </summary>
        /// <param name="cacheDirectory">The directory containing the index and data files.</param>
        /// <exception cref="FileNotFoundException">If any of the main_file_cache.* files could not be found.</exception>
        public FileStore(string cacheDirectory)
        {
            var dataFile = Path.Combine(cacheDirectory, "main_file_cache.dat2");

            if (!File.Exists(dataFile))
            {
                throw new FileNotFoundException("Cache data file does not exist.");
            }

            DataStream = File.Open(dataFile, FileMode.Open);

            for (var indexId = 0; indexId < 254; indexId++)
            {
                var indexFile = Path.Combine(cacheDirectory + "main_file_cache.idx" + indexId);

                if (!File.Exists(indexFile))
                {
                    continue;
                }

                IndexStreams.Add(indexId, File.Open(indexFile, FileMode.Open));
            }

            if (IndexStreams.Count == 0)
            {
                throw new FileNotFoundException("No index files found.");
            }

            var metaFile = Path.Combine(cacheDirectory + "main_file_cache.idx255");

            if (!File.Exists(metaFile))
            {
                throw new FileNotFoundException("Meta index file does not exist.");
            }

            MetaStream = File.Open(metaFile, FileMode.Open);
        }

        private Stream DataStream { get; }
        private IDictionary<int, Stream> IndexStreams { get; } = new Dictionary<int, Stream>();
        private Stream MetaStream { get; }

        /// <summary>
        ///     The number of index files, not including the meta index file.
        /// </summary>
        public int IndexCount => IndexStreams.Count;

        /// <summary>
        ///     Returns the number of files of the specified type.
        /// </summary>
        /// <param name="indexId"></param>
        /// <returns></returns>
        public int GetFileCount(int indexId)
        {
            if (!IndexStreams.ContainsKey(indexId))
            {
                throw new CacheException("Invalid index specified.");
            }

            return (int) (IndexStreams[indexId].Length / Index.Length);
        }

        public byte[] GetFileData(int indexId, int fileId)
        {
            if (!IndexStreams.ContainsKey(indexId))
            {
                throw new CacheException("Invalid index specified.");
            }

            var indexReader = new BinaryReader(IndexStreams[indexId]);

            var indexPosition = (long) fileId * Index.Length;

            if (indexPosition < 0 || indexPosition >= indexReader.BaseStream.Length)
            {
                throw new FileNotFoundException("Given file does not exist.");
            }

            indexReader.BaseStream.Position = indexPosition;

            var reversedIndexBytes = indexReader.ReadBytes(Index.Length);
            Array.Reverse(reversedIndexBytes);

            var index = new Index(reversedIndexBytes);

            var chunkId = 0;
            var remaining = index.Size;
            var dataPosition = (long) index.Sector * Sector.Length;

            IEnumerable<byte> data = new byte[0];
            do
            {
                DataStream.Position = dataPosition;

                var extended = fileId > 65535;

                var sectorData = new byte[Sector.Length];
                Array.Reverse(sectorData);
                var sector = new Sector(sectorData, extended);

                if (sector.IndexId != indexId)
                {
                    throw new CacheException("Sector index id mismatch.");
                }

                if (sector.FileId != fileId)
                {
                    throw new CacheException("Sector file id mismatch.");
                }

                if (sector.ChunkId != chunkId)
                {
                    throw new CacheException("Sector index mismatch.");
                }

                data = data.Concat(sector.Data);
                remaining -= extended ? Sector.ExtendedDataLength : Sector.DataLength;

                dataPosition = sector.NextSectorId * Sector.Length;
                chunkId++;
            }
            while (remaining > 0);

            return data.Reverse().ToArray();
        }

        public void WriteFile(int indexId, int fileId, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void WriteFile(int indexId, int fileId, byte[] data, bool overwrite)
        {
            throw new NotImplementedException();
        }
    }
}