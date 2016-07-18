﻿using System;
using System.Collections.Generic;
using System.Linq;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using BackingCache = Villermen.RuneScapeCacheTools.Cache.RuneTek5.Cache;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    ///     RuneTek5 (RS3 in NXT & HTML) cache format.
    /// </summary>
    public class RuneTek5Cache : Cache
    {
        private BackingCache _backingCache;

        private string _cacheDirectory;

        public RuneTek5Cache()
        {
        }

        public RuneTek5Cache(IDataProcessor dataProcessor) : base(dataProcessor)
        {
        }

        public override string DefaultCacheDirectory
            => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jagexcache/runescape/LIVE/";

        public override string CacheDirectory
        {
            get { return _cacheDirectory; }
            set
            {
                _cacheDirectory = value;
                _backingCache = new BackingCache(new FileStore(_cacheDirectory));
            }
        }

        public override IEnumerable<int> GetIndexIds()
        {
            return Enumerable.Range(0, _backingCache.IndexCount);
        }

        public override IEnumerable<int> GetFileIds(int indexId)
        {
            return Enumerable.Range(0, _backingCache.GetFileCount(indexId));
        }

        public override IEnumerable<int> GetArchiveFileIds(int indexId, int archiveId)
        {
            throw new NotImplementedException();
        }

        public override byte[] GetFileData(int indexId, int fileId)
        {
            throw new NotImplementedException();
        }

        public override byte[] GetArchiveFileData(int indexId, int archiveId, int fileId)
        {
            throw new NotImplementedException();
        }
    }
}