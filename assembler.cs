using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apriori
{
    public interface Assembler
    {
        DataLoader<T> GetDataLoader<T>(DataSource ds, object[] objs);
        Processor GetProcesser<T>(AttributeType at, object[] objs);
        Dictionary GetDictionary<T>(Processor proc, object[] objs);
        SupportCounter GetSupportCounter<T>(object[] objs);
    }
    public class ApAssembler : Assembler
    {
        public DataLoader<T> GetDataLoader<T>(DataSource ds, object[] objs)
        {
            if (ds == DataSource.Database) return (DataLoader<T>)Activator.CreateInstance(typeof(DBDataLoader<T>), objs);
            else return (DataLoader<T>)Activator.CreateInstance(typeof(FlatFileDataLoader<T>), objs);
        }

        public Processor GetProcesser<T>(AttributeType at, object[] objs)
        {
            if (at == AttributeType.Quantitative) return (Processor)Activator.CreateInstance(typeof(QuantitativeProcessor), objs);
            else return (Processor)Activator.CreateInstance(typeof(CategoricalProcessor<T>), objs);
        }

        public Dictionary GetDictionary<T>(Processor proc, object[] objs)
        {
            if (proc.GetType() == typeof(QuantitativeProcessor)) return (Dictionary)Activator.CreateInstance(typeof(QuantDictionary), objs);
            else return (Dictionary)Activator.CreateInstance(typeof(CategoricalDictionary<T>), objs);
        }

        public virtual SupportCounter GetSupportCounter<T>(object[] objs)
        {
            return (SupportCounter)Activator.CreateInstance(typeof(ApSupportCounter<T>), objs);
        }
    }

    public class FupAssembler : ApAssembler
    {
        public override SupportCounter GetSupportCounter<T>(object[] objs)
        {
            return (SupportCounter)Activator.CreateInstance(typeof(FupSupportCounter<T>), objs);
        }
    }

    public class IUAssembler : ApAssembler
    {
        public override SupportCounter GetSupportCounter<T>(object[] objs)
        {
            return (SupportCounter)Activator.CreateInstance(typeof(IUASupportCounter<T>), objs);
        }
    }

}
