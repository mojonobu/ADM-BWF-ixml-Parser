using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AdmBwfReader
{
    class IxmlParser
    {
        static void Main(string[] args)
        {
            // ファイルオープン
            string filePath = args[0];
            Console.WriteLine(filePath);

            var data = File.ReadAllBytes(filePath);
            string str = null;

            int fullLength = data.Length;
            int stepSize = 1000000;
            int parseSize = 0;

            while (parseSize < fullLength)
            {
                parseSize += stepSize;
                byte[] parseByteArray = new byte[parseSize];
                Array.Copy(data, fullLength - parseSize, parseByteArray, 0, parseSize);
                try
                {
                    str = Encoding.UTF8.GetString(parseByteArray);
                    if (str.Contains("encoding"))
                    {
                        Console.WriteLine("encoding found");
                    }

                    XDocument doc = XDocument.Parse(str);
                    Console.WriteLine(doc);

                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("xml not found");
                    // Hexadecimal value 0x00 is a invalid character　が出る

                }
            }

            //int size = 1000;
            //BinaryReader reader = null;
            //using (reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            //{
            //    var data = reader.ReadBytes(size);
            //    string str = Encoding.ASCII.GetString(data);

            //}
            //// xmlをパース

        }
    }
}
