using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            //richTxt.IsEnabled = (file != null && File.Exists(file));
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
        private void EnsureDirExist(string f)
        {
            DirectoryInfo dir = new FileInfo(f).Directory;
            EnsureCreateDir(dir);
        }
        private void EnsureCreateDir(DirectoryInfo dir)
        {

            if (!dir.Exists)
            {
                DirectoryInfo parent = dir.Parent;
                if (!parent.Exists)
                {
                    EnsureCreateDir(parent);
                    
                }
                dir.Create();
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
                    if (range.IsEmpty)
                    {
                        CfgPath.MoveToRecycle(file);
                    }
                    else
                    {
                        if (!File.Exists(file))
                        {
                            EnsureDirExist(file);
                        }
                        FileStream fs = new FileStream(file, FileMode.Create);
                        range.Save(fs, System.Windows.DataFormats.Rtf);
                        fs.Close();
                    }
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

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {

                BitmapSource bs = Clipboard.GetImage();
                Image image = new Image();
                image.Width = bs.Width;
                image.Height = bs.Height;
                image.Stretch = Stretch.None;
                image.Source = bs;

                InlineUIContainer MyUI = new InlineUIContainer(image, richTxt.Selection.Start);


                Paragraph myParagraph = new Paragraph();
                myParagraph.Inlines.Add(MyUI);

                //object ortf = Clipboard.GetData(DataFormats.Rtf);

                //Clipboard.SetData(DataFormats.Rtf, ortf);
                richTxt.Paste();

                
                
            }
            else
            {
                richTxt.Paste();
            }
            //object o = Clipboard.GetData(DataFormats.Html);
            AdjustImg();

        }
        private void AdjustImg()
        {
            
                foreach (Block block in richTxt.Document.Blocks)
                {
                    if (block is Paragraph)
                    {
                        Paragraph paragraph = (Paragraph)block;
                        foreach (Inline inline in paragraph.Inlines)
                        {
                            if (inline is InlineUIContainer)
                            {
                                InlineUIContainer uiContainer = (InlineUIContainer)inline;
                                if (uiContainer.Child is Image)
                                {
                                    Image image = (Image)uiContainer.Child;
                                    image.Width = image.ActualWidth + 1;
                                    image.Height = image.ActualHeight + 1;
                                }
                            }
                        }
                    }
                }
            
        }
        
        
    }
}
