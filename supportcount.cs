using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apriori.incap;


namespace apriori
{
    public abstract class SupportCounter
    {
        public Dictionary<string, int> SuptCnt;

        public abstract int GetCount(List<int> li);
    }

    public class ApSupportCounter<T> : SupportCounter
    {
        List<List<int>> DSProcessed;
        List<List<Item>> L;

        public ApSupportCounter(Apriori<T> ap)
        {
            SuptCnt = new Dictionary<string, int>();
            DSProcessed = ap.DSProcessed;
            L = ap.L;
        }

        public void Add(string s, int i)
        {
            SuptCnt.Add(s, i);
        }

        public string ToString(List<int> li)
        {
            string s = "";
            foreach (int i in li)
            {
                s += i.ToString() + " ";
            }
            return s;
        }

        public override int GetCount(List<int> li)
        {
            int sc;
            string s = ToString(li);
            bool finded;

            if (SuptCnt.TryGetValue(s, out sc)) return sc;
            else
            {
                finded = false;
                if (L.Count >= li.Count)
                {
                    foreach (Item it in L[li.Count - 1])
                        if (it.Pattern.Intersect(li).Count() == li.Count)
                        {
                            finded = true;
                            sc = it.Count;
                        }
                }

                if (!finded)
                {
                    sc = 0;
                    foreach (List<int> lin in DSProcessed)
                    {
                        if (lin.Intersect(li).Count() == li.Count) sc++;
                    }
                }
            }

            SuptCnt.Add(ToString(li), sc);

            return sc;
        }
    }

    public class IUASupportCounter<T> : ApSupportCounter<T>
    {
        public IUASupportCounter(IUA<T> iua):
            base(iua)
        {
            if (iua.Ap.SC.SuptCnt!=null) SuptCnt = iua.Ap.SC.SuptCnt;
        }
    }

    public class FupSupportCounter<T> : SupportCounter
    {
        Apriori<T> Ap1, Ap2;

        public FupSupportCounter(FUP<T> fup)
        {
            Ap1 = fup.Ap1;
            Ap2 = fup.Ap2;
        }

        public override int GetCount(List<int> li)
        {
            return Ap1.SC.GetCount(li) + Ap2.SC.GetCount(li);

        }

    }
}
