using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apriori
{
    public class Rule
    {
        public List<int> Antecedent;
        public List<int> Consequent;
        public double Confidence;
        public Rule(List<int> a, List<int> c, double cf)
        {
            Antecedent = a;
            Consequent = c;
            Confidence = cf;
        }
    }
}
