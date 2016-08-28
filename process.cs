using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apriori
{
    public abstract class Processor
    {
        public abstract List<List<int>> Process();
    }

    public class CategoricalProcessor<T> : Processor//double, int, string
    {
        public List<List<T>> DS;//ref
        public Dictionary<T, int> Dict;
        public List<T> Keys;

        public CategoricalProcessor(Apriori<T> ap)
        {
            DS = ap.DS;
            Dict = new Dictionary<T, int>();
            Keys = new List<T>();
        }

        public override List<List<int>> Process()
        {
            List<List<int>> lli = new List<List<int>>();
            int id, idx = -1;
            List<int> li;
            for (int i = 0; i < DS.Count; i++)
            {
                li = new List<int>();
                for (int j = 0; j < DS[i].Count; j++)
                {
                    if (Dict.TryGetValue(DS[i][j], out id)) li.Add(id);
                    else
                    {
                        Dict.Add(DS[i][j], ++idx); li.Add(idx);
                        Keys.Add(DS[i][j]);
                    }
                }
                lli.Add(li);
            }
            return lli;
        }
    }

    public class QuantitativeProcessor : Processor//double
    {
        public List<List<double>> DS;//ref
        public List<List<double>> Fractiles;//ref
        public List<int> Fractions;//ref
        public int ClsCnt;//ref

        public QuantitativeProcessor(Apriori<double> ap)
        {
            DS = ap.DS;
            Fractions = ap.Fractions;
            if (ap.Fractiles == null)
            {
                Fractiles = CalculateFractile(Transpose());
                ap.Fractiles = Fractiles;
            }
            else Fractiles = ap.Fractiles;
            ClsCnt = ap.ClsCnt;
        }

        public QuantitativeProcessor(List<List<double>> ds, List<int> f, int c)
        {
            DS = ds;
            Fractions = f;
            Fractiles = CalculateFractile(Transpose());
            ClsCnt = c;
        }

        public QuantitativeProcessor(List<List<double>> ds, List<int> f, List<List<double>> ft, int c)
        {
            DS = ds;
            Fractiles = ft;
            Fractions = f;
            ClsCnt = c;
        }

        public List<List<double>> Transpose()
        {
            List<List<double>> lldt = new List<List<double>>();
            List<double> ld = new List<double>();

            for (int i = 0; i < DS[0].Count; i++)
            {
                ld = new List<double>();
                for (int j = 0; j < DS.Count; j++)
                {
                    ld.Add(DS[j][i]);
                }
                lldt.Add(ld);
            }
            return lldt;
        }

        public List<List<double>> CalculateFractile(List<List<double>> lld)//lld - DS transposed
        {
            int interval;
            List<double> ld;
            Fractiles = new List<List<double>>();

            for (int i = 0; i < lld.Count; i++)
            {
                ld = new List<double>();
                for (int j = 0; j < lld[i].Count; j++)
                {
                    ld.Add(lld[i][j]);
                }

                ld.Sort();

                interval = (lld[i].Count - 1) / Fractions[i];

                Fractiles.Add(new List<double>());
                for (int j = 0; j < Fractions[i] + 1; j++)
                {
                    Fractiles[i].Add(ld[j * interval]);
                }
            }

            return Fractiles;
        }

        public List<int> Discrete(List<double> ld)
        {
            List<int> li = new List<int>();
            for (int i = 0; i < ld.Count; i++)//(,]
            {
                if (ld[i] <= Fractiles[i][0]) li.Add(0);
                else if (ld[i] > Fractiles[i][Fractiles[i].Count - 1]) li.Add(Fractiles[i].Count);
                else for (int j = 0; j < Fractiles[i].Count - 1; j++)
                    {
                        if (ld[i] > Fractiles[i][j] && ld[i] <= Fractiles[i][j + 1]) li.Add(j + 1);
                    }
            }

            return li;
        }

        public override List<List<int>> Process()
        {
            List<List<int>> lli = new List<List<int>>();
            for (int i = 0; i < DS.Count; i++)
            {
                lli.Add(Discrete(DS[i]));
            }
            return Transform(lli);
        }

        public List<List<int>> Transform(List<List<int>> lli)//lli - untransformed, 0-based
        {
            List<List<int>> llir = new List<List<int>>();
            List<int> li;

            for (int i = 0; i < lli.Count; i++)
            {
                li = new List<int>();
                for (int j = 0; j < lli[i].Count; j++)
                {
                    li.Add(Fractions.Take(j).Sum(x => x + 2) + lli[i][j]);

                }
                llir.Add(li);
            }

            return llir;
        }

    }
}
