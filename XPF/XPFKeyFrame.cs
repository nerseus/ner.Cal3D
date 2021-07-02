using System;

namespace ner.Cal3D.XPF
{
    public class XPFKeyFrame : IFormattable
    {
        public float Time { get; set; }
        public float Weight { get; set; }

        public string ToFormattedString()
        {
            return String.Format(XPF.KeyFrameFormat, Time, Weight);
        }
    }
}