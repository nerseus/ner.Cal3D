using ner.Cal3D.Core;
using System;
using System.Text;

namespace ner.Cal3D.XMF
{
    public class XMFSubmesh : IFormattable
    {
        /// <summary>
        /// String.Format
        ///     0 : NumVertices
        ///     1 : NumFaces
        ///     2 : NumMorphs
        ///     3 : NumTextureCoordinates
        ///     4 : Material ID
        ///     5 : Child objects
        /// </summary>
        public static readonly string SubmeshFormat = "\t<SUBMESH NUMVERTICES=\"{0}\" NUMFACES=\"{1}\" NUMLODSTEPS=\"0\" NUMSPRINGS=\"0\" NUMMORPHS=\"{2}\" NUMTEXCOORDS=\"{3}\" MATERIAL=\"{4}\">\r\n{5}\t</SUBMESH>\r\n";

        public int MaterialID { get; set; }
        public uint NumTextureCoordinates { get; set; }
        public string RawMorphData { get; set; }
        public BaseCal3DCollection<XMFMorph> Morphs { get; set; }
        public BaseCal3DCollection<XMFVertex> Vertices { get; set; }
        public XMFFaceCollection Faces { get; set; }
        public BaseCal3DCollection<XMFSpring> Springs { get; set; }

        public XMFSubmesh()
        {
            this.Vertices = new BaseCal3DCollection<XMFVertex>();
            this.Morphs = new BaseCal3DCollection<XMFMorph>();
            this.Faces = new XMFFaceCollection();
            this.Springs = new BaseCal3DCollection<XMFSpring>();
        }

        public string ToFormattedString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Vertices.ToFormattedString());
            if (this.Morphs.Count > 0) sb.Append(this.Morphs.ToFormattedString());
            if (!string.IsNullOrEmpty(this.RawMorphData)) sb.Append(this.RawMorphData);
            sb.Append(this.Faces.ToFormattedString());
            return String.Format(SubmeshFormat, this.Vertices.Count, this.Faces.Count, this.Morphs.Count, this.NumTextureCoordinates, this.MaterialID, sb.ToString());
        }

        public static XMFSubmesh Parse(BufferCrawler crawler)
        {
            if (crawler.IsFinished)
            {
                throw new ArgumentException("Missing submesh");
            }

            XMFSubmesh submesh = new XMFSubmesh();

            submesh.MaterialID = crawler.ReadInt();
            var vertexCount = crawler.ReadUInt();
            var faceCount = crawler.ReadUInt();
            var lodStepCount = crawler.ReadUInt();
            var springCount = crawler.ReadUInt();
            submesh.NumTextureCoordinates = crawler.ReadUInt();
            var morphTargetCount = crawler.ReadUInt();

            for (int i = 0; i < vertexCount; i++)
            {
                var vertex = XMFVertex.Parse(crawler, submesh.NumTextureCoordinates, springCount, i);
                submesh.Vertices.Add(vertex);
            }

            for (int i = 0; i < morphTargetCount; i++)
            {
                var morph = XMFMorph.Parse(crawler, i, submesh.NumTextureCoordinates, submesh.Vertices);
                submesh.Morphs.Add(morph);
            }

            for (int i = 0; i < springCount; i++)
            {
                var spring = XMFSpring.Parse(crawler);
                submesh.Springs.Add(spring);
            }

            for (int i = 0; i < faceCount; i++)
            {
                var face = XMFFace.Parse(crawler);
                submesh.Faces.Add(face);
            }

            return submesh;
        }
    }
}