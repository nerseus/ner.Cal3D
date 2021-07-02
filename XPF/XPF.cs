using ner.Cal3D.Core;
using System;
using System.Linq;
using System.Xml;

namespace ner.Cal3D.XPF
{
    /// <summary>
    /// Represents an XPF file, the Cal3D format for morph animation. Once an XPF has been parsed, can call MergeWithOffset to merge with another XPF. See comments in the method for more details.
    /// Usage: Call static methods to parse. For example XPF.Parse(byte[])
    /// </summary>
    public class XPF
    {
        public static readonly string HEADER = "<HEADER MAGIC=\"XPF\" VERSION=\"919\" />";
        public static readonly string AnimationFormat = "<ANIMATION NUMTRACKS=\"{0}\" DURATION=\"{1}\">\r\n{2}</ANIMATION>";
        public static readonly string TrackFormat = "\t<TRACK NUMKEYFRAMES=\"{0}\" MORPHNAME=\"{1}\">\r\n{2}\t</TRACK>\r\n";
        public static readonly string KeyFrameFormat = "\t\t<KEYFRAME TIME=\"{0}\">\r\n\t\t\t<WEIGHT>{1}</WEIGHT>\r\n\t\t</KEYFRAME>\r\n";

        public float Duration { get; set; }
        public BaseCal3DCollection<XPFTrack> Tracks { get; set; }

        public XPF()
        {
            this.Tracks = new BaseCal3DCollection<XPFTrack>();
        }

        public static XPF ParseXml(string rawData)
        {
            XPF xpf = new XPF();

            XmlDocument xmlDocument = Cal3D.GetXml(XPF.HEADER, rawData);

            XmlNodeList animationNodes = xmlDocument.SelectNodes("/*/ANIMATION");

            if (animationNodes.Count != 1)
            {
                return null;
            }

            xpf.Duration = Cal3D.GetFloatAttribute(animationNodes[0], "DURATION");

            foreach (XmlNode trackNode in animationNodes[0].SelectNodes("TRACK"))
            {
                XPFTrack track = new XPFTrack();
                track.MorphName = Cal3D.GetStringAttribute(trackNode, "MORPHNAME");

                foreach (XmlNode keyFrameNode in trackNode.SelectNodes("KEYFRAME"))
                {
                    XPFKeyFrame keyFrame = new XPFKeyFrame();
                    keyFrame.Time = Cal3D.GetFloatAttribute(keyFrameNode, "TIME");
                    XmlNode weightNode = keyFrameNode.SelectSingleNode("WEIGHT");
                    keyFrame.Weight = Cal3D.GetFloatValue(weightNode);

                    track.KeyFrames.Add(keyFrame);
                }

                xpf.Tracks.Add(track);
            }

            return xpf;
        }

        public static XPF ParseBinary(byte[] data)
        {
            BufferCrawler crawler = new BufferCrawler(data);

            var header = crawler.ReadString(4);

            XPF xpf = new XPF();
            var fileVersion = crawler.ReadUInt();

            xpf.Duration = crawler.ReadFloat();

            var numTracks = crawler.ReadUInt();
            for (var i = 0; i < numTracks; i++)
            {
                var track = XPFTrack.Parse(crawler);
                xpf.Tracks.Add(track);
            }


            return xpf;
        }

        /// <summary>
        /// Parses the specified data into an XPF.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>An instance of XPF.</returns>
        /// <exception cref="System.ArgumentException">Invalid XPF/CPF header.</exception>
        public static XPF Parse(byte[] data)
        {
            BufferCrawler crawler = new BufferCrawler(data);

            var header = crawler.ReadString(4);

            if (header == "<HEA")
            {
                var xmlString = System.Text.Encoding.Default.GetString(data);
                return ParseXml(xmlString);
            }
            else if (header == "CPF\0")
            {
                return ParseBinary(data);
            }

            throw new ArgumentException("Invalid XPF/CPF header.");
        }

        public string ToFormattedString()
        {
            return XPF.HEADER + Environment.NewLine + String.Format(XPF.AnimationFormat, Tracks.Count, Duration, Tracks.ToFormattedString());
        }

