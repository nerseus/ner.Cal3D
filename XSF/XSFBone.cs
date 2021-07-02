using System.Text;
using System.Collections.Generic;
using System;
using ner.Cal3D.Core;

namespace ner.Cal3D.XSF
{
    public class XSFBone : IFormattable
    {
        /// <summary>
        /// String.Format
        ///     0 : Name
        ///     1 : Number of child bones
        ///     2 : ID
        ///     3 : Child objects
        /// </summary>
        public static readonly string BoneFormat = "\t<BONE NAME=\"{0}\" NUMCHILDS=\"{1}\" ID=\"{2}\">\r\n{3}\t</BONE>\r\n";

        public List<XSFBone> Children { get; set; }

        private List<int> ChildrenIds { get; set; }

        public XSFBone()
        {
            this.Translation = new XCal3DPoint3();
            this.Rotation = new XCal3DPoint4();
            this.LocalTranslation = new XCal3DPoint3();
            this.LocalRotation = new XCal3DPoint4();
            this.Children = new List<XSFBone>();
            this.ChildrenIds = new List<int>();
        }

        public string Name { get; set; }
        public int ID { get; set; }
        public XCal3DPoint3 Translation { get; set; }
        public XCal3DPoint4 Rotation { get; set; }
        public XCal3DPoint3 LocalTranslation { get; set; }
        public XCal3DPoint4 LocalRotation { get; set; }
        public int ParentID { get; set; }

        public string ToFormattedString()
        {
            StringBuilder sb = new StringBuilder();
            if (Translation.Used) sb.Append($"\t\t<TRANSLATION>{Translation.ToFormattedString()}</TRANSLATION>\r\n");
            if (Rotation.Used) sb.Append($"\t\t<ROTATION>{Rotation.ToFormattedString()}</ROTATION>\r\n");
            if (LocalTranslation.Used) sb.Append($"\t\t<LOCALTRANSLATION>{LocalTranslation.ToFormattedString()}</LOCALTRANSLATION>\r\n");
            if (LocalRotation.Used) sb.Append($"\t\t<LOCALROTATION>{LocalRotation.ToFormattedString()}</LOCALROTATION>\r\n");
            sb.Append($"\t\t<PARENTID>{ParentID}</PARENTID>\r\n");

            foreach (var child in Children)
            {
                sb.Append($"\t\t<CHILDID>{child.ID}</CHILDID>\r\n");
            }

            return string.Format(BoneFormat, Name, Children.Count, ID, sb.ToString());
        }

        public static XSFBone Parse(int id, BufferCrawler crawler, XSF xsf)
        {
            if (crawler.IsFinished)
            {
                throw new ArgumentException("Missing skeleton bone data");
            }

            var bone = new XSFBone();
            bone.ID = id;
            var nameLength = crawler.ReadInt();
            bone.Name = crawler.ReadStringTrailingNull(nameLength);

            bone.Translation = crawler.ReadPoint3();
            bone.Rotation = crawler.ReadPoint4();
            bone.LocalTranslation = crawler.ReadPoint3();
            bone.LocalRotation = crawler.ReadPoint4();
            bone.ParentID = crawler.ReadInt();

            var f1 = crawler.ReadFloat();
            var f2 = crawler.ReadFloat();
            var f3 = crawler.ReadFloat();
            var f4 = crawler.ReadFloat();
            crawler.currentPos -= (4 * 4);

            var u1 = crawler.ReadUInt();
            var u2 = crawler.ReadUInt();
            var u3 = crawler.ReadUInt();
            var u4 = crawler.ReadUInt();

            if (f1 != 0f
                || f2 != 0f
                || f3 != 0f
                || f4 != 0f
                || u1 != 0
                || u2 != 0
                || u3 != 0
                || u4 != 0
                )
            {
                System.Diagnostics.Debugger.Break();
            }

            var numChildren = crawler.ReadUInt();

            for (int i = 0; i < numChildren; i++)
            {
                var childId = crawler.ReadInt();
                bone.ChildrenIds.Add(childId);
            }

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

            return bone;
        }

        public XSFBone FindByID(int id)
        {
            if (ID == id) return this;

            foreach (var child in Children)
            {
                var match = child.FindByID(id);
                if (match != null) return match;
            }

            return null;
        }
    }
}