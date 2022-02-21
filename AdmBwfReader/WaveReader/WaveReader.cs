using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WaveReader
{
    class WaveReader
    {
        public string RiffFourCC;
        public int DataSize;
        public string WaveFourCC;
        public string fmtChunk;

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
                }
                catch
                {
                    stream.Close();
                }
            }
        }
    }
}
