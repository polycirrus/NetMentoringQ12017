using System;

namespace Shared
{
    [Serializable]
    public class DocumentChunk
    {
        public int ChunkNumber { get; set; }
        public int TotalChunks { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }
    }
}
