using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apriori
{
    public interface Dictionary
    {
        void GenerateDictionary();
        List<string> Translate(Rule r);
        List<List<string>> Translate(List<Rule> lr);
    }

    public class QuantDictionary : Dictionary//double
    {
        List<string> Attrs;
        List<List<double>> Fractiles;//ref
        List<int> Fractions;//ref
        int ClsCnt;
        Dictionary<int, string> Dic;

        public QuantDictionary(QuantitativeProcessor d)
        {
            Fractiles = d.Fractiles;
            Fractions = d.Fractions;
            ClsCnt = d.ClsCnt;
            Dic = new Dictionary<int, string>();

            Attrs = new List<string>();
            Attrs.Add("每股收益");
            Attrs.Add("净资产收益率");
            Attrs.Add("资产报酬率");
            Attrs.Add("营业利润率");
            Attrs.Add("流动比率");
            Attrs.Add("速动比率");
            Attrs.Add("每股收益增长率");
            Attrs.Add("营业收入增长率");
            Attrs.Add("总资产增长率");
            Attrs.Add("存货周转率");
            Attrs.Add("应收账款周转率");
            Attrs.Add("流动资产周转率");
            Attrs.Add("销售商品劳务收入现金/营业收入");
            Attrs.Add("资产负债率");
            Attrs.Add("分类");
        }

        public void GenerateDictionary()
        {
            for (int i = 0; i < Fractiles.Count - 1; i++)
            {
                Dic.Add(Fractions.Take(i).Sum(x => x + 2), Attrs[i] + " < " + Fractiles[i][0]);

                for (int j = 1; j < Fractiles[i].Count; j++)
                {
                    Dic.Add(Fractions.Take(i).Sum(x => x + 2) + j, String.Format("{0} ∈ [{1}, {2})", Attrs[i], Fractiles[i][j - 1], Fractiles[i][j]));
                }

                Dic.Add(Fractions.Take(i).Sum(x => x + 2) + Fractiles[i].Count, Attrs[i] + " >= " + Fractiles[i][Fractiles[i].Count - 1]);
            }

            for (int i = 0; i < ClsCnt; i++)
            {
                Dic.Add(Fractions.Take(Fractions.Count - 1).Sum(x => x + 2) + i, "Class " + i);
            }

        }

        public List<string> Translate(Rule r)
        {
            List<string> ls = new List<string>();
            string s1 = "";
            for (int i = 0; i < r.Antecedent.Count - 1; i++)
            {
                s1 += Dic[r.Antecedent[i]] + " and ";
            }
            s1 += Dic[r.Antecedent[r.Antecedent.Count - 1]];

            string s2 = "";
            for (int i = 0; i < r.Consequent.Count - 1; i++)
            {
                s2 += Dic[r.Consequent[i]] + " and ";
            }
            s2 += Dic[r.Consequent[r.Consequent.Count - 1]];

            ls.Add(s1);
            ls.Add(s2);
            string s = r.Confidence.ToString();
            ls.Add(s.Substring(0, Math.Min(s.Length, 6)));

            return ls;
        }

        public List<List<string>> Translate(List<Rule> lr)
        {
            List<List<string>> lls = new List<List<string>>();

            foreach (Rule r in lr)
            {
                lls.Add(Translate(r));
            }

            return lls;
        }
    }

    public class CategoricalDictionary<T> : Dictionary//double, int, string
    {
        Dictionary<T, int> DictKey;
        Dictionary<int, T> Dict;
        List<T> Keys;

        public CategoricalDictionary(CategoricalProcessor<T> p)
        {
            DictKey = p.Dict;
            Keys = p.Keys;
            Dict = new Dictionary<int, T>();

        }

        public void GenerateDictionary()
        {

            foreach (T s in Keys)
            {
                Dict.Add(DictKey[s], s);
            }
        }

        public List<string> Translate(Rule r)
        {
            List<string> ls = new List<string>();

            ls.Add(r.Antecedent.Select(x => Dict[x].ToString()).Aggregate((c, n) => (c + " " + n)));
            ls.Add(r.Consequent.Select(x => Dict[x].ToString()).Aggregate((c, n) => (c + " " + n)));
            ls.Add(r.Confidence.ToString());

            return ls;
        }

        public List<List<string>> Translate(List<Rule> lr)
        {
            List<List<string>> lls = new List<List<string>>();
            foreach (Rule r in lr)
            {
                lls.Add(Translate(r));
            }

            return lls;
        }
    }
}
