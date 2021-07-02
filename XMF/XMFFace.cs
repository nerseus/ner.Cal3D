using ner.Cal3D.Core;
using System;

namespace ner.Cal3D.XMF
{
    public class XMFFace : XCal3DInt3
    {
        public new string ToFormattedString()
        {
            return String.Format("\t\t<FACE VERTEXID=\"{0}\" />\r\n", base.ToFormattedString());
        }

        public static XMFFace Parse(BufferCrawler crawler)
        {
            if (crawler.IsFinished)
            {
                throw new ArgumentException("Missing face data");
            }

            var face = new XMFFace();
            face.V1 = crawler.ReadInt();
            face.V2 = crawler.ReadInt();
            face.V3 = crawler.ReadInt();
            face.Used = true;

            return face;
        }
    }
}