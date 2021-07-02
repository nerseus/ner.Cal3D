using ner.Cal3D.Core;
using System;
using System.Linq;
using System.Xml;

namespace ner.Cal3D.XAF
{
    /// <summary>
    /// Represents an XAF file, the Cal3D format for skeletal animation. Once an XAF has been parsed, there are a few helper methods (such as lengthen, stretch, etc.) that can be used.
    /// Usage: Call static methods to parse. For example XAF.Parse(byte[])
    /// </summary>
    public class XAF
    {
        public static readonly string HEADER = "<HEADER MAGIC=\"XAF\" VERSION=\"919\" />";
        public static readonly string AnimationFormat = "<ANIMATION VERSION=\"1000\" DURATION=\"{0}\" NUMTRACKS=\"{1}\">\r\n{2}</ANIMATION>";
        public static readonly string TrackFormat = "\t<TRACK TRANSLATIONREQUIRED=\"{3}\" TRANSLATIONISDYNAMIC=\"0\" HIGHRANGEREQUIRED=\"0\" BONEID=\"{0}\" NUMKEYFRAMES=\"{1}\">\r\n{2}\t</TRACK>\r\n";
        public static readonly string KeyframeFormat = "\t\t<KEYFRAME TIME=\"{0}\">\r\n{1}\t\t\t<ROTATION>{2}</ROTATION>\r\n\t\t</KEYFRAME>\r\n";
        public static readonly string TranslationFormat = "\t\t\t<TRANSLATION>{0}</TRANSLATION>\r\n";

        public float Duration { get; set; }

        public BaseCal3DCollection<XAFTrack> Tracks { get; set; }

        public XAF()
        {
            Tracks = new BaseCal3DCollection<XAFTrack>();
        }

        public static XAF ParseXml(string rawData)
        {
            XAF xaf = new XAF();

            XmlDocument xmlDocument = Cal3D.GetXml(HEADER, rawData);

            XmlNodeList animationNodes = xmlDocument.SelectNodes("/*/ANIMATION");

            if (animationNodes.Count != 1)
            {
                return null;
            }

            xaf.Duration = Cal3D.GetFloatAttribute(animationNodes[0], "DURATION");

            XmlNodeList trackNodes = animationNodes[0].SelectNodes("TRACK");
            foreach (XmlNode trackNode in trackNodes)
            {
                XAFTrack track = new XAFTrack();
                track.BoneID = Cal3D.GetIntAttribute(trackNode, "BONEID");

                foreach (XmlNode keyFrameNode in trackNode.SelectNodes("KEYFRAME"))
                {
                    XAFKeyFrame keyFrame = new XAFKeyFrame();
                    keyFrame.Time = Cal3D.GetFloatAttribute(keyFrameNode, "TIME");

                    XmlNode translationNode = keyFrameNode.SelectSingleNode("TRANSLATION");
                    keyFrame.Translation = Cal3D.GetPoint3Value(translationNode);

                    XmlNode rotationNode = keyFrameNode.SelectSingleNode("ROTATION");
                    keyFrame.Rotation = Cal3D.GetPoint4Value(rotationNode);

                    track.KeyFrames.Add(keyFrame);
                }

                xaf.Tracks.Add(track);
            }

            return xaf;
        }

        public static XAF ParseBinary(byte[] data)
        {
            BufferCrawler crawler = new BufferCrawler(data);

            var header = crawler.ReadString(4);

            XAF xaf = new XAF();
            var fileVersion = crawler.ReadUInt();

            var unknown = crawler.ReadUInt();

            xaf.Duration = crawler.ReadFloat();

            var numTracks = crawler.ReadUInt();
            for (var i = 0; i < numTracks; i++)
            {
                var track = XAFTrack.Parse(crawler);
                xaf.Tracks.Add(track);
            }

            return xaf;
        }

        /// <summary>
        /// Parses the specified data into an XAF.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>An instance of XAF.</returns>
        /// <exception cref="System.ArgumentException">Invalid XAF/CAF header.</exception>
        public static XAF Parse(byte[] data)
        {
            BufferCrawler crawler = new BufferCrawler(data);

            var header = crawler.ReadString(4);

            if (header == "<HEA")
            {
                var xmlString = System.Text.Encoding.Default.GetString(data);
                return ParseXml(xmlString);
            }
            else if (header == "CAF\0")
            {
                return ParseBinary(data);
            }

            throw new ArgumentException("Invalid XAF/CAF header.");
        }

        public string ToFormattedString()
        {
            return XAF.HEADER + Environment.NewLine
                + String.Format(XAF.AnimationFormat, this.Duration, this.Tracks.Count, this.Tracks.ToFormattedString());
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReverseAndAppend()
        {
            this.Duration *= 2f;

            foreach (XAFTrack track in this.Tracks)
            {
                // Copy all of the keyframes on this track, in reverse order
                // Adjusting the time to reverse them
                var newKeyFrames = new BaseCal3DCollection<XAFKeyFrame>();
                foreach (XAFKeyFrame keyFrame in track.KeyFrames.OrderByDescending(x => x.Time))
                {
                    if (keyFrame.Time != this.Duration)
                    {
                        XAFKeyFrame newKeyFrame = new XAFKeyFrame();
                        newKeyFrame.Translation = keyFrame.Translation;
                        newKeyFrame.Rotation = keyFrame.Rotation;
                        newKeyFrame.Time = this.Duration - keyFrame.Time;
                        newKeyFrames.Add(newKeyFrame);
                    }
                }

                // Append the reversed keyframes
                foreach (XAFKeyFrame keyFrame in newKeyFrames)
                {
                    track.KeyFrames.Add(keyFrame);
                }
            }
        }

        public void Offset(float offset)
        {
            this.Duration += offset;

            foreach (XAFTrack track in this.Tracks)
            {
                foreach (XAFKeyFrame keyframe in track.KeyFrames)
                {
                    keyframe.Time += offset;
                }
            }
        }

        public void LengthenLastFrame(float lengthenTime)
        {
            this.Duration += lengthenTime;
        }

        public void Stretch(float stretchTime)
        {
            this.Duration *= stretchTime;

            foreach (XAFTrack track in this.Tracks)
            {
                foreach (XAFKeyFrame keyframe in track.KeyFrames)
                {
                    keyframe.Time *= stretchTime;
                }
            }
        }

        public void Untwitch()
        {
            foreach (XAFTrack track in this.Tracks)
            {
                // remove all but the first two keyframes
                for (int i = track.KeyFrames.Count - 1; i >= 2; i--)
                {
                    track.KeyFrames.RemoveAt(i);
                }

                // Force the 2nd keyframe (if it exists) to match the first keyframe
                if (track.KeyFrames.Count > 1)
                {
                    track.KeyFrames[1].Rotation = track.KeyFrames[0].Rotation;
                    track.KeyFrames[1].Translation = track.KeyFrames[0].Translation;
                }
            }
        }
    }
}