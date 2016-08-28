using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace apriori
{
    public interface DataLoader<T>
    {
        List<List<T>> LaodData();
        List<List<T>> Covert(object obj);
        List<List<T>> Shuffle(List<List<T>> llt, int steps);
    }

    public class DBDataLoader<T> : DataLoader<T>
    {
        DBAccess dba;
        public string CS, Query;//ref
        public bool Quantative;


        public DBDataLoader(Apriori<T> ap)
        {
            CS = ap.DsrcParams[0];
            Query = ap.DsrcParams[1];
            dba = new DBAccess(CS);
        }

        public DBDataLoader(string cs, string q)
        {
            CS = cs;
            Query = q;
            dba = new DBAccess(CS);
        }//for external call

        public List<List<T>> LaodData()
        {
            return Covert(dba.ReadData(Query));
        }

        public List<List<T>> Covert(object t)
        {
            if (typeof(T) == typeof(double))//double
            {
                List<List<double>> lld = new List<List<double>>();
                List<double> ld;
                foreach (DataRow r in (t as DataTable).Rows)
                {
                    ld = new List<double>();

                    for (int i = 0; i < r.ItemArray.Count(); i++)
                    {
                        ld.Add(Convert.ToDouble(r.ItemArray[i].ToString()));
                    }
                    lld.Add(ld);
                }

                return lld as List<List<T>>;
            }
            else if (typeof(T) == typeof(int))//int
            {
                List<List<int>> lli = new List<List<int>>();
                List<int> ld;
                foreach (DataRow r in (t as DataTable).Rows)
                {
                    ld = new List<int>();

                    for (int i = 0; i < r.ItemArray.Count(); i++)
                    {
                        ld.Add(Convert.ToInt32(r.ItemArray[i].ToString()));
                    }
                    lli.Add(ld);
                }

                return lli as List<List<T>>;
            }
            else//string
            {
                List<List<string>> lli = new List<List<string>>();
                List<string> ld;
                foreach (DataRow r in (t as DataTable).Rows)
                {
                    ld = new List<string>();

                    for (int i = 0; i < r.ItemArray.Count(); i++)
                    {
                        ld.Add(r.ItemArray[i].ToString());
                    }
                    lli.Add(ld);
                }

                return lli as List<List<T>>;
            }

        }

        public List<List<T>> Shuffle(List<List<T>> llt, int steps)
        {
            return llt;
        }
    }

    public class FlatFileDataLoader<T> : DataLoader<T>
    {
        string Path;

        public FlatFileDataLoader(Apriori<T> ap)
        {
            Path = ap.DsrcParams[0];
        }

        public List<List<T>> LaodData()
        {
            StreamReader sr = new StreamReader(Path);
            string s;
            List<List<T>> llt = new List<List<T>>();

            while ((s = sr.ReadLine()) != null)
            {
                List<string> sa = s.Trim().Split(new char[] { ' ' }).ToList();

                if (typeof(T) == typeof(double)) llt.Add(sa.Select(x => Convert.ToDouble(x)).ToList() as List<T>);//double
                else if (typeof(T) == typeof(int)) llt.Add(sa.Select(x => Convert.ToInt32(x)).ToList() as List<T>);//int
                else llt.Add(sa as List<T>);//string
            }

            return llt;
        }

        public List<List<T>> Covert(object obj) { return null; }

        public List<List<T>> Shuffle(List<List<T>> llt, int steps)
        {
            Random r = new Random();
            int Len = llt.Count;
            int s, t;
            List<T> temp;
            for (int i = 0; i < steps; i++)
            {
                s = r.Next(Len);
                t = r.Next(Len);

                if (s != t)
                {
                    temp = llt[s];
                    llt[s] = llt[t];
                    llt[t] = temp;
                }
            }

            return llt;
        }
    }
}
