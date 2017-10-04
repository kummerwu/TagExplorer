using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TagExplorer.UriMgr;
using TagExplorer.Utils;

namespace TagExplorer.RichTxt
{
    /// <summary>
    /// RichTxtBox.xaml 的交互逻辑
    /// </summary>
    public partial class RichTxtBox : UserControl,IDisposable
    {
        public RichTxtBox()
        {
            InitializeComponent();
            IDisposableFactory.New<RichTxtBox>(this);
            
        }
        public new void Focus()
        {
            richTxt.Focus();
            ChangeFile(file);
        }
        private string file;
        private void ChangeFile(string f)
        {
            file = f;
            richTxt.IsEnabled = (file != null);
        }
        public void Load(string f)
        {
            try
            {
                Save();
                if (file == f) return;

                //是一个无效的rtf文件
                if (f == null || System.IO.Path.GetExtension(f).ToLower() != ".rtf")
                {
                    richTxt.Document.Blocks.Clear();
                    ChangeFile(null);
                }
                else if (!File.Exists(f))
                {
                    richTxt.Document.Blocks.Clear();
                    ChangeFile(f);
                }
                else
                {
                    TextRange range = new TextRange(richTxt.Document.ContentStart,
                                    richTxt.Document.ContentEnd);
                    FileStream fs = new FileStream(f, FileMode.Open);
                    range.Load(fs, System.Windows.DataFormats.Rtf);
                    fs.Close();
                    ChangeFile(f);
                }
            }catch(Exception e)
            {
                Logger.E(e);
                MessageBox.Show(e.Message, "打开文件失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void Save()
        {
            try
            {
                if (file != null)
                {
                    TextRange range = new TextRange(richTxt.Document.ContentStart,
                                            richTxt.Document.ContentEnd);
                    FileStream fs = new FileStream(file, FileMode.Create);
                    range.Save(fs, System.Windows.DataFormats.Rtf);
                    fs.Close();
                }
            }catch(Exception e)
            {
                Logger.E(e);
                MessageBox.Show(e.Message, "保存文件失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public void Dispose()
        {
            Save();
            file = null;
        }
    }
}
