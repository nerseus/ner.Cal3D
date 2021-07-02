using ner.Cal3D.Core;
using System;
using System.Xml;

namespace ner.Cal3D.XSF
{
    /// <summary>
    /// Represents an XSF file, the Cal3D format for skeletal bone information.
    /// Usage: Call static methods to parse. For example XSF.Parse(byte[])
    /// NOTE: This library does NOT handle bone scalers. Those are handled via extra information in imvu and not contained in the skeleton itself.
    /// </summary>
    public class XSF
    {
        public static readonly string HEADER = "<HEADER MAGIC=\"XSF\" VERSION=\"919\" />";
        public static readonly string SkeletonFormat = "<SKELETON NUMBONES=\"{0}\" SCENEAMBIENTCOLOR=\"{1}\">\r\n{2}</SKELETON>";

        public BaseCal3DCollection<XSFBone> Bones { get; set; }

        public XCal3DPoint3 SceneAmbientColor { get; set; }

        public XSFBone GetRootBone()
        {
            return FindBone(0);
        }

        public XSF()
        {
            this.Bones = new BaseCal3DCollection<XSFBone>();
        }

        public static XSF ParseXml(string rawData)
        {
            XSF xsf = new XSF();

            XmlDocument xmlDocument = Cal3D.GetXml(HEADER, rawData);

            XmlNodeList skeletonNodes = xmlDocument.SelectNodes("/*/SKELETON");

            if (skeletonNodes.Count != 1)
            {
                return null;
            }

            XmlNode skeletonNode = skeletonNodes[0];
            xsf.SceneAmbientColor = Cal3D.GetPoint3Attribute(skeletonNode, "SCENEAMBIENTCOLOR");

            XmlNodeList boneNodes = skeletonNode.SelectNodes("BONE");
            foreach (XmlNode boneNode in boneNodes)
            {
                XSFBone bone = new XSFBone();
                bone.Name = Cal3D.GetStringAttribute(boneNode, "NAME");
                bone.ID = Cal3D.GetIntAttribute(boneNode, "ID");
                bone.Translation = XCal3DPoint3.Parse(boneNode.SelectSingleNode("TRANSLATION"));
                bone.Rotation = XCal3DPoint4.Parse(boneNode.SelectSingleNode("ROTATION"));
                bone.LocalTranslation = XCal3DPoint3.Parse(boneNode.SelectSingleNode("LOCALTRANSLATION"));
                bone.LocalRotation = XCal3DPoint4.Parse(boneNode.SelectSingleNode("LOCALROTATION"));
                bone.ParentID = Cal3D.GetIntValue(boneNode.SelectSingleNode("PARENTID"));

                // A parent of -1 indicates a root bone.
                // For all other bones, find the parent bone and add this one to their Children collection.
                if (bone.ParentID != -1)
                {
                    var parent = xsf.FindBone(bone.ParentID);
                    if (parent != null)
                    {
                        parent.Children.Add(bone);
                    }
                    else
                    {
                        throw new Exception("Could not find parent bone with ID " + bone.ParentID.ToString());
                    }
                }

                xsf.Bones.Add(bone);
            }

            return xsf;
        }

        public static XSF ParseBinary(byte[] data)
        {
            BufferCrawler crawler = new BufferCrawler(data);

            var header = crawler.ReadString(4);

            XSF xsf = new XSF();
            var fileVersion = crawler.ReadUInt();

            var boneCount = crawler.ReadUInt();

            xsf.SceneAmbientColor = crawler.ReadPoint3();

            for (int i = 0; i < boneCount && !crawler.IsFinished; i++)
            {
                var bone = XSFBone.Parse(i, crawler, xsf);
                xsf.Bones.Add(bone);
            }

            return xsf;
        }

        /// <summary>
        /// Parses the specified data into an XSF.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>An instance of XSF.</returns>
        /// <exception cref="System.ArgumentException">Invalid XSF/CSF header.</exception>
        public static XSF Parse(byte[] data)
        {
            BufferCrawler crawler = new BufferCrawler(data);

            var header = crawler.ReadString(4);

            if (header == "<HEA")
            {
                var xmlString = System.Text.Encoding.Default.GetString(data);
                return ParseXml(xmlString);
            }
            else if (header == "CSF\0")
            {
                return ParseBinary(data);
            }

            throw new ArgumentException("Invalid XSF/CSF header.");
        }

        public string ToFormattedString()
        {
            return HEADER + Environment.NewLine +
                String.Format(SkeletonFormat, this.Bones.Count, this.SceneAmbientColor.ToFormattedString(), Bones.ToFormattedString());
        }

        public XSFBone FindBone(int id)
        {
            foreach (var bone in Bones)
            {
                var match = bone.FindByID(id);
                if (match != null) return match;
            }

            return null;
        }
    }
}