using ner.Cal3D.Core;
using System;
using System.Linq;

namespace ner.Cal3D.XAF
{
    /// <summary>
    /// Represents a track in a skeletal animation file.
    /// </summary>
    public class XAFTrack : IFormattable
    {
        public int BoneID { get; set; }
        public BaseCal3DCollection<XAFKeyFrame> KeyFrames { get; set; }

        public XAFTrack()
        {
            this.KeyFrames = new BaseCal3DCollection<XAFKeyFrame>();
        }

        private string IsTranslationRequired()
        {
            bool? isTranslationRequired = KeyFrames?.Any(kf => kf.Translation.Used);

            if (isTranslationRequired.HasValue && isTranslationRequired.Value)
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }

        public string ToFormattedString()
        {
            return String.Format(XAF.TrackFormat, this.BoneID, this.KeyFrames.Count, this.KeyFrames.ToFormattedString(), IsTranslationRequired());
        }

        public static XAFTrack Parse(BufferCrawler crawler)
        {
            if (crawler.IsFinished)
            {
                throw new ArgumentException("Missing skeleton track data");
            }

            var track = new XAFTrack();
            track.BoneID = crawler.ReadInt();
            var numKeyframes = crawler.ReadUInt();
            for (var i = 0; i < numKeyframes; i++)
            {
                var keyframe = new XAFKeyFrame();
                keyframe.Time = crawler.ReadFloat();
                keyframe.Translation = crawler.ReadPoint3();
                keyframe.Rotation = crawler.ReadPoint4();

                if (keyframe.Translation.X == 10000000000f
                    && keyframe.Translation.Y == 10000000000f
                    && keyframe.Translation.Z == 10000000000f)
                {
                    keyframe.Translation.Used = false;
                }

                track.KeyFrames.Add(keyframe);
            }

            return track;
        }
    }
}