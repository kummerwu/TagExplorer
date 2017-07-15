using Lucene.Net.Analysis;
using System.IO;

namespace TagExplorer.UriMgr
{
    class UriQueryAnalyser :Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {

            //NGramTokenizer source = new NGramTokenizer(reader,1,3);
            //Tokenizer source = new StandardTokenizer( Lucene.Net.Util.Version.LUCENE_30,reader);
            Tokenizer source = new KeywordTokenizer(reader);
            TokenFilter filter = new LowerCaseFilter(source);

            return filter;
        }
        
    }
}
