using ner.Cal3D.Core;
using System;
using System.Text;
using System.Xml;

namespace ner.Cal3D.XRF
{
    /// <summary>
    /// Represents an XRF file, the Cal3D format for material naming and mapping.
    /// Usage: Call static methods to parse. For example XRF.Parse(byte[])
    /// </summary>
    public class XRF
    {
        public static readonly string HEADER1 = "<HEADER MAGIC=\"XRF\" VERSION=\"919\" />";
        public static readonly string HEADER2 = "<HEADER MAGIC=\"XRF\" VERSION=\"918\" />";
        public static readonly string MaterialFormat = "<MATERIAL NUMMAPS=\"{0}\">\r\n{1}</MATERIAL>";

        public XCal3DByte4 Ambient { get; set; }
        public XCal3DByte4 Diffuse { get; set; }
        public XCal3DByte4 Specular { get; set; }
        public float Shininess { get; set; }
        public BaseCal3DCollection<XRFMap> Maps { get; set; }

        public XRF()
        {
            this.Maps = new BaseCal3DCollection<XRFMap>();
        }

        public static XRF ParseXml(string rawData)
        {
            XRF xpf = new XRF();

            XmlDocument xmlDocument = null;
            if (rawData.Substring(0, HEADER1.Length) == HEADER1)
            {
                xmlDocument = Cal3D.GetXml(XRF.HEADER1, rawData);
            }
            else if (rawData.Substring(0, HEADER2.Length) == HEADER2)
            {
                xmlDocument = Cal3D.GetXml(XRF.HEADER2, rawData);
            }
            else
            {
                return null;
            }

            XmlNode materialNode = xmlDocument.SelectSingleNode("/*/MATERIAL");
            if (materialNode == null)
            {
                return null;
            }

            xpf.Ambient = XCal3DByte4.Parse(materialNode.SelectSingleNode("AMBIENT"));
            xpf.Diffuse = XCal3DByte4.Parse(materialNode.SelectSingleNode("DIFFUSE"));
            xpf.Specular = XCal3DByte4.Parse(materialNode.SelectSingleNode("SPECULAR"));
            xpf.Shininess = Cal3D.GetFloatValue(materialNode.SelectSingleNode("SHININESS"));

            var maps = materialNode.SelectNodes("MAP");
            foreach (XmlNode mapNode in maps)
            {
                var map = new XRFMap();
                map.Type = Cal3D.GetStringAttribute(mapNode, "TYPE");
                map.AssetName = mapNode.InnerText;
                xpf.Maps.Add(map);
            }

            return xpf;
        }

        public static XRF ParseBinary(byte[] data)
        {
            BufferCrawler crawler = new BufferCrawler(data);

            var header = crawler.ReadString(4);

            XRF xrf = new XRF();
            var fileVersion = crawler.ReadUInt();
            xrf.Ambient = crawler.ReadByte4();
            xrf.Diffuse = crawler.ReadByte4();
            xrf.Specular = crawler.ReadByte4();
            xrf.Shininess = crawler.ReadFloat();

            var numMaps = crawler.ReadUInt();
            for (int i = 0; i < numMaps; i++)
            {
                var map = new XRFMap();
                map.AssetName = crawler.ReadStringWithCountPrefix();
                map.Type = crawler.ReadStringWithCountPrefix();
            }

            return xrf;
        }

        /// <summary>
        /// Parses the specified data into an XRF.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>An instance of XRF.</returns>
        /// <exception cref="System.ArgumentException">Invalid XRF/CRF header.</exception>
        public static XRF Parse(byte[] data)
        {
            BufferCrawler crawler = new BufferCrawler(data);

            var header = crawler.ReadString(4);

            if (header == "<HEA")
            {
                var xmlString = System.Text.Encoding.Default.GetString(data);
                return ParseXml(xmlString);
            }
            else if (header == "CRF\0")
            {
                return ParseBinary(data);
            }

            throw new ArgumentException("Invalid XRF/CRF header.");
        }

        public string ToFormattedString()
        {
            StringBuilder sb = new StringBuilder();
            if (this.Ambient.Used) sb.Append("\t<AMBIENT>" + Ambient.ToFormattedString() + "</AMBIENT>\r\n");
            if (this.Diffuse.Used) sb.Append("\t<DIFFUSE>" + Diffuse.ToFormattedString() + "</DIFFUSE>\r\n");
            if (this.Specular.Used) sb.Append("\t<SPECULAR>" + Specular.ToFormattedString() + "</SPECULAR>\r\n");
            sb.Append("\t<SHININESS>" + Shininess + "</SHININESS>\r\n");

            foreach (var map in Maps)
            {
                sb.Append(map.ToFormattedString());
            }

            return HEADER1 + Environment.NewLine +
                String.Format(MaterialFormat, this.Maps.Count, sb.ToString());
        }
    }
}