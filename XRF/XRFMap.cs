namespace ner.Cal3D.XRF
{
    public class XRFMap : IFormattable
    {
        public string Type { get; set; }
        public string AssetName { get; set; }

        public string ToFormattedString()
        {
            return $"\t<MAP TYPE=\"{Type}\">{AssetName}</MAP>\r\n";
        }
    }
}