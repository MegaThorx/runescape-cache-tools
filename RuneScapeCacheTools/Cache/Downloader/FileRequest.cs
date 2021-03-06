﻿using System.IO;
using System.Threading.Tasks;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public abstract class FileRequest
    {
        protected FileRequest(Index index, int fileId, CacheFileInfo cacheFileInfo)
        {
            this.Index = index;
            this.FileId = fileId;
            this.CacheFileInfo = cacheFileInfo;
        }

        public CacheFileInfo CacheFileInfo { get; }

        public MemoryStream DataStream { get; } = new MemoryStream();

        public int FileId { get; }

        public Index Index { get; }

        private TaskCompletionSource<byte[]> CompletionSource { get; } = new TaskCompletionSource<byte[]>();

        public virtual void Write(byte[] data)
        {
            this.DataStream.Write(data, 0, data.Length);
        }

        public void Complete()
        {
            this.CompletionSource.SetResult(this.DataStream.ToArray());
        }

        public byte[] WaitForCompletion()
        {
            return this.CompletionSource.Task.Result;
        }
    }
}