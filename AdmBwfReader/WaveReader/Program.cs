using System;

namespace WaveReader
{
    class Program
    {
        static void Main(string[] args)
        {
            // File Read
            // ファイルオープン
            string filePath = args[0];
            Console.WriteLine(filePath);

            WaveReader waveReader = new WaveReader();
            waveReader.Parse(filePath);



        }
    }
}
