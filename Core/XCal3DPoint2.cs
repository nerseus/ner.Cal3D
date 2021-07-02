using System;

namespace ner.Cal3D.Core
{
    /// <summary>
    /// Represents 2 float values in format "1.0 2.0".
    /// </summary>
    public class XCal3DPoint2 : IFormattable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public bool Used { get; set; }

        public string ToFormattedString()
        {
            if (!Used) return string.Empty;

            return String.Format("{0} {1}", this.X, this.Y);
        }

        public static XCal3DPoint2 Parse(string value)
        {
            XCal3DPoint2 point2 = new XCal3DPoint2();
            if (string.IsNullOrEmpty(value.Trim()))
            {
                point2.Used = false;
            }
            else
            {
                try
                {
                    string[] values = value.Split(' ');
                    point2.X = Cal3D.ParseFloat(values[0]);
                    point2.Y = Cal3D.ParseFloat(values[1]);
                    point2.Used = true;
                }
                catch
                {
                    point2.Used = false;
                }
            }

            return point2;
        }
    }
}