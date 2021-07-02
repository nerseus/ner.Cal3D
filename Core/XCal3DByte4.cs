using System;
using System.Xml;

namespace ner.Cal3D.Core
{
    /// <summary>
    /// Represents 4 byte values in format "1 2 3 4".
    /// </summary>
    public class XCal3DByte4
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Z { get; set; }
        public byte A { get; set; }
        public bool Used { get; set; }

        public string ToFormattedString()
        {
            if (!Used) return string.Empty;

            return String.Format("{0} {1} {2} {3}", this.X, this.Y, this.Z, this.A);
        }

        public static XCal3DByte4 Parse(string value)
        {
            XCal3DByte4 point4 = new XCal3DByte4();
            if (!string.IsNullOrEmpty(value.Trim()))
            {
                point4.Used = false;
            }
            else
            {
                try
                {
                    string[] values = value.Split(' ');
                    point4.X = Cal3D.ParseByte(values[0]);
                    point4.Y = Cal3D.ParseByte(values[1]);
                    point4.Z = Cal3D.ParseByte(values[2]);
                    point4.A = Cal3D.ParseByte(values[3]);
                    point4.Used = true;
                }
                catch
                {
                    point4.Used = false;
                }
            }

            return point4;
        }

        public static XCal3DByte4 Parse(XmlNode node)
        {
            if (node == null || string.IsNullOrEmpty(node.InnerText.Trim()))
            {
                return new XCal3DByte4();
            }
            else
            {
                return Parse(node.InnerText);
            }
        }
    }
}