using ner.Cal3D.Core;
using ner.Cal3D.XMF;
using System;

namespace ner.Cal3D
{
    /// <summary>
    /// A rudimentary class to "crawl" a byte array. This is intended to help parse binary files by offering methods that simulate "reading" common values and moving a "read pointer" in the file.
    /// </summary>
    public class BufferCrawler
    {
        public byte[] data { get; set; }
        public int currentPos = 0;

        public bool IsFinished
        {
            get
            {
                return currentPos >= data.Length;
            }
        }

        public BufferCrawler(byte[] data)
        {
            this.data = data;
        }

        public uint ReadUInt()
        {
            var val = BitConverter.ToUInt32(data, currentPos);
            currentPos += 4;
            return val;
        }

        public int ReadInt()
        {
            var val = BitConverter.ToUInt32(data, currentPos);
            currentPos += 4;
            return (int)val;
        }

        public byte ReadByte()
        {
            var val = data[currentPos];
            currentPos += 1;
            return val;
        }

        public float ReadFloat()
        {
            var val = BitConverter.ToSingle(data, currentPos);
            currentPos += 4;
            return val;
        }

        public byte[] ReadBytes(int numberOfBytes)
        {
            var copiedData = new byte[numberOfBytes];
            Array.Copy(data, currentPos, copiedData, 0, numberOfBytes);
            currentPos += numberOfBytes;
            return copiedData;
        }

        public string ReadString(int numberOfBytes)
        {
            var bytes = ReadBytes(numberOfBytes);
            return System.Text.Encoding.Default.GetString(bytes);
        }

        public string ReadStringWithCountPrefix()
        {
            var nameLength = ReadInt();
            var bytes = ReadBytes(nameLength);
            var str = System.Text.Encoding.Default.GetString(bytes);
            str = str.Substring(0, str.Length - 1);
            return str;
        }

        public string ReadStringTrailingNull(int numberOfBytes)
        {
            var bytes = ReadBytes(numberOfBytes);
            var str = System.Text.Encoding.Default.GetString(bytes);
            str = str.Substring(0, str.Length - 1);
            return str;
        }

        public XCal3DPoint3 ReadPoint3()
        {
            XCal3DPoint3 point = new XCal3DPoint3();
            point.X = ReadFloat();
            point.Y = ReadFloat();
            point.Z = ReadFloat();
            point.Used = true;

            return point;
        }

        public XCal3DPoint4 ReadPoint4()
        {
            XCal3DPoint4 point = new XCal3DPoint4();
            point.X = ReadFloat();
            point.Y = ReadFloat();
            point.Z = ReadFloat();
            point.A = ReadFloat();
            point.Used = true;

            return point;
        }

        public XMFTextureCoordinate ReadTexCoord()
        {
            var point = new XMFTextureCoordinate();
            point.X = ReadFloat();
            point.Y = ReadFloat();
            point.Used = true;

            return point;
        }

        public XCal3DByte4 ReadByte4()
        {
            XCal3DByte4 point = new XCal3DByte4();
            point.X = ReadByte();
            point.Y = ReadByte();
            point.Z = ReadByte();
            point.A = ReadByte();
            point.Used = true;

            return point;
        }
    }
}
