using System;

namespace ner.Cal3D.XMF
{
    public class XMFSpring : IFormattable
    {
        public int Vertex1 { get; set; }

        public int Vertex2 { get; set; }

        public float Coefficient { get; set; }

        public float Idle { get; set; }

        public static XMFSpring Parse(BufferCrawler crawler)
        {
            if (crawler.IsFinished)
            {
                throw new ArgumentException("Missing spring data");
            }

            XMFSpring spring = new XMFSpring();
            spring.Vertex1 = crawler.ReadInt();
            spring.Vertex2 = crawler.ReadInt();
            spring.Coefficient = crawler.ReadFloat();
            spring.Idle = crawler.ReadFloat();

            return spring;
        }

        public string ToFormattedString()
        {
            return null;
        }
    }
}