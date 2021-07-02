using ner.Cal3D.Core;
using System;
using System.Linq;
using System.Text;

namespace ner.Cal3D.XMF
{
    public class XMFBlendVertex : IFormattable
    {
        public static readonly string MorphFormat = "\t\t\t<BLENDVERTEX VERTEXID=\"{0}\" POSDIFF=\"{1}\">\r\n{2}\t\t\t</BLENDVERTEX>\r\n";

        public int VertexId { get; set; }

        public float PosDiff { get; set; }

        public XCal3DPoint3 Position { get; set; }
        public XCal3DPoint3 Normal { get; set; }
        public BaseCal3DCollection<XMFTextureCoordinate> TextureCoordinates { get; set; }

        public XMFBlendVertex()
        {
            this.Position = new XCal3DPoint3();
            this.Normal = new XCal3DPoint3();
            this.TextureCoordinates = new BaseCal3DCollection<XMFTextureCoordinate>();
        }

        public string ToFormattedString()
        {
            StringBuilder sb = new StringBuilder();
            if (this.Position.Used) sb.Append("\t\t\t\t<POSITION>" + Position.ToFormattedString() + "</POSITION>\r\n");
            if (this.Normal.Used) sb.Append("\t\t\t\t<NORMAL>" + Normal.ToFormattedString() + "</NORMAL>\r\n");
            sb.Append(this.TextureCoordinates.ToFormattedString());

            return string.Format(MorphFormat, this.VertexId, this.PosDiff, sb.ToString());
        }

        public static XMFBlendVertex Parse(int vertexId, BufferCrawler crawler, uint numberOfTextureCoordinates, BaseCal3DCollection<XMFVertex> vertices)
        {
            if (crawler.IsFinished)
            {
                throw new ArgumentException("Missing morph blendvertex data");
            }

            XMFBlendVertex blendVertex = new XMFBlendVertex();
            blendVertex.VertexId = vertexId;

            blendVertex.Position = crawler.ReadPoint3();
            blendVertex.Normal = crawler.ReadPoint3();

            for (int i = 0; i < numberOfTextureCoordinates; i++)
            {
                var texCoordinate = crawler.ReadTexCoord();
                texCoordinate.NumIndentTabs = 4;
                blendVertex.TextureCoordinates.Add(texCoordinate);
            }

            var origVertex = vertices.FirstOrDefault(x => x.ID == blendVertex.VertexId);
            var diff = blendVertex.Position.Subtract(origVertex.Position);
            blendVertex.PosDiff = diff.Magnitude();

            return blendVertex;
        }
    }
}