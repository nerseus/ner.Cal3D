using ner.Cal3D.Core;
using System;
using System.Text;

namespace ner.Cal3D.XMF
{
    public class XMFMorph : IFormattable
    {
        public static readonly string MorphFormat = "\t\t<MORPH NAME=\"{0}\" NUMBLENDVERTS=\"{1}\" MORPHID=\"{2}\">\r\n{3}\t\t</MORPH>\r\n";

        public string Name { get; set; }

        public int MorphId { get; set; }

        public BaseCal3DCollection<XMFBlendVertex> BlendVertices { get; set; }

        public XMFMorph()
        {
            this.BlendVertices = new BaseCal3DCollection<XMFBlendVertex>();
        }

        public string ToFormattedString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(BlendVertices.ToFormattedString());

            return string.Format(MorphFormat, this.Name, this.BlendVertices.Count, this.MorphId, sb.ToString());
        }

        public static XMFMorph Parse(BufferCrawler crawler, int morphIndex, uint numberOfTextureCoordinates, BaseCal3DCollection<XMFVertex> vertices)
        {
            if (crawler.IsFinished)
            {
                throw new ArgumentException("Missing morph data");
            }

            XMFMorph morph = new XMFMorph();
            morph.MorphId = morphIndex;
            var nameLength = crawler.ReadInt();
            morph.Name = crawler.ReadStringTrailingNull(nameLength);

            int vertexId = crawler.ReadInt();
            while (vertexId <= vertices.Count)
            {
                var blendVertex = XMFBlendVertex.Parse(vertexId, crawler, numberOfTextureCoordinates, vertices);
                morph.BlendVertices.Add(blendVertex);
                vertexId = crawler.ReadInt();
            }

            return morph;
        }
    }
}