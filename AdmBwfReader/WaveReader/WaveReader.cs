using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace WaveReader
{
    [StructLayout(LayoutKind.Sequential)]
    public class FmtChunk
    {
        /// <summary>
        /// Chunk ID: "fmt "
        /// </summary>
        public string ckID;
        /// <summary>
        /// Chunk size: 16(リニアPCM), 18 or 40
        /// 以降のfmtチャンクのサイズ
        /// </summary>
        public int cksize;
        /// <summary>
        /// Format code : 非圧縮のリニアPCMは1(0x0100),A-lawは6などなど
        /// </summary>
        public short wFormatTag;
        /// <summary>
        /// Number of interleaved channels
        /// チャンネル数
        /// </summary>
        public short nChannels;
        /// <summary>
        /// sample rate
        /// サンプリング周波数
        /// 44.1kHzなど
        /// </summary>
        public int nSamplesPerSec;
        /// <summary>
        /// data rate
        /// 1秒当たりのバイト数平均
        /// bit数 * サンプリング周波数 * 1データあたりブロックサイズ(下記)
        /// 44.1kHz, 16bit,モノラルの場合
        /// 44100 * 2(byte) * 1 = 88200
        /// </summary>
        public int nAvgBytesPerSec;
        /// <summary>
        /// Data block size (bytes)
        /// (1サンプル当たりのビット数 / 8) * チャンネル数
        /// 16bitステレオなら16 / 8  * 2 = 4
        /// で、4となる
        /// </summary>
        public short nBlockAlign;
        /// <summary>
        /// Bits per sample
        /// 1サンプル当たりのビット数。ビットレート
        /// 8bit, 16bit...	
        /// </summary>
        public short wBitsPerSample;
        /// <summary>
        /// Size of the extension (0 or 22)
        /// 拡張パラメータサイズ
        /// </summary>
        public short cbSize;
        /// <summary>
        /// Number of valid bits
        /// </summary>
        public short wValidBitsPerSample;
        /// <summary>
        /// Speaker position mask
        /// 実際はDWORDだがInt32に格納
        /// 汎用機の世界では、32ビット程度をワード(WORD)とし、二倍の64ビット程度をダブルワード(DWORD)としていた。 パーソナルコンピューターの世界では、16ビット長をワード(WORD)とするため、その倍の32ビット長をダブルワード(DWORD)と呼んでいる。
        /// https://www.wdic.org/w/TECH/%E3%83%80%E3%83%96%E3%83%AB%E3%83%AF%E3%83%BC%E3%83%89#:~:text=%E6%B1%8E%E7%94%A8%E6%A9%9F%E3%81%AE%E4%B8%96%E7%95%8C%E3%81%A7%E3%81%AF,DWORD)%E3%81%A8%E5%91%BC%E3%82%93%E3%81%A7%E3%81%84%E3%82%8B%E3%80%82 
        /// </summary>
        public int dwChannelMask;
        /// <summary>
        /// GUID, including the data format code
        /// 16byte, unknown format
        /// string
        /// </summary>
        public string SubFormat_former;

        public FmtChunk Clone()
        {
            return (FmtChunk)MemberwiseClone();
        }
    }

    public class DataChunk
    {
        /// <summary>
        /// Chunk ID: "data"
        /// </summary>
        public string ckID;
        /// <summary>
        /// Chunk size: n
        /// </summary>
        public int cksize;
        /// <summary>
        /// Samples
        /// オーディオデータ本体
        /// リニアPCMの場合は時間順。1チャンネルごとに順に格納
        /// ステレオの場合左右左右…
        /// 8bitの場合は符号なし整数(0-255), 16bitの場合は符号付整数(-32768 - 32767)
        /// </summary>
        public byte[] sampledData;
        /// <summary>
        /// Padding byte if n is odd
        /// パディングバイト
        /// </summary>
        public byte padByte;
        /// <summary>
        /// 各チャンネルのバイト配列
        /// </summary>
        public List<byte[]> channelDataArray;
    }

    class WaveReader
    {
        public string RiffFourCC;
        public int DataSize;
        public string WaveFourCC;
        public FmtChunk impFmtChunk = new FmtChunk();
        public DataChunk impDataChunk = new DataChunk();


        public void Parse(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
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

                        if (chunk == "fmt ")
                        {
                            impFmtChunk.ckID = chunk;
                            impFmtChunk.cksize = BitConverter.ToInt16(binaryReader.ReadBytes(4), 0);
                            impFmtChunk.wFormatTag = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                            impFmtChunk.nChannels = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                            impFmtChunk.nSamplesPerSec = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                            impFmtChunk.nAvgBytesPerSec = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                            impFmtChunk.nBlockAlign = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                            impFmtChunk.wBitsPerSample = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);

                            switch (impFmtChunk.cksize)
                            {
                                case 16:
                                    break;
                                case 18:
                                    impFmtChunk.cbSize = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                                    break;
                                case 40:
                                    impFmtChunk.cbSize = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                                    impFmtChunk.wValidBitsPerSample = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                                    impFmtChunk.dwChannelMask = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                                    impFmtChunk.SubFormat_former = System.Text.Encoding.ASCII.GetString(binaryReader.ReadBytes(16));
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (chunk == "data")
                        {
                            impDataChunk.ckID = chunk;
                            impDataChunk.cksize = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                            impDataChunk.sampledData = binaryReader.ReadBytes(impDataChunk.cksize);

                            DistributeDataToChannel();
                            CalcuratePlayTime();
                            break;
                        }
                    }
                }
                catch
                {
                    stream.Close();
                }
            }
            ExportWav(filePath);
        }

        /// <summary>
        /// データチャンクをチャンネルごとに分割する
        /// </summary>
        private void DistributeDataToChannel()
        {
            // 1チャンネル当たりのバイト数
            int byteSizePerCh = impDataChunk.cksize / impFmtChunk.nChannels;

            // チャンネルごとにバイト列のサイズを確保
            impDataChunk.channelDataArray = new List<byte[]>();
            for (int i = 0; i < impFmtChunk.nChannels; i++)
            {
                impDataChunk.channelDataArray.Add(new byte[byteSizePerCh]);
            }

            int indexPerChannel = 0;
            int indexPerWholeData = 0;
            int bytesPerSample = impFmtChunk.wBitsPerSample / 8;
            while (indexPerWholeData < impDataChunk.cksize)
            {
                for (int chIndex = 0; chIndex < impFmtChunk.nChannels; chIndex++)
                {
                    for (int byteReadIdx = 0; byteReadIdx < bytesPerSample; byteReadIdx++)
                    {
                        impDataChunk.channelDataArray[chIndex][indexPerChannel + byteReadIdx] = impDataChunk.sampledData[indexPerWholeData];
                        indexPerWholeData++;
                    }
                }
                indexPerChannel += bytesPerSample;
            }
        }

        private void CalcuratePlayTime()
        {
            //int BytesPerChannel = dataChunk.cksize / fmtChunk.nChannels;
            float ByteSizePerSec = impFmtChunk.nSamplesPerSec * impFmtChunk.nBlockAlign;
            float timeSec = impDataChunk.cksize / ByteSizePerSec;
        }

        public void ExportWav(string path)
        {
            // copy metadata
            FmtChunk expFmtChunk = impFmtChunk.Clone();
            expFmtChunk.nChannels = 1;
            expFmtChunk.nAvgBytesPerSec /= impFmtChunk.nChannels;
            expFmtChunk.nBlockAlign /= impFmtChunk.nChannels;

            var dataSizePerCh = impDataChunk.cksize / impFmtChunk.nChannels;
            for (int channelIdx = 0; channelIdx < impFmtChunk.nChannels; channelIdx++)
            {
                string outputFullPath = path + "-" + channelIdx.ToString() + ".wav";
                // file Open
                using (FileStream fs = new FileStream(outputFullPath, FileMode.Create, FileAccess.Write))
                {
                    // RIFF Chunk
                    fs.Write(Encoding.ASCII.GetBytes(RiffFourCC));
                    int dataSize = DataSize - dataSizePerCh * (impFmtChunk.nChannels - 1);
                    fs.Write(BitConverter.GetBytes(dataSize));
                    fs.Write(Encoding.ASCII.GetBytes(WaveFourCC));

                    // fmtChunk
                    fs.Write(Encoding.ASCII.GetBytes(expFmtChunk.ckID));
                    fs.Write(BitConverter.GetBytes(expFmtChunk.cksize));
                    fs.Write(BitConverter.GetBytes(expFmtChunk.wFormatTag));
                    fs.Write(BitConverter.GetBytes(expFmtChunk.nChannels));
                    fs.Write(BitConverter.GetBytes(expFmtChunk.nSamplesPerSec));
                    fs.Write(BitConverter.GetBytes(expFmtChunk.nAvgBytesPerSec));
                    fs.Write(BitConverter.GetBytes(expFmtChunk.nBlockAlign));
                    fs.Write(BitConverter.GetBytes(expFmtChunk.wBitsPerSample));

                    switch (expFmtChunk.cksize)
                    {
                        case 16:
                            break;
                        case 18:
                            fs.Write(BitConverter.GetBytes(expFmtChunk.cbSize));
                            break;
                        case 40:
                            fs.Write(BitConverter.GetBytes(expFmtChunk.cbSize));
                            fs.Write(BitConverter.GetBytes(expFmtChunk.wValidBitsPerSample));
                            fs.Write(BitConverter.GetBytes(expFmtChunk.dwChannelMask));
                            fs.Write(Encoding.ASCII.GetBytes(expFmtChunk.SubFormat_former));
                            break;
                        default:
                            break;

                    }

                    // dataChunk
                    fs.Write(Encoding.ASCII.GetBytes(impDataChunk.ckID));
                    fs.Write(BitConverter.GetBytes(dataSizePerCh));
                    fs.Write(impDataChunk.channelDataArray[channelIdx]);

                }


            }


        }

    }
}
