using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Xsl;

namespace TrxerConsole
{
    internal static class Program
    {
        private const string xsltFileName = "Trxer.xslt";
        private const string outputFileExtension = ".html";

        private static void Main(string[] args)
        {
            if (args.Any() == false)
            {
                Console.WriteLine("No trx file. Usage: trxer.exe <filename>");
                return;
            }

            Console.WriteLine("Trx File\n{0}", args[0]);
            Transform(args[0], PrepareXsl());
        }

        private static void Transform(string fileName, XmlDocument xsl)
        {
            var x = new XslCompiledTransform(true);
            x.Load(xsl, new XsltSettings(true, true), null);

            Console.WriteLine("Transforming...");
            x.Transform(fileName, fileName + outputFileExtension);

            Console.WriteLine("Done transforming xml into html");
        }

        private static XmlDocument PrepareXsl()
        {
            var xdoc = new XmlDocument();
            
            Console.WriteLine("Loading xslt template...");
            using (var stream = StreamFromResource(xsltFileName))
                xdoc.Load(stream);
            MergeCss(xdoc);
            MergeJavaScript(xdoc);
            
            return xdoc;
        }

        private static void MergeJavaScript(XmlDocument xslDoc)
        {
            Console.WriteLine("Loading javascript...");

            var scriptElement = xslDoc.GetElementsByTagName("script")[0];
            var scriptSrcTag = scriptElement.Attributes["src"];
            var script = LoadTextFromResource(scriptSrcTag.Value);

            _ = scriptElement.Attributes.Remove(scriptSrcTag);
            scriptElement.InnerText = script;
        }

        /// <summary>
        /// Merges all css linked to page ito Trxer html report itself
        /// </summary>
        /// <param name="xslDoc">Xsl document</param>
        private static void MergeCss(XmlDocument xslDoc)
        {
            Console.WriteLine("Loading css...");

            var headNode = xslDoc.GetElementsByTagName("head")[0];
            var linkNodes = xslDoc.GetElementsByTagName("link");
            var toChangeList = linkNodes.Cast<XmlNode>().ToList();

            foreach (var xmlElement in toChangeList)
            {
                var styleElement = xslDoc.CreateElement("style");
                styleElement.InnerText = LoadTextFromResource(xmlElement.Attributes["href"].Value);
                headNode.ReplaceChild(styleElement, xmlElement);
            }
        }

        private static string LoadTextFromResource(string name)
        {
            using (var stream = StreamFromResource(name))
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        private static Stream StreamFromResource(string name) =>
            Assembly.GetExecutingAssembly().GetManifestResourceStream("trxer." + name);
    }
}
