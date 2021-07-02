using System;

namespace ner.Cal3D.XMF
{
    public class XMFInfluence
    {
        public static readonly string InfluenceFormat = "\t\t\t<INFLUENCE ID=\"{0}\">{1}</INFLUENCE>\r\n";

        public int BoneID { get; set; }
        public float Influence { get; set; }

        public string ToFormattedString()
        {
            return String.Format(InfluenceFormat, BoneID, Influence);
        }
    }
}