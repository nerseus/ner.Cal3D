using ner.Cal3D.Core;
using System;
using System.Xml;

namespace ner.Cal3D
{
    /// <summary>
    /// A utility class for parsing values and converting to/from Xml.
    /// </summary>
    public static class Cal3D
    {
        public static readonly System.Globalization.NumberStyles Cal3DFloatFormat =
            System.Globalization.NumberStyles.AllowExponent
            | System.Globalization.NumberStyles.AllowDecimalPoint
            | System.Globalization.NumberStyles.AllowLeadingSign;

        public static float ParseFloat(string value)
        {
            try
            {
                return float.Parse(value, Cal3DFloatFormat);
            }
            catch
            {
                return 0;
            }
        }

        public static int ParseInt(string value)
        {
            try
            {
                return int.Parse(value);
            }
            catch
            {
                return 0;
            }
        }

        public static byte ParseByte(string value)
        {
            try
            {
                return byte.Parse(value);
            }
            catch
            {
                return 0;
            }
        }

        public static XmlDocument GetXml(string header, string rawData)
        {
            if (rawData.Substring(0, header.Length) != header)
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml("<Root>" + rawData + "</Root>");
                return xml;
            }
            else
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml("<Root>" + rawData.Substring(header.Length + 1) + "</Root>");
                return xml;
            }
        }

        public static int GetIntAttribute(XmlNode node, string attributeName)
        {
            if (node.Attributes[attributeName] == null)
            {
                return 0;
            }

            string attributeValue = node.Attributes[attributeName].Value;
            return ParseInt(attributeValue);
        }

        public static XCal3DInt3 GetInt3Attribute(XmlNode node, string attributeName)
        {
            if (node.Attributes[attributeName] == null)
            {
                // Return empty object - will default ".Used" to false.
                return new XCal3DInt3();
            }

            string attributeValue = node.Attributes[attributeName].Value;
            return XCal3DInt3.Parse(attributeValue);
        }

        public static XCal3DPoint3 GetPoint3Attribute(XmlNode node, string attributeName)
        {
            if (node.Attributes[attributeName] == null)
            {
                // Return empty object - will default ".Used" to false.
                return new XCal3DPoint3();
            }

            string attributeValue = node.Attributes[attributeName].Value;
            return XCal3DPoint3.Parse(attributeValue);
        }

        public static float GetFloatAttribute(XmlNode node, string attributeName)
        {
            string attributeValue = node.Attributes[attributeName].Value;
            return ParseFloat(attributeValue);
        }

        public static string GetStringAttribute(XmlNode node, string attributeName)
        {
            string attributeValue = node.Attributes[attributeName].Value;
            return attributeValue;
        }

        public static int GetIntValue(XmlNode node)
        {
            string stringValue = node.InnerText;
            return ParseInt(stringValue);
        }

        public static float GetFloatValue(XmlNode node)
        {
            string stringValue = node.InnerText;
            return ParseFloat(stringValue);
        }

        public static XCal3DPoint3 GetPoint3Value(XmlNode node)
        {
            try
            {
                return XCal3DPoint3.Parse(node);
            }
            catch
            {
                return new XCal3DPoint3() { Used = false };
            }
        }

        public static XCal3DPoint4 GetPoint4Value(XmlNode node)
        {
            try
            {
                return XCal3DPoint4.Parse(node.InnerText);
            }
            catch
            {
                return new XCal3DPoint4();
            }
        }

        public static XCal3DPoint3 Subtract(this XCal3DPoint3 source, XCal3DPoint3 valueToSubtract)
        {
            var sum = new XCal3DPoint3();
            sum.X = source.X - valueToSubtract.X;
            sum.Y = source.Y - valueToSubtract.Y;
            sum.Z = source.Z - valueToSubtract.Z;
            sum.Used = true;
            return sum;
        }

        public static float Magnitude(this XCal3DPoint3 point)
        {
            var squared = (point.X * point.X)
                + (point.Y * point.Y)
                + (point.Z * point.Z);

            var sqrRoot = Math.Sqrt((double)squared);
            return (float)sqrRoot;
        }
    }
}