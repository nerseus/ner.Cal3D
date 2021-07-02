using System.Collections.Generic;
using System.Text;

namespace ner.Cal3D.Core
{
    /// <summary>
    /// A base collection class. This wraps a List and the IFormattable interface to support writing out the collection's items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseCal3DCollection<T> : List<T> where T : IFormattable, new()
    {
        public string ToFormattedString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in this)
            {
                sb.Append(item.ToFormattedString());
            }

            return sb.ToString();
        }
    }
}