using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
            Regex regex = new Regex("<\\?xml version.+</ebuCoreMain>", RegexOptions.Singleline);


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
                    if (str.Contains("version"))
                    {
                        Console.WriteLine("encoding found");
                    }
                    Match xmlMatch = regex.Match(str);
                    XDocument doc = XDocument.Parse(xmlMatch.Value);

                    ExportXML(doc);
                    //Console.WriteLine(doc.ToString());

                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("xml not found");
                }
            }

        }

        private static void ExportXML(XDocument doc)
        {
            File.WriteAllText(@"C:\proj\MyProject\resources\iXML.xml",doc.ToString());
        }
    }
}
