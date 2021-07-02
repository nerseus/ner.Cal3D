using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ner.Cal3D.XMF
{
    public class XMFInfluenceCollection : List<XMFInfluence>
    {
        public void NormalizeInfluences()
        {
            // Create a list of distinct influences. This is needed in case a vertex lists the same bone more than once.
            // For example, if a vertex has:
            //      ID=0 : Influence = 0.2
            //      ID=0 : Influence = 0.3
            //      ID=1 : Influence = 0.4
            // Then distinctInfluences will have:
            //      ID=0 : Influence = 0.5 (sum of all influences for ID = 0)
            //      ID=1 : Influence = 0.4 (sum of all influences for ID = 1)
            // Take only the first 4 biggest influences. If there are more they will be dropped.
            var distinctInfluences = this
                .GroupBy(x => x.BoneID)
                .Select(grp =>
                    new
                    {
                        BoneID = grp.First().BoneID,
                        Influence = grp.Sum(x => x.Influence)
                    })
                    .OrderByDescending(x => x.Influence)
                    .Take(4)
                    .ToList();

            var sum = this.Sum(x => x.Influence);

            this.Clear();

            // Add all but the last influence.
            this.AddRange(
                distinctInfluences.Select(i => new XMFInfluence
                {
                    BoneID = i.BoneID,
                    Influence = i.Influence / sum
                })
                    .Take(distinctInfluences.Count - 1)
            );

            // recalculate the sum of all items except the last.
            sum = this.Sum(x => x.Influence);

            // Create the last influence, guaranteeing the influence will add up to exactly 1f.
            this.Add(new XMFInfluence
            {
                BoneID = distinctInfluences[distinctInfluences.Count - 1].BoneID,
                Influence = 1f - sum
            });
        }

        public string ToFormattedString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (XMFInfluence influence in this)
            {
                sb.Append(influence.ToFormattedString());
            }

            return sb.ToString();
        }

        public void Add(int boneId, float influenceValue)
        {
            XMFInfluence influence = new XMFInfluence() { BoneID = boneId, Influence = influenceValue };
            this.Add(influence);
        }

        public XMFInfluence Find(int boneId)
        {
            return this.FirstOrDefault(influence => influence.BoneID == boneId);
        }
    }
}