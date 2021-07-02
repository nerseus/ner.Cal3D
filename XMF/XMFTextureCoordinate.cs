using ner.Cal3D.Core;

namespace ner.Cal3D.XMF
{
    public class XMFTextureCoordinate : XCal3DPoint2, IFormattable
    {
        public int NumIndentTabs = 3;

        public new string ToFormattedString()
        {
            var tabs = new string('\t', NumIndentTabs);
            return tabs + "<TEXCOORD>" + base.ToFormattedString() + "</TEXCOORD>\r\n";
        }
    }
}