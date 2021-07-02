using System;
using System.Collections.Generic;

namespace ner.Cal3D.Core
{
    /// <summary>
    /// Represents 3 int values in format "1 2 3".
    /// </summary>
    public class XCal3DInt3
    {
        public int V1 { get; set; }
        public int V2 { get; set; }
        public int V3 { get; set; }
        public bool Used { get; set; }

        public string ToFormattedString()
        {
            if (!Used) return string.Empty;

            return String.Format("{0} {1} {2}", this.V1, this.V2, this.V3);
        }

        public List<int> ToIntList(bool reverseWindingOrder)
        {
            if (reverseWindingOrder)
            {
                return new List<int> { V1, V3, V2 };
            }
            else
            {
                return new List<int> { V1, V2, V3 };
            }
        }

        public static XCal3DInt3 Parse(string value)
        {
            XCal3DInt3 int3 = new XCal3DInt3();
            if (string.IsNullOrEmpty(value.Trim()))
            {
                int3.Used = false;
            }
            else
            {
                try
                {
                    string[] values = value.Split(' ');
                    int3.V1 = Convert.ToInt32(values[0]);
                    int3.V2 = Convert.ToInt32(values[1]);
                    int3.V3 = Convert.ToInt32(values[2]);
                    int3.Used = true;
                }
                catch
                {
                    int3.Used = false;
                }
            }

            return int3;
        }
    }
}