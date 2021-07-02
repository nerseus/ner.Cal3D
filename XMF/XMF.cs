using ner.Cal3D.Core;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace ner.Cal3D.XMF
{
    /// <summary>
    /// Represents an XMF file, the Cal3D format for meshes including copies of the morph targets. Once an XMF has been parsed, can call Join to merge the XMF with another XMF.
    /// Usage: Call static methods to parse. For example XMF.Parse(byte[])
    /// </summary>
    public class XMF
    {
        public static readonly string HEADER = "<HEADER MAGIC=\"XMF\" VERSION=\"919\" />";
        public static readonly string MeshFormat = "<MESH NUMSUBMESH=\"{0}\">\r\n{1}</MESH>";

        private static int MorphReplaceCount = 0;

        public BaseCal3DCollection<XMFSubmesh> Submeshes { get; set; }

        public XMF()
        {
            this.Submeshes = new BaseCal3DCollection<XMFSubmesh>();
        }

        private static string ExtraMorphRemover(Match match)
        {
            MorphReplaceCount++;
            if (MorphReplaceCount % 2 == 0)
            {
                return string.Empty;
            }
            else
            {
                return match.Value;
            }
        }

        public static XMF ParseXml(string rawData)
        {
            XMF xmf = new XMF();

            // Occasionally see a SUBMESH line with two NUMMORPHS attributes.
            // As this is in rawData, must remove them but we can't find the "attribute."
            // Instead, take a shortcut and remove every-other occurance of NUMMORPHS.
            // Could possibly check that count of NUMMORPHS is exact 2x the count of SUBMESH. But ah well.
            var morphPattern = @"NUMMORPHS=""\d+""";
            var submeshPattern = @"<SUBMESH ";
            MatchCollection morphMatches = Regex.Matches(rawData, morphPattern);
            MatchCollection submeshMatches = Regex.Matches(rawData, submeshPattern);
            if (morphMatches.Count > submeshMatches.Count)
            {
                MorphReplaceCount = 0;
                var evaluator = new MatchEvaluator(ExtraMorphRemover);
                rawData = Regex.Replace(rawData, morphPattern, evaluator);
            }

            XmlDocument xmlDocument = Cal3D.GetXml(HEADER, rawData);

            XmlNodeList meshNodes = xmlDocument.SelectNodes("/*/MESH");

            if (meshNodes.Count != 1)
            {
                return null;
            }

            XmlNode meshNode = meshNodes[0];

            XmlNodeList submeshNodes = meshNode.SelectNodes("SUBMESH");

            foreach (XmlNode submeshNode in submeshNodes)
            {
                XMFSubmesh submesh = new XMFSubmesh();
                var numMorphs = Cal3D.GetIntAttribute(submeshNode, "NUMMORPHS");
                submesh.NumTextureCoordinates = (uint)Cal3D.GetIntAttribute(submeshNode, "NUMTEXCOORDS");
                submesh.MaterialID = Cal3D.GetIntAttribute(submeshNode, "MATERIAL");

                foreach (XmlNode vertexNode in submeshNode.SelectNodes("VERTEX"))
                {
                    XMFVertex vertex = new XMFVertex();
                    vertex.ID = Cal3D.GetIntAttribute(vertexNode, "ID");
                    vertex.Position = XCal3DPoint3.Parse(vertexNode.SelectSingleNode("POS"));
                    vertex.Normal = XCal3DPoint3.Parse(vertexNode.SelectSingleNode("NORM"));
                    vertex.Color = XCal3DPoint3.Parse(vertexNode.SelectSingleNode("COLOR"));

                    foreach (XmlNode textureCoordinateNode in vertexNode.SelectNodes("TEXCOORD"))
                    {
                        XMFTextureCoordinate textureCoordinate = new XMFTextureCoordinate();
                        XCal3DPoint2 point2 = XCal3DPoint2.Parse(textureCoordinateNode.InnerText);
                        textureCoordinate.X = point2.X;
                        textureCoordinate.Y = point2.Y;
                        textureCoordinate.Used = point2.Used;
                        vertex.TextureCoordinates.Add(textureCoordinate);
                    }

                    foreach (XmlNode influenceNode in vertexNode.SelectNodes("INFLUENCE"))
                    {
                        XMFInfluence influence = new XMFInfluence();
                        influence.BoneID = Cal3D.GetIntAttribute(influenceNode, "ID");
                        influence.Influence = Cal3D.GetFloatValue(influenceNode);
                        vertex.Influences.Add(influence);
                    }

                    vertex.Influences.NormalizeInfluences();

                    submesh.Vertices.Add(vertex);
                }

                // TODO - Can change to NOT use RawMorphData, but actually parse into XMFMorph objects.
                StringBuilder morphData = new StringBuilder();
                foreach (XmlNode morphNode in submeshNode.SelectNodes("MORPH"))
                {
                    // Don't do anything with the MORPH data for now - just store as a string blob
                    morphData.Append(morphNode.OuterXml);
                }
                submesh.RawMorphData = morphData.ToString();

                foreach (XmlNode faceNode in submeshNode.SelectNodes("FACE"))
                {
                    XMFFace xmfFace = new XMFFace();
                    var face = XCal3DInt3.Parse(Cal3D.GetStringAttribute(faceNode, "VERTEXID"));
                    xmfFace.V1 = face.V1;
                    xmfFace.V2 = face.V2;
                    xmfFace.V3 = face.V3;
                    xmfFace.Used = face.Used;
                    submesh.Faces.Add(xmfFace);
                }

                xmf.Submeshes.Add(submesh);
            }

            return xmf;
        }

        public static XMF ParseBinary(byte[] data)
        {
            BufferCrawler crawler = new BufferCrawler(data);

            var header = crawler.ReadString(4);

            XMF xmf = new XMF();
            var fileVersion = crawler.ReadUInt();

            var submeshCount = crawler.ReadUInt();
            for (int i = 0; i < submeshCount && !crawler.IsFinished; i++)
            {
                var submesh = XMFSubmesh.Parse(crawler);
                xmf.Submeshes.Add(submesh);
            }

            return xmf;
        }

        public static XMF Parse(byte[] data)
        {
            BufferCrawler crawler = new BufferCrawler(data);

            var header = crawler.ReadString(4);

            if (header == "<HEA")
            {
                var xmlString = System.Text.Encoding.Default.GetString(data);
                return ParseXml(xmlString);
            }
            else if (header == "CMF\0")
            {
                return ParseBinary(data);
            }

            throw new ArgumentException("Invalid XMF/CMF header.");
        }

        public string ToFormattedString()
        {
            return XMF.HEADER + Environment.NewLine +
                String.Format(XMF.MeshFormat, this.Submeshes.Count, this.Submeshes.ToFormattedString());
        }

        internal static XMF JoinFiles(string[] fileList)
        {
            XMF xmf = null;
            foreach (string file in fileList)
            {
                XMF xmfTemp = null;
                try
                {
                    string fileData = File.ReadAllText(file);
                    xmfTemp = XMF.ParseXml(fileData);
                }
                catch
                {
                    return null;
                }


                if (xmf == null)
                {
                    xmf = xmfTemp;
                }
                else
                {
                    foreach (XMFSubmesh subMesh in xmfTemp.Submeshes)
                    {
                        xmf.Submeshes.Add(subMesh);
                    }
                }
            }

            return xmf;
        }
    }
}