        /// <summary>
        /// Creates a new morph based on baseMorph. The baseMorph is meant to match the startTime, endTime, duration, etc. based on the playMorph.
        /// The usage scenario is:
        ///     The 3d software creates a long animation of an avatar acting surprised. At the 3 second mark, the face needs to make the "Oh my!" face.
        ///     Using a tool like Deliverance's face tool, a single-frame XPF is created for the "Oh my!" face.
        ///     This method would take the "Oh my!" morph animation as baseMorph.
        ///     StartTime might be 3.0 and endTime is 4.0 with a duration of 1.0.
        ///     So the mouth doesn't "jump" to the "Oh my!" face, the blendFrames can be set to 10 to allow a blending of the non-Oh-My face
        ///     to the Oh-my-face.
        /// </summary>
        /// <param name="baseMorph"></param>
        /// <param name="playMorph"></param>
        /// <param name="newStartTime"></param>
        /// <param name="newEndTime"></param>
        /// <param name="newDuration"></param>
        /// <param name="blendFrames"></param>
        /// <returns></returns>
        public static XPF MergeWithOffset(XPF baseMorph, XPF playMorph, float newStartTime, float newEndTime, float newDuration, int blendFrames)
        {
            XPF newMorph = new XPF();
            newMorph.Duration = newDuration;

            // Copy the keyframes from baseMorph into a newMorph
            // Force time = 0
            foreach (XPFTrack track in baseMorph.Tracks)
            {
                XPFTrack newTrack = new XPFTrack();
                newTrack.MorphName = track.MorphName;

                XPFKeyFrame newKeyFrame = new XPFKeyFrame();
                newKeyFrame.Time = 0;
                if (track.KeyFrames.Count > 0)
                {
                    newKeyFrame.Weight = track.KeyFrames[0].Weight;
                }
                else
                {
                    newKeyFrame.Weight = 0;
                }

                newTrack.KeyFrames.Add(newKeyFrame);
                newMorph.Tracks.Add(newTrack);
            }

            // Add three more keyFrames:
            //      Time                    Weight                      Desc
            //      =====================   =========                   ====================================================================================================
            //      newStartTime - blend    match weight at frame 0     Start the transition from base morph into this morph
            //      newStartTime            this morph's weight         Play this morph
            //      newEndTime              this morph's weight         Continue playing this morph until end time
            //      newEndTime + blend      match weight at frame 0     Begin blending back to base morph. This will stick til animation is complete (for the duration)
            float blendTime = (float)Math.Round((double)blendFrames / 30.0, 5);
            float blendInTime = newStartTime - blendTime;
            float blendOutTime = newEndTime + blendTime;
            if (blendInTime < 0.0) blendInTime = 0.0001f;
            if (blendOutTime > newDuration) blendOutTime = newDuration - 0.0001f;
            foreach (XPFTrack track in playMorph.Tracks)
            {
                XPFTrack newTrack = newMorph.Tracks.FirstOrDefault(x => x.MorphName == track.MorphName);

                // Make sure all morph targets in this morph are in newMorph
                // They might be missing if they didn't exist in baseMorph
                if (newTrack == null)
                {
                    newTrack = new XPFTrack();
                    newTrack.MorphName = track.MorphName;

                    XPFKeyFrame newKeyFrame = new XPFKeyFrame();
                    newKeyFrame.Time = 0;
                    if (track.KeyFrames.Count > 0)
                    {
                        newKeyFrame.Weight = track.KeyFrames[0].Weight;
                    }
                    else
                    {
                        newKeyFrame.Weight = 0;
                    }

                    newTrack.KeyFrames.Add(newKeyFrame);
                    newMorph.Tracks.Add(newTrack);
                }

                float baseWeight = newTrack.KeyFrames[0].Weight;
                float thisWeight = track.KeyFrames[0].Weight;
                // Skip creating extra keyframes if the weights don't change
                if (baseWeight != thisWeight)
                {
                    // Create the "blendIn" keyframe
                    XPFKeyFrame blendInFrame = new XPFKeyFrame();
                    blendInFrame.Time = blendInTime;
                    blendInFrame.Weight = baseWeight;
                    newTrack.KeyFrames.Add(blendInFrame);

                    // Create the main morph keyframe at start Time
                    XPFKeyFrame startMorphFrame = new XPFKeyFrame();
                    startMorphFrame.Time = newStartTime;
                    startMorphFrame.Weight = thisWeight;
                    newTrack.KeyFrames.Add(startMorphFrame);

                    // Create the main morph keyframe at end Time
                    XPFKeyFrame endMorphFrame = new XPFKeyFrame();
                    endMorphFrame.Time = newEndTime;
                    endMorphFrame.Weight = thisWeight;
                    newTrack.KeyFrames.Add(endMorphFrame);

                    // Create the "blendOut" keyframe
                    XPFKeyFrame blendOutFrame = new XPFKeyFrame();
                    blendOutFrame.Time = blendOutTime;
                    blendOutFrame.Weight = baseWeight;
                    newTrack.KeyFrames.Add(blendOutFrame);
                }
            }

            return newMorph;
        }
    }
}