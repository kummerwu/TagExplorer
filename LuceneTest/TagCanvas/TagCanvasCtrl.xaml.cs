﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagExplorer.TagLayout;
using TagExplorer.TagLayout.CommonLayout;
using TagExplorer.TagLayout.TreeLayout;
using TagExplorer.TagMgr;
using TagExplorer.UriMgr;
using TagExplorer.Utils;
using TagExplorer.Utils.Cfg;

namespace TagExplorer.TagCanvas
{
    public delegate void CurrentTagChanged(GUTag tag);
    /// <summary>
    /// TagCanvasCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class TagCanvasCtrl : UserControl
    {
        private ITagDB TagDB = null;
        private IUriDB UriDB = null;
        private LayoutCanvas CanvasType;

        //初始化TagDB，该函数必须在空间初始化时就指定
        public void Initial(ITagDB db, IUriDB uridb,LayoutMode mode,LayoutCanvas canvasType)
        {
            myMode = mode;
            TagDB = db;
            UriDB = uridb;
            CanvasType = canvasType;
        }
        public TagCanvasCtrl()
        {
            InitializeComponent();
            TagSwitchDB.Ins.SwitchChanged += SwitchChangedCallback;
            FloatTextBox.Ins.TextChangedCallback += TextChanged;

        }
        private void TextChanged(Canvas Parent, GUTag tag, string NewString)
        {
            if(canvas == Parent && tag!=null &&NewString!=null)
            {
                tag.ChangeTitle(NewString);
                SetCurrentTag(tag);
            }
        }
        private void SwitchChangedCallback()
        {
            RedrawGraph();
        }
        //currentTag和root的区别： 
        //root是当前有向图显示的中心节点，
        //currentTag是当前选中的节点
        //
        private GUTag rootTag = null;
        private GUTag currentTag = null;
        private Size oriSize
        {
            get
            {
                return scrollViewer.RenderSize;
            }
        }
        private LayoutMode myMode;
        public void ChangeRoot(GUTag rootTag,GUTag selectTag)
        {
            if (rootTag!=null)
            {
                if(CanvasType == LayoutCanvas.MAIN_CANVAS)
                {
                    DynamicCfg.Ins.ChangeMainCanvasRoot( rootTag);
                }
                else
                {
                    DynamicCfg.Ins.ChangeSubCanvasRoot( rootTag);
                }
            }
            else
            {
                if (CanvasType == LayoutCanvas.MAIN_CANVAS)
                {
                    rootTag  = TagDB.GetTag(Guid.Parse(DynamicCfg.Ins.MainCanvasRoot));
                    
                }
                else
                {
                    rootTag = TagDB.GetTag(Guid.Parse(DynamicCfg.Ins.SubCanvasRoot));
                }
            }
            this.rootTag = rootTag;
            currentTag = selectTag == null ? rootTag : selectTag;
            RedrawGraph();
        }
        public void RedrawGraph()
        {
            LayoutMode bak = GLayoutMode.mode;
            GLayoutMode.mode = myMode;
            RedrawGraph_();
            GLayoutMode.mode = bak;
        }

        private List<TagBox> allTagBox = new List<TagBox>();
        private List<System.Windows.Shapes.Path> allLine = new List<System.Windows.Shapes.Path>();
        private TreeLayoutEnv env = new TreeLayoutEnv();
        private void RedrawGraph_()
        {
            if (TagDB != null && rootTag != null && oriSize.Height!=0 && oriSize.Width!=0 && !oriSize.IsEmpty)
            {
                Logger.I("ShowGraph at " + rootTag);
               
                if (allTagBox != null)
                {
                    env.Return(allTagBox);
                }
                if(allLine!=null)
                {
                    env.Return(allLine);
                }
                //canvasRecentTags.Children.Clear();

                //计算有向图布局
                ITagLayout tagLayout = TagLayoutFactory.CreateLayout();
                tagLayout.Layout(TagDB, rootTag,oriSize,env);

                //将有向图中的元素显示在界面上
                IEnumerable<UIElement> lines = tagLayout.Lines;
                IEnumerable<UIElement> allTxt = tagLayout.TagArea;
                allTagBox = allTxt as List<TagBox>;
                allLine = lines as List<System.Windows.Shapes.Path>;
                canvas.Width = tagLayout.Size.Width;
                layoutHeight = tagLayout.Size.Height;
                SetHeight();

                foreach (UIElement l in lines)
                {
                    if (!l.IsVisible)
                    {
                        canvas.Children.Add(l);
                    }
                }
                foreach (TagBox t in allTxt)
                {
                    //设置每一个tag的上下文菜单和事件响应钩子
                    if (t.ContextMenu == null)
                    {
                        t.ContextMenu = TagAreaMenu;
                        t.MouseLeftButtonDown += Tag_MouseLeftButtonDown;
                        t.MouseDoubleClick += Tag_MouseDoubleClick;
                        canvas.Children.Add(t);
                    }
                    
                }
                //UpdateRecentTags(root);
                //SetCurrentTag(root);
                SetCurrentTag();
            }
        }
        
       
        private double canvasMinHeight = 0;
        private double layoutHeight = 0;
        private double CanvasMinHeight
        {
            set
            {
                canvasMinHeight = value;
                SetHeight();
            }
        }

        private void SetHeight()
        {
            if (double.IsNaN(canvasMinHeight) || double.IsInfinity(canvasMinHeight))
            {
                canvasMinHeight = 0;
            }
            canvas.Height = Math.Max(layoutHeight, canvasMinHeight);
        }
        
        private GUTag NavigateTagBox(Key direction)
        {
            //先找到当前选择的节点
            TagBox curB = null;
            foreach (UIElement b in allTagBox)//Bug:不能使用Children，Children中有些事不可见的（为了性能优化，没有将所有无用的TagBox从Canvas.Children中删除
            {
                TagBox tmp = b as TagBox;
                if (tmp != null && tmp.GUTag == currentTag)
                {
                    curB = tmp;
                    break;
                }
            }
            double xyRadio = 0.1;
            if (direction == Key.Left || direction == Key.Right) xyRadio = 1 / xyRadio;

            double mimDistance = double.MaxValue;
            double mimDistanceBetter = double.MaxValue;
            GUTag result = null;
            GUTag resultBetter = null;
            //在移动当前节点
            if (curB != null)
            {
                foreach (UIElement tmp in allTagBox)
                {
                    TagBox b = tmp as TagBox;

                    //过滤掉不满足条件的所有元素
                    if (b == null) continue;
                    if (b == curB) continue;
                    switch (direction)
                    {
                        case Key.Up: if (curB.Margin.Top <= b.Margin.Top + b.Height1) continue; break;
                        case Key.Down: if (curB.Margin.Top + curB.Height1 >= b.Margin.Top) continue; break;
                        case Key.Left: if (curB.Margin.Left <= b.Margin.Left + b.Width1) continue; break;
                        case Key.Right: if (curB.Margin.Left + curB.Width1 >= b.Margin.Left) continue; break;
                        default: break;
                    }

                    double x1 = curB.Margin.Left + curB.Width1 / 2;
                    double y1 = curB.Margin.Top + curB.Height1 / 2;
                    double x2 = b.Margin.Left + b.Width1 / 2;
                    double y2 = b.Margin.Top + b.Height1 / 2;


                    double dis = double.MaxValue;
                    double delta = 0.2;
                    //左右移动时，如果两个矩形在水平方向上有交集，优先采用
                    //采用的标准是看水平方向的距离
                    if (direction == Key.Left || direction == Key.Right)
                    {
                        double top1 = b.Margin.Top - delta;
                        double bottom1 = top1 + b.Height1 + delta;

                        double top2 = curB.Margin.Top - delta;
                        double bottom2 = top2 + curB.Height1 + delta;

                        if (!(top1 > bottom2 || bottom1 < top2))
                        {

                            dis = (x1 - x2) * (x1 - x2);
                            dis = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
                        }
                    }
                    //上下移动时，如果两个矩形在垂直方向上有交集，优先采用
                    //采用的标准是看垂直方向的距离
                    else if (direction == Key.Up || direction == Key.Down)
                    {
                        double left1 = b.Margin.Left - delta;
                        double right1 = left1 + b.Width1 + delta;

                        double left2 = curB.Margin.Left - delta;
                        double right2 = left2 + curB.Width1 + delta;



                        if (!(left1 > right2 || right1 < left2))
                        {
                            dis = (y1 - y2) * (y1 - y2);
                            dis = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
                        }

                    }
                    //记录下优选中的佼佼者
                    if (dis < mimDistanceBetter)
                    {
                        mimDistanceBetter = dis;
                        resultBetter = b.GUTag;
                    }
                    //如果都没有交集，就直接看距离
                    dis = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
                    if (dis < mimDistance)
                    {
                        mimDistance = dis;
                        result = b.GUTag;
                    }


                }
            }
            if (resultBetter != null)
            {
                SetCurrentTag(resultBetter);
            }
            else if (result != null)
            {
                SetCurrentTag(result);
            }
            //如果当前节点时根节点，则向上退一级
            else if ((direction == Key.Left || direction == Key.Up) && currentTag == rootTag)
            {
                UpTag();
            }
            return result;
        }

        public void UpTag()
        {
            List<GUTag> parents = TagDB.QueryTagParent(rootTag);
            if (parents.Count > 0)
            {
                ChangeRoot(parents[0], rootTag);
                SetCurrentTag(parents[0]);
            }
        }

        private void DbgShowTagBox()
        {
            Logger.D("\r\n\r\n  BeginDbgShowTagBox {0}=====================", CanvasType);

            foreach(UIElement u in canvas.Children)
            {
                TagBox t = u as TagBox;
                if(t!=null)
                {
                    Logger.D("DbgShowTagBox:{0} - POS:{1}-{2}，Visibility={3}", t.GUTag, t.Margin.Left, t.Margin.Top,t.Visibility);
                }
            }
            Logger.D("EndDbgShowTagBox {0}=====================\r\n\r\n  ", CanvasType);
        }
        private TagBox FindTagBox(GUTag tag)
        {
            foreach (UIElement u in allTagBox)//此处不能在Canvas.Children中查找，因为为了性能做了特殊优化，一些不可见的tagbox仍然存在于Canvas.Children中。
            {
                TagBox t = u as TagBox;
                if (t != null)
                {
                    if (t.GUTag == tag)
                    {
                        Logger.D("FindTagBox:{0} - POS:{1}-{2}", tag, t.Margin.Left, t.Margin.Top);
                        return t;
                    }
                }
            }
            return null;
        }
        private TagBox FindTagBoxByTxt(string tag)
        {
            foreach (UIElement u in allTagBox)//此处不能在Canvas.Children中查找，因为为了性能做了特殊优化，一些不可见的tagbox仍然存在于Canvas.Children中。
            {
                TagBox t = u as TagBox;
                if (t != null)
                {
                    if (t.Text == tag)
                    {
                        Logger.D("FindTagBox:{0} - POS:{1}-{2}", tag, t.Margin.Left, t.Margin.Top);
                        return t;
                    }
                }
            }
            return null;
        }
        //在当前图中的所有tag查找，看看当前是否已经显示，如果已经显示，直接切换节点
        //如果没有显示，返回null
        public TagBox ChangeSelectedByTxt(string txt)
        {
            DbgShowTagBox();
            TagBox target = FindTagBoxByTxt(txt);
            if (target != null)
            {
                SetCurrentTag(target.GUTag);
            }
            return target;
        }
        public TagBox ChangeSelectd(GUTag tag)
        {
            DbgShowTagBox();
            TagBox target = FindTagBox(tag);
            if(target!=null)
            {
                SetCurrentTag(tag);
            }
            return target;
            
        }
        //public void ShowGraph(ITagDB tagDB, string root)
        //{
        //    Logger.I("ShowGraph at " + root);
        //    this.tagDB = tagDB;
        //    this.rootTag = root;

        //    canvas.Children.Clear();
        //    canvasRecentTags.Children.Clear();

        //    //计算有向图布局
        //    ITagLayout tagLayout = TagLayoutFactory.CreateLayout();
        //    tagLayout.Layout(tagDB, root);

        //    //将有向图中的元素显示在界面上
        //    IEnumerable<UIElement> lines = tagLayout.Lines;
        //    IEnumerable<UIElement> allTxt = tagLayout.TagArea;

        //    canvas.Width = tagLayout.Size.Width;
        //    layoutHeight = tagLayout.Size.Height;
        //    SetHeight();

        //    foreach (UIElement l in lines)
        //    {
        //        canvas.Children.Add(l);
        //    }
        //    foreach (TagBox t in allTxt)
        //    {
        //        //设置每一个tag的上下文菜单和事件响应钩子
        //        t.ContextMenu = TagAreaMenu;
        //        t.MouseLeftButtonDown += Tag_MouseLeftButtonDown;
        //        t.MouseDoubleClick += Tag_MouseDoubleClick;
        //        canvas.Children.Add(t);
        //    }
        //    UpdateRecentTags(root);
        //    //SetCurrentTag(root);
        //    SetCurrentTag();
        //}
        //双击tag，以该tag为根显示有向图
        private void Tag_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TagBox b = sender as TagBox;
            if (b != null)
            {
                ChangeRoot(b.GUTag, b.GUTag);
            }
        }
        //单击tag，将该tag改为选定状态  TODO，运行多个tag选中
        private void Tag_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TagBox)
            {
                DateTime t1 = DateTime.Now;
                SetCurrentTag((sender as TagBox).GUTag);
                DateTime t2 = DateTime.Now;
                //MessageBox.Show("ts=" + (t2 - t1).TotalSeconds);
            }
        }
        ContextMenu TagAreaMenu
        {
            get
            {
                return Resources["tagAreaMenu"] as ContextMenu;
            }
        }
        private void miOpenTagDir_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            FileShell.OpenExplorerByTag(currentTag.Title);

        }
        public void ClearSelected()
        {
            UpdateSelectedStatus(null, TagBox.Status.None);
        }
        private void UpdateSelectedStatus(GUTag tag, TagBox.Status stat)
        {
            foreach (UIElement u in allTagBox)
            {
                TagBox tb = u as TagBox;

                if (tb != null)
                {
                    tb.Stat = TagBox.Status.None;
                    if (tb.GUTag == tag)
                    {
                        tb.Stat = stat;
                    }
                }
            }

        }
        


        private void SetCurrentTag()
        {
            SetCurrentTag(currentTag);
        }
        
        public CurrentTagChanged SelectedTagChanged = null;
        private void SetCurrentTag(GUTag tag)
        {
            UpdateSelectedStatus(tag, TagBox.Status.Selected); //这一句必须放在下面检查并return之前，
                                                               //即无论currentTag是否变化，都需要更新一下border，否则会有bug；
                                                               //bug现象：在curtag没有变化的时候，重新绘制整个graph，
                                                               //会出现所有的tag都不显示边框（包括curtag），因为直接返回了。

            //if (currentTag == tag) return;  //原来在tag没有变化时不通知变更，导致有些问题，后面将该语句取消了。
            GUTag oldTag = currentTag;
            currentTag = tag;


            ShowCurrentTagInf();
            SelectedTagChanged?.Invoke(tag);
            TagVirtualDir.Ins.KeepVDir(tag.Title);

        }
        
        

       

        

        

        private string GetTagInf(GUTag tag, ITagDB db)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("当前选中标签：" + tag);

            List<GUTag> parents = db.QueryTagParent(tag);
            if (parents.Count > 0)
            {
                sb.Append(" Parent::= ");
                foreach (GUTag s in parents) sb.Append(" " + s.Title);
            }


            List<GUTag> children = TagDB.QueryTagChildren(tag);
            if (children.Count > 0)
            {
                sb.Append(" Children::= ");
                foreach (GUTag s in children) sb.Append(" " + s.Title);
            }
            return sb.ToString().Trim();
        }
        private void ShowCurrentTagInf()
        {
            TipsCenter.Ins.TagInf = GetTagInf(currentTag, TagDB);
            //TipsCenter.Ins.Tips = GetTagInf(currentTag,tagDB);
            //CurrentTagInf.Text = GetTagInf(currentTag, tagDB);
        }

        public void miPasteFile_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();


            string[] token = Clipboard.GetText().Split(new char[] { ClipboardConst.CommandSplitToken }, StringSplitOptions.RemoveEmptyEntries);

            if (token.Length == 2)
            {
                string arg = token[1];

                string[] args = arg.Split(new char[] { ClipboardConst.ArgsSplitToken }, StringSplitOptions.RemoveEmptyEntries);
                switch (token[0])
                {
                    case ClipboardConst.KUMMERWU_TAG_COPY:
                        {
                            GUTag argTag = TagDB.GetTag(Guid.Parse(arg));
                            if (argTag != null)
                            {
                                TagDB.AddTag(currentTag, argTag);
                                RedrawGraph();
                            }
                        }
                        break;
                    case ClipboardConst.KUMMERWU_TAG_CUT:
                        {
                            GUTag argTag = TagDB.GetTag(Guid.Parse(arg));
                            if (argTag != null)
                            {
                                TagDB.ResetParent(currentTag, argTag);
                                RedrawGraph();
                            }
                        }
                        break;
                    case ClipboardConst.KUMMERWU_URI_CUT:
                        MoveUris(args);
                        break;
                    case ClipboardConst.KUMMERWU_URI_COPY:
                        UriDB.AddUri(args, new List<string>() { currentTag.Title });
                        //foreach (string uri in args)
                        //{
                        //    UriDB.AddUri(uri, new List<string>() { currentTag });
                        //}
                        break;
                    default: PasteFiles(); break;
                }
            }
            else
            {
                PasteFiles();
            }

        }

        //TODO:这儿有bug，moveuri时，src可能是文件，也可能是http链接。
        //需要有一种机制，将文件和链接统一对待处理。

        private void MoveUris(string[] args)
        {
            string[] src = args;
            string[] dst = PathHelper.MapFilesToTagDir(src, currentTag.Title);
            FileShell.SHMoveFiles(src, dst);
            UriDB.DelUri(src, false);  //TODO bug2:对于http链接，删除后，标题就没有了。
            //foreach (string uri in src)
            //{
            //    UriDB.DelUri(uri, false); 
            //}
            UriDB.AddUri(dst, new List<string>() { currentTag.Title });
            //foreach(string uri in dst)
            //{
            //    UriDB.AddUri(uri, new List<string>() { currentTag });
            //}
        }
        private void PasteFiles() { PasteFiles(true); }
        private void PasteFiles(bool NeedCopy)
        {
            Cursor bak = this.Cursor;
            try
            {
                this.Cursor = Cursors.Wait;
                UpdateCurrentTagByContextMenu();
                AddUri(FileShell.GetFileListFromClipboard(), NeedCopy);
            }
            catch (Exception e)
            {
                Logger.E(e);
            }
            finally
            {
                this.Cursor = bak;
            }
        }

        private void UpdateCurrentTagByContextMenu()
        {
            TagBox t = TagAreaMenu.PlacementTarget as TagBox;
            if (t != null && t.GUTag!=null)
            {
                SetCurrentTag(t.GUTag);
            }
        }
        private void AddUri(List<string> files) { AddUri(files, true); }
        private void AddUri(List<string> files, bool NeedCopy)
        {
            if (UriDB == null || currentTag == null ||currentTag.Title==null || currentTag.Title.Length == 0) return;

            List<string> tags = new List<string>() { currentTag.Title };
            IEnumerable<string> dst = null;
            if (NeedCopy)
            {
                dst = CopyToHouse(files.ToArray(), currentTag.Title);
            }
            else
            {
                dst = files;
            }

            //TODO,这个动作时间很长，整个过程中界面没有响应。
            UriDB.AddUri(dst, tags);
            foreach (string uri in dst)
            {
                if (PathHelper.IsValidHttps(uri))
                {
                    string title = WebHelper.GetWebTitle(uri);
                    if (title != null)
                    {
                        UriDB.UpdateUri(uri, title);
                    }
                }
            }
            //foreach(string f in dst)
            //{
            //    string dstFile = f;
            //    if (dstFile != null)
            //    {
            //        UriDB.AddUri(dstFile, tags);
            //    }
            //}
        }

        private string[] CopyToHouse(string[] list, string tag)
        {
            List<string> ret = new List<string>();
            List<string> scrList = new List<string>();
            List<string> dstList = new List<string>();
            foreach (string f in list)
            {
                if (PathHelper.IsValidHttps(f))
                {
                    ret.Add(f);
                }
                else if (!PathHelper.IsValidFS(f))
                {
                    Logger.E("Copy To House: File not Exist " + f);
                    ret.Add(f);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(PathHelper.IsValidFS(f));
                    FileInfo fi = new FileInfo(f);
                    string dstDir = CfgPath.GetDirByTag(tag);
                    string dstFile = System.IO.Path.Combine(dstDir, fi.Name);
                    if (dstFile == f)
                    {
                        ret.Add(f);
                    }
                    else
                    {
                        ret.Add(dstFile);
                        if (!System.IO.File.Exists(dstFile) && !System.IO.Directory.Exists(dstFile))
                        {
                            scrList.Add(f);
                            dstList.Add(dstFile);
                        }
                    }
                }
            }

            FileShell.SHCopyFile(scrList.ToArray(), dstList.ToArray());
            return ret.ToArray();

            /*
            if(dstFile==f)
            {
                return f;
            }
            if (!System.IO.File.Exists(dstFile) && !System.IO.Directory.Exists(dstFile))//TODO 已经存在的需要提示覆盖、放弃、重命名
            {
                if(FileShell.CopyFile(f, dstFile))
                {
                    return dstFile;
                }
            }
            return null;*/
        }
        private void miCopyTagName_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            ClipBoardSafe.SetText(currentTag.Title);

        }

        private void miCopyTagFullPath_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            ClipBoardSafe.SetText(CfgPath.GetDirByTag(currentTag.Title));
        }

        private void tagAreaMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
        }

        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double h = e.NewSize.Height;
            

            canvasMinHeight = this.ActualHeight - 60;
            SetHeight();
            
            System.Diagnostics.Debug.WriteLine(e.NewSize.Height+" "+e.NewSize.Width + " "+rootTag + " "+currentTag);
            RedrawGraph();
        }

        private void miNewFile_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            string initDir = CfgPath.GetDirByTag(currentTag.Title);
            SaveFileDialog sf = new SaveFileDialog();
            sf.InitialDirectory = initDir;

            sf.Filter = TemplateHelper.GetTemplateFileFilter();//"One文件(*.one)|*.one|Mind文件(*.xmind)|*.xmind";
            if (sf.ShowDialog() == true)
            {
                if (File.Exists(sf.FileName))
                {
                    MessageBox.Show("该文件已经存在" + sf.FileName, "文件名冲突", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    FileInfo fi = new FileInfo(sf.FileName);
                    string tmplateFile = TemplateHelper.GetTemplateByExtension(fi.Extension);
                    if (tmplateFile != null && File.Exists(tmplateFile))
                    {
                        File.Copy(tmplateFile, sf.FileName);
                        AddUri(new List<string>() { sf.FileName });
                        FileShell.StartFile(sf.FileName);
                    }
                    else
                    {
                        File.Create(sf.FileName).Close();
                    }
                }
            }
        }

        public void miCopyTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            ClipBoardSafe.SetText(ClipboardConst.KUMMERWU_TAG_COPY + ClipboardConst.CommandSplitToken + currentTag.Id);
            UpdateSelectedStatus(currentTag, TagBox.Status.Copy);
        }

        public void miCutTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            ClipBoardSafe.SetText(ClipboardConst.KUMMERWU_TAG_CUT + ClipboardConst.CommandSplitToken + currentTag.Id);
            UpdateSelectedStatus(currentTag, TagBox.Status.Cut);
        }

        private void miDeleteTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            if (TagDB.QueryTagChildren(currentTag).Count == 0)
            {
                GUTag oldCurrentTag = currentTag;
                List<GUTag> parents = TagDB.QueryTagParent(oldCurrentTag);

                //找到一个合适的父节点
                GUTag newCurrentTag = NavigateTagBox(Key.Left);
                if(parents.Count==1)
                {
                    newCurrentTag = parents[0];
                }
                if (newCurrentTag==null)
                {
                    newCurrentTag = TagDB.GetTag(StaticCfg.Ins.DefaultTagID);
                }

                TagDB.RemoveTag(oldCurrentTag);
                
                //如果新选出来的当前节点在视图中，直接选中该tag
                foreach(UIElement u in allTagBox)
                {
                    TagBox t = u as TagBox;
                    if(t!=null && t.GUTag == newCurrentTag)
                    {
                        SetCurrentTag(newCurrentTag);
                        

                    }
                }
                //当新选出来的tag不再视图中时，才需要切换视图的根节点
                if (currentTag != newCurrentTag)
                {
                    ChangeRoot(newCurrentTag, newCurrentTag);
                }
                else
                {
                    RedrawGraph();
                }
            }
            else
            {
                MessageBox.Show(string.Format("[{0}]下还有其他子节点，如果确实需要删除该标签，请先删除所有子节点", currentTag), "提示：", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void miLinkInFile_Click(object sender, RoutedEventArgs e)
        {
            PasteFiles(false);
        }
        public string[] ParseTags(string input)
        {
            return input.Split(new char[] { ' ', ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
        }
        //private string GetNewTagTitle()
        //{
        //    string tag = StaticCfg.Ins.DefaultNewTag;
        //    int i = 0;
        //    while(TagDB.QueryTagAlias(tag).Count>0)
        //    {
        //        tag = StaticCfg.Ins.DefaultNewTag + "-" + (++i);
        //    }
        //    return tag;
        //}
        private void miNewTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            if (currentTag == null) return;
            //TODO 如果有多个创建子标签如何正确处理？
            GUTag newTag = new GUTag(StaticCfg.Ins.DefaultNewTag);
            TagDB.AddTag(currentTag, newTag);
            RedrawGraph();

            //BUG20171031: 子标签如果没有在图中显示出来（比如mainCanvas中因为深度的限制，并没有将其显示出来，下面b可能为null
            TagBox b = ChangeSelectd(newTag);
            FloatTextBox.Ins.ShowEdit(canvas, b);

            
        }

        private void miPasteTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();


            string[] token = Clipboard.GetText().Split(new char[] { ClipboardConst.CommandSplitToken }, StringSplitOptions.RemoveEmptyEntries);

            if (token.Length == 2)
            {
                string arg = token[1];

                string[] args = arg.Split(new char[] { ClipboardConst.ArgsSplitToken }, StringSplitOptions.RemoveEmptyEntries);
                switch (token[0])
                {
                    case ClipboardConst.KUMMERWU_TAG_COPY:
                        {
                            GUTag argTag = TagDB.GetTag(Guid.Parse(arg));
                            if (argTag != null)
                            {
                                TagDB.AddTag(currentTag, argTag);
                                RedrawGraph();
                            }
                        }
                        break;
                    case ClipboardConst.KUMMERWU_TAG_CUT:
                        {
                            GUTag argTag = TagDB.GetTag(Guid.Parse(arg));
                            if (argTag != null)
                            {
                                TagDB.ResetParent(currentTag, argTag);
                                RedrawGraph();
                            }
                        }
                        break;
                }
            }

        }

        private void miCopyTagFullPathEx_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            string dir = CfgPath.GetDirByTag(currentTag.Title);
            dir = System.IO.Path.Combine(dir, DateTime.Now.ToString("yyyyMMdd") + "-");
            ClipBoardSafe.SetText(dir);
        }

        private void OpenTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miOpenTagDir_Click(sender, e);
        }

        private void CopyTagFullPath_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miCopyTagFullPath_Click(sender, e);
        }

        private void CopyTagFullPathEx_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miCopyTagFullPathEx_Click(sender, e);
        }

        private void CopyTagName_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miCopyTagName_Click(sender, e);
        }

        private void NewTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miNewTag_Click(sender, e);
        }

        private void CopyTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miCopyTag_Click(sender, e);
        }

        private void CutTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miCutTag_Click(sender, e);
        }

        private void PasteTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miPasteTag_Click(sender, e);
        }

        private void DeleteTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miDeleteTag_Click(sender, e);
        }

        private void NewFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miNewFile_Click(sender, e);
        }

        private void PasteFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miPasteFile_Click(sender, e);
        }

        private void LinkFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miLinkInFile_Click(sender, e);
        }
        private void EditFile(string dotPostfix)
        {
            UpdateCurrentTagByContextMenu();
            if (currentTag == null ) return;

            string defaultFile = CfgPath.GetTemplateFileByTag(currentTag.Title, dotPostfix);
            if (defaultFile == null) return;

            FileShell.StartFile(defaultFile);
        }
        private void EditFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditFile(".one");
        }

        private void NavigateTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NavigateTagBox(Key.Up);
        }
        private void precanvas_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("precanvas_KeyDown");
            canvas_KeyDown(sender, e);
        }
        private void canvas_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("canvas_KeyDown {0}-{1}", e.Key,e.ImeProcessedKey);
            if (!FloatTextBox.Ins.IsVisible)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
                {
                    NavigateTagBox(e.Key);
                    e.Handled = true;
                }
                else if(e.Key == Key.Enter)
                {
                    NewBrotherTag();
                    e.Handled = true;
                }
                else if(e.Key == Key.F2)
                {
                    TagBox t = FindTagBox(currentTag);
                    if (t != null)
                    {
                        FloatTextBox.Ins.ShowEdit(canvas, t);
                        e.Handled = true;
                    }
                }
            }


        }
        private void ChangeTagPos(int direct)
        {
            UpdateCurrentTagByContextMenu();
            if (currentTag == null ) return;
            TagDB.ChangePos(currentTag, direct);
            RedrawGraph();
        }
        private void UpTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ChangeTagPos(-1);
        }

        

        private void DownTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ChangeTagPos(1);
        }

        //private void canvas_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
        //    {
        //        NavigateTagBox(e.Key);
        //    }
        //}

        private void NavigateTagUp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NavigateTagBox(Key.Up);
        }

        private void NavigateTagDown_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NavigateTagBox(Key.Down);
        }

        private void NavigateTagLeft_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NavigateTagBox(Key.Left);
        }

        private void NavigateTagRight_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NavigateTagBox(Key.Right);
        }

        private void EditRTFFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditFile(".rtf");
        }

        private void scrollViewer_GotFocus(object sender, RoutedEventArgs e)
        {
            SetCurrentTag();
        }

        private void scrollViewer_LostFocus(object sender, RoutedEventArgs e)
        {
            ClearSelected();
        }

        private void ModifyTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TagBox t = TagAreaMenu.PlacementTarget as TagBox;
            if (t != null && t.GUTag!=null)
            {
                SetCurrentTag(t.GUTag);
                FloatTextBox.Ins.ShowEdit(canvas, t);
            }

        }

        private void NewBrotherTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NewBrotherTag();
        }

        private void NewBrotherTag()
        {
            UpdateCurrentTagByContextMenu();
            if (currentTag == null ) return;
            List<GUTag> ps = TagDB.QueryTagParent(currentTag);
            if (ps.Count == 0) return;

            GUTag parent = ps[0];
            GUTag newTag = new GUTag(StaticCfg.Ins.DefaultNewTag);
            TagDB.AddTag(parent, newTag);//TODO 如果有多个创建子标签如何正确处理？
            RedrawGraph();
            TagBox b = ChangeSelectd(newTag);
            FloatTextBox.Ins.ShowEdit(canvas, b);
        }
    }
    
    
}
