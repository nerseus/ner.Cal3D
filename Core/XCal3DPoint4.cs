using System;
using System.Xml;

namespace ner.Cal3D.Core
{
    /// <summary>
    /// Represents 4 float values in format "1.0 2.0 3.0 4.0".
    /// </summary>
    public class XCal3DPoint4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float A { get; set; }
        public bool Used { get; set; }

        public string ToFormattedString()
        {
            if (!Used) return string.Empty;

            return String.Format("{0} {1} {2} {3}", this.X, this.Y, this.Z, this.A);
        }

        public static XCal3DPoint4 Parse(string value)
        {
            XCal3DPoint4 point4 = new XCal3DPoint4();
            if (string.IsNullOrEmpty(value.Trim()))
            {
                point4.Used = false;
            }
            else
            {
                try
                {
                    string[] values = value.Split(' ');
                    point4.X = Cal3D.ParseFloat(values[0]);
                    point4.Y = Cal3D.ParseFloat(values[1]);
                    point4.Z = Cal3D.ParseFloat(values[2]);
                    point4.A = Cal3D.ParseFloat(values[3]);
                    point4.Used = true;
                }
                catch
                {
                    point4.Used = false;
                }
            }

            return point4;
        }

        public static XCal3DPoint4 Parse(XmlNode node)
        {
            if (node == null || string.IsNullOrEmpty(node.InnerText.Trim()))
            {
                return new XCal3DPoint4();
            }
            else
            {
                return Parse(node.InnerText);
            }
        }
    }
}