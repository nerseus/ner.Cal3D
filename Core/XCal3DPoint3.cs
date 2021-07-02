using System;
using System.Xml;

namespace ner.Cal3D.Core
{
    /// <summary>
    /// Represents 3 float values in format "1.0 2.0 3.0".
    /// </summary>
    public class XCal3DPoint3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public bool Used { get; set; }

        public string ToFormattedString()
        {
            if (!Used) return string.Empty;

            return String.Format("{0} {1} {2}", this.X, this.Y, this.Z);
        }

        public static XCal3DPoint3 Parse(string value)
        {
            XCal3DPoint3 point3 = new XCal3DPoint3();
            try
            {
                string[] values = value.Split(' ');
                point3.X = Cal3D.ParseFloat(values[0]);
                point3.Y = Cal3D.ParseFloat(values[1]);
                point3.Z = Cal3D.ParseFloat(values[2]);
                point3.Used = true;
            }
            catch
            {
                point3.Used = false;
            }

            return point3;
        }

        public static XCal3DPoint3 Parse(XmlNode node)
        {
            if (node == null || string.IsNullOrEmpty(node.InnerText.Trim()))
            {
                return new XCal3DPoint3();
            }
            else
            {
                return Parse(node.InnerText);
            }
        }
    }
}