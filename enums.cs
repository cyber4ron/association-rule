using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apriori
{
    public enum DataSource
    {
        Database,
        Excel,
        Flat
    };

    public enum AttributeType
    {
        Quantitative,
        Categorical
    };

    public enum Method
    {
        Apriori,
        FPGrowth
    };
}
