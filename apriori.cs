using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apriori
{
    public class Apriori<T>
    {
        #region Vars

        public DataLoader<T> DL;// used when DS is null
        public Processor Proc;//used to calculate fractiles, discretize and transform DS, CalculateFractile() is called when fractiles is not given
        public SupportCounter SC;//apriori support counter/ fup support counter
        public Dictionary Dict;//mapping

        public List<List<T>> DS;
        public List<List<int>> DSProcessed;//DS Discreted and transformed

        public List<int> Fractions;
        public int ClsCnt;
        public List<List<double>> Fractiles;

        public List<List<Item>> L;

        public List<Rule> Rules;
        public List<List<string>> RulesTranslated;

        public double MinSupportDbl;
        public int MinSupportInt;
        public double MinConfidence;

        public List<string> DsrcParams;

        public Assembler AS;//get instances
        public DataSource DSrc;
        public AttributeType AT;

        public FPGrowth FP_Growth;

        public Method Mtd;

        #endregion

        public Apriori(DataSource ds, AttributeType at, List<string> dp, double ms, double mc, List<int> f, List<List<double>> ft, int c, Assembler a)
        //dataset is not given
        {
            DSrc = ds;
            AT = at;
            DsrcParams = dp;

            L = new List<List<Item>>();
            Rules = new List<Rule>();

            MinSupportDbl = ms;
            MinConfidence = mc;

            Fractiles = ft;
            Fractions = f;
            ClsCnt = c;
            if (ClsCnt != 0 && Fractions != null) Fractions.Add(ClsCnt - 1);

            AS = a;
            Mtd = Method.Apriori;
        }

        public Apriori(DataSource ds, AttributeType at, List<List<T>> llt, double ms, double mc, List<int> f, List<List<double>> ft, int c, Assembler a)
        //dataset is given
        {
            DSrc = ds;
            AT = at;

            DS = llt;
            if (typeof(int) == typeof(T)) DSProcessed = DS as List<List<int>>;

            L = new List<List<Item>>();
            Rules = new List<Rule>();

            MinSupportDbl = ms;
            MinConfidence = mc;

            Fractiles = ft != null ? ft : new List<List<double>>();
            Fractions = f;
            ClsCnt = c;
            if (ClsCnt != 0 && Fractions != null) Fractions.Add(ClsCnt - 1);

            AS = a;
            Mtd = Method.Apriori;
        }

        public void ApInit()
        {
            if (DS == null && DSProcessed == null)
            {
                DL = AS.GetDataLoader<T>(DSrc, new object[] { this });
                DS = DL.LaodData();
            }

            if (Proc == null && DSProcessed == null)
            {
                Proc = AS.GetProcesser<T>(AT, new object[] { this });
            }

            if (DSProcessed == null) 
                DSProcessed = Proc.Process();

            if (Dict == null && typeof(T) != typeof(int))
            {
                Dict = AS.GetDictionary<T>(Proc, new object[] { Proc });
                Dict.GenerateDictionary();
            }

            if (SC == null) SC = AS.GetSupportCounter<T>(new object[] { this });

            MinSupportInt = (int)(DSProcessed.Count * MinSupportDbl);
        }

        public List<Item> OneFrequentItemsetAlt()
        {

            List<int> tDS = DSProcessed.Aggregate((cur, next) => cur.Concat(next).ToList()).ToList();

            List<int> tL = tDS.Where(curw => tDS.Count(curc => curc == curw) >= MinSupportInt).Distinct().ToList();

            tL.Sort();

            List<Item> L = new List<Item>();
            foreach (int i in tL)
            {
                Item it = new Item(new List<int> { i });
                it.Count = tDS.Count(x => x == it.Pattern[0]);
                L.Add(it);
            }

            return L;
        }

        public List<Item> OneFrequentItemset()
        {
            List<Item> lit = new List<Item>();

            int max = DSProcessed.Max(x => x.Max());

            List<int> hash = Enumerable.Repeat(0, max + 1).ToList();

            foreach (List<int> li in DSProcessed)
            {
                foreach (int i in li)
                {
                    hash[i]++;
                }
            }

            for (int i = 0; i < hash.Count; i++)
            {
                if (hash[i] >= MinSupportInt) lit.Add(new Item(new List<int>() { i }, hash[i]));
            }

            return lit;
        }

        public List<Item> KFrequentItemset(List<Item> FrequentItemSet)
        {
            List<List<int>> tC = new List<List<int>>();

            foreach (Item i1 in FrequentItemSet)
            {
                foreach (Item i2 in FrequentItemSet)
                {
                    int cnt = i1.Pattern.Count;

                    for (int i = 0; i < cnt - 1; i++)
                    {
                        if (i1.Pattern[i] != i2.Pattern[i]) goto E;
                    }

                    if (i1.Pattern[cnt - 1] < i2.Pattern[cnt - 1])
                    {
                        List<int> tl = i1.Pattern.Intersect(i2.Pattern).ToList();

                        tl.Add(i1.Pattern[cnt - 1]);
                        tl.Add(i2.Pattern[cnt - 1]);

                        tC.Add(tl);
                    }
                E: ;
                }
            }

            List<Item> C = tC.Select(x => new Item(x)).ToList();

            foreach (Item it in C)
            {
                it.Count = SC.GetCount(it.Pattern);
            }

            List<Item> L = new List<Item>();
            foreach (Item it in C)
            {
                if (it.Count >= MinSupportInt) L.Add(it);
            }

            return L;
        }

        public void GenerateFrequentItemsetList()
        {
            L.Add(OneFrequentItemset());

            for (int k = 1; L[k - 1].Count != 0; k++)
            {
                L.Add(KFrequentItemset(L[k - 1]));
            }

        }

        public void GeneratePattern(List<int> CurPattern, List<int> Pattern, int i, int n)
        {
            if (i == n)
                if (CurPattern.Count != 0 && CurPattern.Count < n)
                {
                    List<int> a = new List<int>();
                    List<int> c = new List<int>();
                    a = CurPattern.Select(x => x).ToList();
                    c = Pattern.Except(CurPattern).ToList();
                    double cf = 1.0 * SC.GetCount(Pattern) / SC.GetCount(a);
                    if (cf > MinConfidence) Rules.Add(new Rule(a, c, cf));
                }
                else return;
            else
            {
                CurPattern.Add(Pattern[i]);
                GeneratePattern(CurPattern, Pattern, i + 1, n);
                CurPattern.Remove(Pattern[i]);
                GeneratePattern(CurPattern, Pattern, i + 1, n);
            }

        }

        public void GenerateRules()
        {
            for (int i = 1; i < L.Count; i++)
            {
                foreach (Item it in L[i])
                    GeneratePattern(new List<int>(), it.Pattern, 0, it.Pattern.Count);
            }
            Rules = Rules.OrderByDescending(r => r.Confidence).ToList();

            //RulesTranslated = Dict.Translate(Rules);
        }

        public virtual void SurpportDown() { }
        public virtual void SurpportUp() { }
        public virtual void ConfidenceDown() { }
        public virtual void ConfidenceUp() { }
        public virtual void FupMainMethod() { }

        public void ApMainMethod()
        {
            ApInit();

            if (Mtd == Method.FPGrowth)
            {
                FP_Growth = new FPGrowth(DSProcessed, MinSupportDbl);

                FPTree fpt = FP_Growth.BuildTree(DSProcessed.Select(x => new Pattern(x, 1)).ToList());

                FP_Growth.GeneratePatterns(fpt, new Pattern(new List<int>(), Int32.MaxValue));

                FP_Growth.GenerateL();

                L = FP_Growth.L;
            }
            else GenerateFrequentItemsetList();

            GenerateRules();
        }
    }
}