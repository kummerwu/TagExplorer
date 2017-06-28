using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AnyTagNet.BL
{
    public class TagInf
    {
        private List<string> Alias = new List<string>();//Alias的第一个就是该Tag的名称
        private List<string> Parents = new List<string>();
        private List<string> Children = new List<string>();
        private List<string> Brothers = new List<string>();
        public TagInf() { }
        public TagInf(List<string> a, List<string> p, List<string> c, List<string> b)
        {
            Alias = a;
            Parents = p;
            Children = c;
            Brothers = b;
        }
        public List<string> GetParents() { return Parents; }
        public List<string> GetChdren() { return Children; }
        public List<string> GetBrothers() { return Brothers; }
        public static TagInf ParseTRG(string ln)
        {
            ln = ln.Replace("》", ">");
            ln = ln.Replace( "《","<");
            ln = ln.Replace( "＝","=");

            if (ln == null || ln.Length == 0) return null;
            
            ln = ln.Trim();
            //if (ln.IndexOf('=') == -1 && ln[0] != '>' && ln[0] != '<' && ln[0] != '~')
            //{
            //    ln = "=" + ln;
            //}
            if (Regex.IsMatch(ln, @"^[^=><~]+[=><~][^=><~]+$"))
            {
                Regex regSelf = new Regex(@"^[^=><~]+");
                Match mms = regSelf.Match(ln);
                string self = "";
                if(mms != null && mms.Value!=null && mms.Value!="")
                {
                    self = mms.Value;
                }
                Regex reg = new Regex("[=><~][^=><~]+");
                MatchCollection mc = reg.Matches(ln);
                List<string> tokens = new List<string>();
                foreach (Match m in mc)
                {
                    tokens.Add(m.Value);
                }
                string[] tagRelation = tokens.ToArray<string>();
                return ParseRelation(self,tagRelation);
            }
            else
            {
                return null;
            }
        }
       

        private static TagInf ParseRelation(string self,string[] lns)
        {
            TagInf t = new TagInf();
            t.Alias.Add(self);
            foreach (string ln in lns)
            {
                string tmp = ln.Trim();
                if (tmp.Length == 0) continue;
                char type = tmp[0];
                string tags = tmp.Substring(1);
                string[] tagArray = tags.Split(new char[] { ',','，' }, StringSplitOptions.RemoveEmptyEntries);
                switch (type)
                {
                    case '=':t.Alias.AddRange( tagArray.ToList<string>()); break;

                    case '<': t.Parents.AddRange(tagArray.ToList<string>()); break;

                    case '>': t.Children.AddRange(tagArray.ToList<string>()); break;

                    case '~': t.Brothers.AddRange(tagArray.ToList<string>()); break;
                }
            }
            if (t.Alias.Count == 0) return null;

            return t;
        }
        
        public override string ToString()
        {
            return ToString("\r\n");

        }

        private string ToString(string LN)
        {
            StringBuilder sb = new StringBuilder();
            if (Alias.Count > 0)
            {
                sb.Append(LN + "="); foreach (string tmp in Alias) sb.Append(tmp + ",");
            }

            if (Parents.Count > 0)
            {
                sb.Append(LN + "<"); foreach (string tmp in Parents) sb.Append(tmp + ",");
            }

            if (Children.Count > 0)
            {
                sb.Append(LN + ">"); foreach (string tmp in Children) sb.Append(tmp + ",");
            }

            if (Brothers.Count > 0)
            {
                sb.Append(LN + "~"); foreach (string tmp in Brothers) sb.Append(tmp + ",");
            }
            sb.Replace("," + LN, LN);
            return sb.ToString().Trim(',').Trim();
        }

        public List<string> GetAlias()
        {
            return Alias;
        }
        public string GetName()
        {
            if (Alias.Count > 0) return Alias[0];
            else return "";
        }
    }
}
