using Lucene.Net.Analysis;
using Lucene.Net.Analysis.NGram;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneTest.UriMgr
{
    public class UriTokenizer : CharTokenizer
    {
        public UriTokenizer(TextReader input) : base(input)
        {
        }

        protected override bool IsTokenChar(char c)
        {
            return true;
        }
    }
    class UriQueryAnalyser:Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {

            //NGramTokenizer source = new NGramTokenizer(reader,1,3);
            //Tokenizer source = new StandardTokenizer( Lucene.Net.Util.Version.LUCENE_30,reader);
            Tokenizer source = new KeywordTokenizer(reader);
            TokenFilter filter = new LowerCaseFilter(source);

            return filter;
        }
        public /*override*/  TokenStream TokenStream1(string fieldName, TextReader reader)
        {

            //NGramTokenizer source = new NGramTokenizer(reader,1,3);
            //Tokenizer source = new StandardTokenizer( Lucene.Net.Util.Version.LUCENE_30,reader);
            Tokenizer source = new KeywordTokenizer(reader);
            TokenFilter filter = new LowerCaseFilter(source);

            return filter;
        }
    }
    class UriAnalyser : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {

            //NGramTokenizer source = new NGramTokenizer(reader,1,3);
            //Tokenizer source = new StandardTokenizer( Lucene.Net.Util.Version.LUCENE_30,reader);
            Tokenizer source = new KeywordTokenizer(reader);
            TokenFilter filter = new LowerCaseFilter(source);
            filter = new NGramTokenFilter(filter, 1, 50);

            return filter;
        }
        public /*override*/  TokenStream TokenStream1(string fieldName, TextReader reader)
        {

            //NGramTokenizer source = new NGramTokenizer(reader,1,3);
            //Tokenizer source = new StandardTokenizer( Lucene.Net.Util.Version.LUCENE_30,reader);
            Tokenizer source = new UriTokenizer(reader);
            TokenFilter filter = new LowerCaseFilter(source);
            filter = new NGramTokenFilter(filter, 1, 50);

            return filter;
        }
        //public static string dbg(string text)
        //{
        //    Analyzer analyzer = new MyAnalyzer();
        //    StringReader reader = new StringReader(text);
        //    TokenStream tokenStream = analyzer.TokenStream("", reader);

        //    StringBuilder sb = new StringBuilder();
        //    // 递归处理所有语汇单元  
        //    while (tokenStream.IncrementToken())
        //    {
        //        string s = tokenStream.ToString();
        //        sb.AppendLine(s);
        //    }
        //    Console.Write(sb.ToString());
        //    return sb.ToString();
        //}
    }
}
