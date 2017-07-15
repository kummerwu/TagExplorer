using Lucene.Net.Analysis;
using Lucene.Net.Analysis.NGram;
using System.IO;

namespace TagExplorer.UriMgr
{
    class UriAnalyser : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {

            //NGramTokenizer source = new NGramTokenizer(reader,1,3);
            //Tokenizer source = new StandardTokenizer( Lucene.Net.Util.Version.LUCENE_30,reader);
            Tokenizer source = new KeywordTokenizer(reader);
            TokenFilter filter = new LowerCaseFilter(source);
            filter = new NGramTokenFilter(filter, 1,256);
            
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
