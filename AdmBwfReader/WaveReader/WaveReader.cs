using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WaveReader
{
    public struct FmtChunk
    {
        ///
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
    }

    public struct DataChunk
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
        public byte[,] channelDataArray;
    }

    class WaveReader
    {
        public string RiffFourCC;
        public int DataSize;
        public string WaveFourCC;
        public FmtChunk fmtChunk;
        public DataChunk dataChunk;


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
                            fmtChunk.ckID = chunk;
                            fmtChunk.cksize = BitConverter.ToInt16(binaryReader.ReadBytes(4), 0);
                            fmtChunk.wFormatTag = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                            fmtChunk.nChannels = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                            fmtChunk.nSamplesPerSec = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                            fmtChunk.nAvgBytesPerSec = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                            fmtChunk.nBlockAlign = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                            fmtChunk.wBitsPerSample = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);

                            switch (fmtChunk.cksize)
                            {
                                case 16:
                                    break;
                                case 18:
                                    fmtChunk.cbSize = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                                    break;
                                case 40:
                                    fmtChunk.cbSize = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                                    fmtChunk.wValidBitsPerSample = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                                    fmtChunk.dwChannelMask = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                                    fmtChunk.SubFormat_former = System.Text.Encoding.ASCII.GetString(binaryReader.ReadBytes(16));
                                    break;
                                default:
                                    break;
                            }
                        }

                        if(chunk == "data")
                        {
                            dataChunk.ckID = chunk;
                            dataChunk.cksize = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                            dataChunk.sampledData = binaryReader.ReadBytes(dataChunk.cksize);

                            DistributeDataToChannel();
                            CalcuratePlayTime();
                        }
                    }
                }
                catch
                {
                    stream.Close();
                }
            }
        }

        /// <summary>
        /// データチャンクをチャンネルごとに分割する
        /// </summary>
        private void DistributeDataToChannel()
        {
            // 1チャンネル当たりのバイト数
            int byteSizePerCh = dataChunk.cksize / fmtChunk.nChannels;

            // チャンネルごとにバイト列のサイズを確保
            dataChunk.channelDataArray = new byte[fmtChunk.nChannels, byteSizePerCh];

            int indexPerChannel = 0;
            int indexPerWholeData = 0;
            int bytesPerSample = fmtChunk.wBitsPerSample / 8;
            while (indexPerWholeData < dataChunk.cksize)
            {
                for (int chIndex = 0; chIndex < fmtChunk.nChannels; chIndex++)
                {
                    for(int byteReadIdx=0; byteReadIdx< bytesPerSample; byteReadIdx++)
                    {
                        dataChunk.channelDataArray[chIndex,indexPerChannel + byteReadIdx] = dataChunk.sampledData[indexPerWholeData];
                        indexPerWholeData++;
                    }
                }
                indexPerChannel += bytesPerSample;
            }
        }

        private void CalcuratePlayTime()
        {
            //int BytesPerChannel = dataChunk.cksize / fmtChunk.nChannels;
            float ByteSizePerSec = fmtChunk.nSamplesPerSec * fmtChunk.nBlockAlign;
            float timeSec = dataChunk.cksize / ByteSizePerSec;
        }
    }
}
