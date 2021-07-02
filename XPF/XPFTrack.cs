using ner.Cal3D.Core;
using System;

namespace ner.Cal3D.XPF
{
    public class XPFTrack : IFormattable
    {
        public string MorphName;
        public BaseCal3DCollection<XPFKeyFrame> KeyFrames { get; set; }

        public XPFTrack()
        {
            this.KeyFrames = new BaseCal3DCollection<XPFKeyFrame>();
        }

        public string ToFormattedString()
        {
            return String.Format(XPF.TrackFormat, this.KeyFrames.Count, this.MorphName, this.KeyFrames.ToFormattedString());
        }

        public static XPFTrack Parse(BufferCrawler crawler)
        {
            if (crawler.IsFinished)
            {
                throw new ArgumentException("Missing morph track data");
            }

            var track = new XPFTrack();

            track.MorphName = crawler.ReadStringWithCountPrefix();
            var numKeyframes = crawler.ReadUInt();
            for (var i = 0; i < numKeyframes; i++)
            {
                var keyframe = new XPFKeyFrame();
                keyframe.Time = crawler.ReadFloat();
                keyframe.Weight = crawler.ReadFloat();

                track.KeyFrames.Add(keyframe);
            }

            return track;
        }
    }
}