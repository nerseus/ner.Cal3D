using System.Collections.Generic;
using System.Text;

namespace ner.Cal3D.XMF
{
    public class XMFFaceCollection : List<XMFFace>
    {
        public string ToFormattedString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (XMFFace face in this)
            {
                sb.Append(face.ToFormattedString());
            }

            return sb.ToString();
        }

        public List<int> ToIntList(bool reverseWindingOrder)
        {
            List<int> list = new List<int>();
            foreach (var face in this)
            {
                list.AddRange(face.ToIntList(reverseWindingOrder));
            }

            return list;
        }
    }
}