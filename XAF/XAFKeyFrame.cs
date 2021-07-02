using ner.Cal3D.Core;
using System;

namespace ner.Cal3D.XAF
{
    /// <summary>
    /// Represents a keyframe in a Cal3D skeletal animation file.
    /// </summary>
    public class XAFKeyFrame : IFormattable
    {
        public float Time { get; set; }
        public XCal3DPoint3 Translation { get; set; }
        public XCal3DPoint4 Rotation { get; set; }

        public string ToFormattedString()
        {
            // Special case - do not include the "<TRANSLATION>...</TRANSLATION>" in the keyframe if it's not used.
            string translationString = string.Empty;
            if (Translation.Used)
            {
                translationString = String.Format(XAF.TranslationFormat, this.Translation.ToFormattedString());
            }

            return String.Format(XAF.KeyframeFormat, this.Time, translationString, this.Rotation.ToFormattedString());
        }
    }
}