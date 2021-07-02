using ner.Cal3D.Core;
using System;
using System.Text;

namespace ner.Cal3D.XMF
{
    public class XMFVertex : IFormattable
    {
        /// <summary>
        /// String.Format
        ///     0 : NumInfluences
        ///     1 : ID
        ///     2 : Child objects
        /// </summary>
        public static readonly string VertexFormat = "\t\t<VERTEX NUMINFLUENCES=\"{0}\" ID=\"{1}\">\r\n{2}\t\t</VERTEX>\r\n";

        public XMFVertex()
        {
            this.Position = new XCal3DPoint3();
            this.Normal = new XCal3DPoint3();
            this.Color = new XCal3DPoint3();
            this.TextureCoordinates = new BaseCal3DCollection<XMFTextureCoordinate>();
            this.Influences = new XMFInfluenceCollection();
        }

        public XMFVertex(int id, float x, float y, float z, float textureU, float textureV, float normalX, float normalY, float normalZ) : this()
        {
            ID = id;

            Position.X = x;
            Position.Y = y;
            Position.Z = z;
            Position.Used = true;

            Normal.X = normalX;
            Normal.Y = normalY;
            Normal.Z = normalZ;
            Normal.Used = true;

            Color.X = 0;
            Color.Y = 0;
            Color.Z = 0;
            Color.Used = true;

            XMFTextureCoordinate textureCoordinate = new XMFTextureCoordinate();
            textureCoordinate.X = textureU;
            textureCoordinate.Y = textureV;
            textureCoordinate.Used = true;
            TextureCoordinates.Add(textureCoordinate);
        }

        public int ID { get; set; }
        public XCal3DPoint3 Position { get; set; }
        public XCal3DPoint3 Normal { get; set; }
        public XCal3DPoint3 Color { get; set; }
        public BaseCal3DCollection<XMFTextureCoordinate> TextureCoordinates { get; set; }
        public XMFInfluenceCollection Influences { get; set; }

        public string ToFormattedString()
        {
            StringBuilder sb = new StringBuilder();
            if (this.Position.Used) sb.Append("\t\t\t<POS>" + Position.ToFormattedString() + "</POS>\r\n");
            if (this.Normal.Used) sb.Append("\t\t\t<NORM>" + Normal.ToFormattedString() + "</NORM>\r\n");
            if (this.Color.Used) sb.Append("\t\t\t<COLOR>" + Color.ToFormattedString() + "</COLOR>\r\n");
            sb.Append(TextureCoordinates.ToFormattedString());
            sb.Append(Influences.ToFormattedString());

            return string.Format(VertexFormat, this.Influences.Count, this.ID, sb.ToString());
        }

        public static XMFVertex Parse(BufferCrawler crawler, uint numberOfTextureCoordinates, uint springCount, int id)
        {
            if (crawler.IsFinished)
            {
                throw new ArgumentException("Missing submesh vertex data");
            }

            XMFVertex vertex = new XMFVertex();
            vertex.ID = id;

            vertex.Position = crawler.ReadPoint3();
            vertex.Normal = crawler.ReadPoint3();
            vertex.Color = crawler.ReadPoint3();

            var collapseId = crawler.ReadUInt();
            var collapseCount = crawler.ReadUInt();

            for (int i = 0; i < numberOfTextureCoordinates; i++)
            {
                var texCoord = crawler.ReadTexCoord();
                vertex.TextureCoordinates.Add(texCoord);
            }

            var influenceCount = crawler.ReadUInt();
            for (int i = 0; i < influenceCount; i++)
            {
                var influence = new XMFInfluence();
                influence.BoneID = crawler.ReadInt();
                influence.Influence = crawler.ReadFloat();
                vertex.Influences.Add(influence);
            }

            if (springCount > 0)
            {
                var weight = crawler.ReadFloat();
            }

            return vertex;
        }
    }
}