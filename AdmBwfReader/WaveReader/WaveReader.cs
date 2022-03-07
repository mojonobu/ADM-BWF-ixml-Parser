using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WaveReader
{
    public struct FmtChunk
    {
        /// <summary>
        /// Chunk ID: "fmt "
        /// </summary>
        public string ckID;
        /// <summary>
        /// Chunk size: 16, 18 or 40 以降のfmtチャンクのサイズ？
        /// </summary>
        public int cksize;
        /// <summary>
        /// Format code
        /// </summary>
        public short wFormatTag;
        /// <summary>
        /// Number of interleaved channels
        /// </summary>
        public short nChannels;
        /// <summary>
        /// sample rate
        /// </summary>
        public int nSamplesPerSec;
        /// <summary>
        /// data rate
        /// </summary>
        public int nAvgBytesPerSec;
        /// <summary>
        /// Data block size (bytes)
        /// </summary>
        public short nBlockAlign;
        /// <summary>
        /// 	Bits per sample
        /// </summary>
        public short wBitsPerSample;

    }
    class WaveReader
    {
        public string RiffFourCC;
        public int DataSize;
        public string WaveFourCC;
        public FmtChunk fmtChunk;

        public void Parse(string filePath)
        {
            using (FileStream stream = new FileStream(filePath,FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    var binaryReader = new BinaryReader(stream);
                    RiffFourCC = System.Text.Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
                    DataSize = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                    WaveFourCC = System.Text.Encoding.ASCII.GetString(binaryReader.ReadBytes(4));

                    while (true)
                    {
                        var chunk = System.Text.Encoding.ASCII.GetString(binaryReader.ReadBytes(4));

                        if(chunk == "fmt ")
                        {
                            fmtChunk.ckID = chunk;
                            fmtChunk.cksize = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                            fmtChunk.wFormatTag = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                            fmtChunk.nChannels = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                            fmtChunk.nSamplesPerSec = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                            fmtChunk.nAvgBytesPerSec = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                            fmtChunk.nBlockAlign = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                            fmtChunk.wBitsPerSample = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                        }
                    }
                }
                catch
                {
                    stream.Close();
                }
            }
        }
    }
}
