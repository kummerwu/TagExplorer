﻿using AnyTagNet;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagExplorer.AutoComplete;
using TagExplorer.TagLayout;
using TagExplorer.TagLayout.CommonLayout;
using TagExplorer.TagLayout.LayoutCommon;
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
        #region 顶层视图与底层视图处理差异的地方（主要是两个视图的配置存储在不同的字段中，这一块写的比较丑，后面有空再优化）
        private LayoutCanvas MyCanvasType;
        //根据配置，决定是否显示根节点路径
        public bool NeedShowRootPath()
        {

            return (MyCanvasType == LayoutCanvas.MAIN_CANVAS && StaticCfg.Ins.Opt.ShowMainCanvasRootPath) ||
               (MyCanvasType == LayoutCanvas.SUB_CANVAS && StaticCfg.Ins.Opt.ShowSubCanvasRootPath);
        }
        private void SaveRootTag(GUTag rootTag)
        {
            if (MyCanvasType == LayoutCanvas.MAIN_CANVAS)
            {
                DynamicCfg.Ins.MainCanvasRoot = (rootTag.ToString());
            }
            else
            {
                DynamicCfg.Ins.SubCanvasRoot = (rootTag.ToString());
            }
        }
        private GUTag LoadRootTag()
        {
            GUTag rootTag;
            if (MyCanvasType == LayoutCanvas.MAIN_CANVAS)
            {
                rootTag = TagDB.GetTag(Guid.Parse(DynamicCfg.Ins.MainCanvasRoot));

            }
            else
            {
                rootTag = TagDB.GetTag(Guid.Parse(DynamicCfg.Ins.SubCanvasRoot));
            }

            return rootTag;
        }
        //显示的模式
        private LayoutMode MyLayoutMode
        {
            get
            {
                switch (MyCanvasType)
                {
                    case LayoutCanvas.MAIN_CANVAS:
                        return DynamicCfg.Ins.MainCanvasLayoutMode;
                    case LayoutCanvas.SUB_CANVAS:
                        return DynamicCfg.Ins.SubCanvasLayoutMode;
                    default:
                        return LayoutMode.TREE_NO_COMPACT;
                }
            }
        }
        private void SaveLayoutMode(LayoutMode m)
        {
            switch (MyCanvasType)
            {
                case LayoutCanvas.MAIN_CANVAS:
                    DynamicCfg.Ins.MainCanvasLayoutMode = (m);
                    break;
                case LayoutCanvas.SUB_CANVAS:
                    DynamicCfg.Ins.SubCanvasLayoutMode = (m);
                    break;
                default:
                    break;
            }
        }
        #endregion


        #region 私有成员与初始化相关
        private ITagDB TagDB = null;
        private IUriDB UriDB = null;

        
        //初始化TagDB，该函数必须在空间初始化时就指定
        public void Initial(ITagDB db, IUriDB uridb, LayoutCanvas canvasType)
        {

            TagDB = db;
            UriDB = uridb;
            MyCanvasType = canvasType;
            if(!NeedShowRootPath())
            {
                //connectCanvas.Visibility = Visibility.Collapsed;
                connect.Height = new GridLength(0);
            }
            //更新上下文菜单的Check选项（选中哪个模式，就在该模式上打一个勾）
            UpdateMenuItemCheckStatus();
        }


        public TagCanvasCtrl()
        {
            InitializeComponent();
            TagSwitchDB.Ins.SwitchChanged += SwitchChangedCallback;//该功能目前已经没有再使用了。
            FloatTextBox.Ins.TextChangedCallback += TagEdit_TitleChanged;   //修改tag title完成后的回调。

        }
        #endregion

        #region 当前根节点和选中节点的管理
        //显示的根节点和当前（选中）节点
        //currentTag和root的区别： 
        //root是当前有向图显示的中心节点，
        //currentTag是当前选中的节点
        private GUTag RootTag = null;
        private GUTag SelectedTag = null;
        
        //当根节点或选中节点变化时，需要保存下来，以便程序重启后能恢复重启前的状态
        public void ChangeRoot(GUTag aRoot, GUTag aSelect)
        {
            if (aRoot != null)
            {
                SaveRootTag(aRoot);
            }
            else
            {
                aRoot = LoadRootTag();
            }
            RootTag = aRoot;
            SetCurrentTag(aSelect == null ? aRoot : aSelect);
            RedrawGraph();
            ShowRootPath();
        }

        


        private void ShowRootPath()
        {
            //如果配置不需要显示，则直接返回
            if (!NeedShowRootPath()) return;
            if (RootTag == null) return;

            connectCanvas.Children.Clear();

            //需要显示从全局根到当前视图根节点之间的路径
            //查找出所有从当前视图根节点到全局根节点之间的中间节点
            List<GUTag> connect = new List<GUTag>();
            GUTag from = RootTag;
            GUTag tmp = from;
            connect.Add(from);
            while(connect.Count<20 && tmp!=null)
            {
                List<GUTag> ps = TagDB.QueryTagParent(tmp);
                if (ps.Count > 0)
                {
                    tmp = ps[0];
                    connect.Add(tmp);
                }
                else break; 
            }
            connect.Reverse();

            //显示所有中间节点
            double X = 0;
            foreach (GUTag u in connect)
            {
                GTagBox gt = new GTagBox(5, u, X, 0, 1);
                TagBox tx = UIElementFactory.CreateTagBox(gt, null);
                tx.HideCircle();
                
                X += gt.OutterBox.Width;
                if (tx.ContextMenu == null)
                {
                    tx.ContextMenu = TagAreaMenu;
                    tx.MouseLeftButtonDown += Tag_MouseLeftButtonDown;
                    tx.MouseDoubleClick += Tag_MouseDoubleClick;
                    
                }
                connectCanvas.Children.Add(tx);
            }

        }
        #endregion

        #region 显示tag视图
        //所有显示的元素（TagBox-标签和Path-标签间的连线）
        private List<TagBox> allTagBox = new List<TagBox>();
        private List<System.Windows.Shapes.Path> allLine = new List<System.Windows.Shapes.Path>();
        private TreeLayoutEnv env = new TreeLayoutEnv();


        //显示的tag视图大小
        private Size oriSize
        {
            get
            {
                Size s1 = scrollViewer.RenderSize;
                Size s2 = this.RenderSize;
                return s2;
            }
        }
        //大小变化后需要重绘视图
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawGraph();
        }
        //根据当前大小，调整canvas的大小。
        private void AdjustCanvasSize(double w, double h)
        {
            Size s = oriSize;
            double cw = Math.Max(w, s.Width - 30);
            double ch = Math.Max(h, s.Height - 30);
            canvas.Width = cw;
            canvas.Height = ch;
        }

        
        
        //public void RedrawGraph()
        //{
        //    //LayoutMode bak = GLayoutMode.mode;
        //    //GLayoutMode.mode = MyLayoutMode;
        //    RedrawGraph_();
        //    //GLayoutMode.mode = bak;
        //}
        public void RedrawGraph()
        {
            if (TagDB != null && RootTag != null && oriSize.Height != 0 && oriSize.Width != 0 && !oriSize.IsEmpty)
            {
                Logger.I("ShowGraph at " + RootTag);

                if (allTagBox != null)
                {
                    env.Return(allTagBox);
                }
                if (allLine != null)
                {
                    env.Return(allLine);
                }
                //canvasRecentTags.Children.Clear();

                //计算有向图布局
                ITagLayout tagLayout = TagLayoutFactory.CreateLayout(MyLayoutMode);
                tagLayout.Layout(TagDB, RootTag, oriSize, env);

                //将有向图中的元素显示在界面上
                IEnumerable<UIElement> lines = tagLayout.Lines;
                IEnumerable<UIElement> allTxt = tagLayout.TagArea;
                allTagBox = allTxt as List<TagBox>;
                allLine = lines as List<System.Windows.Shapes.Path>;
                AdjustCanvasSize(tagLayout.Size.Width, tagLayout.Size.Height);
                //canvas.Width = tagLayout.Size.Width;
                //canvas.Height = tagLayout.Size.Height;
                //SetHeight();

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
        #endregion
        #region tag的展开与不展开切换
        private void SwitchChangedCallback()
        {
            RedrawGraph();
        }
        #endregion

        #region Tag的增删改
        private void TagEdit_TitleChanged(Canvas Parent, GUTag tag, string NewString)
        {
            
            if(canvas == Parent && tag!=null &&NewString!=null)
            {
                tag = TagDB.ChangeTitle(tag,NewString);
                if (tag != null)
                {
                    RedrawGraph();
                    SetCurrentTag(tag, true);
                }
            }
        }
        #endregion
        
        
        private GUTag NavigateTagBox(Key direction)
        {
            //先找到当前选择的节点
            TagBox curB = null;
            foreach (UIElement b in allTagBox)//Bug:不能使用Children，Children中有些事不可见的（为了性能优化，没有将所有无用的TagBox从Canvas.Children中删除
            {
                TagBox tmp = b as TagBox;
                if (tmp != null && tmp.GUTag == SelectedTag)
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
                SetCurrentTag(resultBetter,true);
            }
            else if (result != null)
            {
                SetCurrentTag(result,true);
            }
            //如果当前节点时根节点，则向上退一级
            else if ((direction == Key.Left || direction == Key.Up) && SelectedTag == RootTag)
            {
                UpTag();
            }
            return result;
        }

        public void UpTag()
        {
            List<GUTag> parents = TagDB.QueryTagParent(RootTag);
            if (parents.Count > 0)
            {
                ChangeRoot(parents[0], RootTag);
                SetCurrentTag(parents[0],true);
            }
        }

        private void DbgShowTagBox()
        {
            Logger.D("\r\n\r\n  BeginDbgShowTagBox {0}=====================", MyCanvasType);

            foreach(UIElement u in canvas.Children)
            {
                TagBox t = u as TagBox;
                if(t!=null)
                {
                    Logger.D("DbgShowTagBox:{0} - POS:{1}-{2}，Visibility={3}", t.GUTag, t.Margin.Left, t.Margin.Top,t.Visibility);
                }
            }
            Logger.D("EndDbgShowTagBox {0}=====================\r\n\r\n  ", MyCanvasType);
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
        private bool Match(TagBox t,AutoCompleteTipsItem aItem)
        {
            GUTag tag = aItem.Data as GUTag;
            if(tag!=null)
            {
                return tag == t.GUTag;
            }
            else
            {
                return t.Text == aItem.Content;
            }
        }
        private TagBox FindTagBoxByTxt(AutoCompleteTipsItem tag)
        {
            foreach (UIElement u in allTagBox)//此处不能在Canvas.Children中查找，因为为了性能做了特殊优化，一些不可见的tagbox仍然存在于Canvas.Children中。
            {
                TagBox t = u as TagBox;
                if (t != null)
                {

                    if (Match(t,tag))
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
        public TagBox ChangeSelectedByTxt(AutoCompleteTipsItem txt)
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
                SetCurrentTag((sender as TagBox).GUTag,true);
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
            FileShell.OpenExplorerByTag(SelectedTag.Title);

        }
        public void ClearSelected()
        {
            
            foreach (UIElement u in allTagBox)
            {
                TagBox tb = u as TagBox;

                if (tb != null)
                {
                    tb.Stat = (tb.GUTag == SelectedTag)? TagBox.Status.SelectedLostFocus : TagBox.Status.None;
                }
            }
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


        public CurrentTagChanged SelectedTagChanged = null;
        private void SetCurrentTag()
        {
            SetCurrentTag(SelectedTag);
        }
        private void SetCurrentTag(GUTag tag,bool forceNotify = false)
        {
            UpdateSelectedStatus(tag, TagBox.Status.Selected); //这一句必须放在下面检查并return之前，
                                                               //即无论currentTag是否变化，都需要更新一下border，否则会有bug；
                                                               //bug现象：在curtag没有变化的时候，重新绘制整个graph，
                                                               //会出现所有的tag都不显示边框（包括curtag），因为直接返回了。

            if (SelectedTag == tag && SelectedTag?.Title == tag?.Title && !forceNotify) return;  //原来在tag没有变化时不通知变更，导致有些问题，后面将该语句取消了。
            GUTag oldTag = SelectedTag;
            SelectedTag = tag;


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
            TipsCenter.Ins.TagInf = GetTagInf(SelectedTag, TagDB);
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
                                TagDB.SetParent(SelectedTag, argTag);
                                RedrawGraph();
                            }
                        }
                        break;
                    case ClipboardConst.KUMMERWU_TAG_CUT:
                        {
                            GUTag argTag = TagDB.GetTag(Guid.Parse(arg));
                            if (argTag != null)
                            {
                                TagDB.ResetParent(SelectedTag, argTag);
                                RedrawGraph();
                            }
                        }
                        break;
                    case ClipboardConst.KUMMERWU_URI_CUT:
                        MoveUris(args);
                        break;
                    case ClipboardConst.KUMMERWU_URI_COPY:
                        UriDB.AddUris(args, new List<string>() { SelectedTag.Title });
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

        //《DEL》TODO:这儿有bug，moveuri时，src可能是文件，也可能是http链接。
        //需要有一种机制，将文件和链接统一对待处理。《DEL》
        //OK,处理干净：对于http和文件区分处理。
        private void MoveUris(string[] args)
        {
            List<string> fsArgs = new List<string>();
            List<string> httpArgs = new List<string>();
            foreach(string a in args)
            {
                if(PathHelper.IsValidFS(a))
                {
                    fsArgs.Add(a);
                }
                else if(PathHelper.IsValidUri(a))
                {
                    httpArgs.Add(a);
                }
            }
            
            //如果是文件系统中的文件，需要移动文件
            string[] fsSrc = fsArgs.ToArray();
            string[] fsDst = PathHelper.MapFilesToTagDir(fsSrc, SelectedTag.Title);
            FileShell.SHMoveFiles(fsSrc, fsDst);
            UriDB.MoveUris(fsSrc, fsDst, SelectedTag.Title);

            //如果是链接，只需要更新tag列表
            UriDB.MoveUris(httpArgs.ToArray(), null, SelectedTag.Title);
            //UriDB.DelUris(fsSrc, false);  //TODO bug2:对于http链接，删除后，标题就没有了。
           
            //UriDB.AddUris(fsDst, new List<string>() { SelectedTag.Title });
           
        }
        private void PasteFiles() { PasteFiles(true); }
        private void PasteFiles(bool NeedCopy,bool download = false)
        {
            Cursor bak = this.Cursor;
            try
            {
                this.Cursor = Cursors.Wait;
                UpdateCurrentTagByContextMenu();
                AddUri(FileShell.GetFileListFromClipboard(), NeedCopy,download);
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
                SetCurrentTag(t.GUTag,true);
            }
        }
        private void AddUri(List<string> files) { AddUri(files, true); }
        private void AddUri(List<string> files, bool NeedCopy,bool download = false)
        {
            if (UriDB == null || SelectedTag == null ||SelectedTag.Title==null || SelectedTag.Title.Length == 0) return;

            List<string> tags = new List<string>() { SelectedTag.Title };
            IEnumerable<string> dst = null;
            if (NeedCopy)
            {
                dst = CopyToHouse(files.ToArray(), SelectedTag.Title);
            }
            else
            {
                dst = files;
            }

            //TODO,这个动作时间很长，整个过程中界面没有响应。
            UriDB.AddUris(dst, tags);
            foreach (string uri in dst)
            {
                if (PathHelper.IsValidWebLink(uri))
                {
                    //string title = WebHelper.GetWebTitle(uri);
                    //if (title != null)
                    //{
                    //    UriDB.UpdateTitle(uri, title);
                    //}
                    BackTask.Ins.Add(new UpdateTitleTaskInf(uri, UriDB, SelectedTag.Title,download));
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
                if (PathHelper.IsValidWebLink(f))
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
                    string dstDir = CfgPath.GetDirByTag(tag,true);//新建文件，保证目录存在
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
            ClipBoardSafe.SetText(SelectedTag.Title);

        }

        private void miCopyTagFullPath_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            ClipBoardSafe.SetText(CfgPath.GetDirByTag(SelectedTag.Title,true)); //拷贝完整路径名，需要保证目录存在
        }

        private void tagAreaMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
        }

        //private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    double h = e.NewSize.Height;
            

        //    //canvasMinHeight = this.ActualHeight - 60;
        //    //SetHeight();
            
        //    System.Diagnostics.Debug.WriteLine(e.NewSize.Height+" "+e.NewSize.Width + " "+rootTag + " "+currentTag);
        //    RedrawGraph();
        //}

        private void miNewFile_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            string initDir = CfgPath.GetDirByTag(SelectedTag.Title,true);//新建文件，保证目录存在
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
                        FileShell.OpenFile(sf.FileName);
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
            ClipBoardSafe.SetText(ClipboardConst.KUMMERWU_TAG_COPY + ClipboardConst.CommandSplitToken + SelectedTag.Id);
            UpdateSelectedStatus(SelectedTag, TagBox.Status.Copy);
        }

        public void miCutTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            ClipBoardSafe.SetText(ClipboardConst.KUMMERWU_TAG_CUT + ClipboardConst.CommandSplitToken + SelectedTag.Id);
            UpdateSelectedStatus(SelectedTag, TagBox.Status.Cut);
        }

        private void miDeleteTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            if (TagDB.QueryTagChildren(SelectedTag).Count == 0)
            {
                GUTag oldCurrentTag, newCurrentTag;
                GetNextTag(out oldCurrentTag, out newCurrentTag);

                TagDB.RemoveTag(oldCurrentTag);

                //如果新选出来的当前节点在视图中，直接选中该tag
                foreach (UIElement u in allTagBox)
                {
                    TagBox t = u as TagBox;
                    if (t != null && t.GUTag == newCurrentTag)
                    {
                        SetCurrentTag(newCurrentTag, true);
                    }
                }
                //当新选出来的tag不再视图中时，才需要切换视图的根节点
                if (SelectedTag != newCurrentTag)
                {
                    ChangeRoot(newCurrentTag, newCurrentTag);
                }
                else
                {
                    RedrawGraph();
                }
                //如果该title的所有tag全部被删除，则需要删除Tag所在目录：
                //这个调用之所以放在这儿，而不放在TagDB.RemoveTag时调用，
                //是因为在彻底删除该tag后（转到其他tag后），程序打开的标签笔记才会被关闭。
                //这个时候才能删除tag的目录（否则会有文件正在使用无法移动目录）
                if (TagDB.QueryTags(oldCurrentTag.Title).Count == 0) //确保没有同名标签，才要删除目录，否则不要删除目录
                {
                    BackTask.Ins.Add(new DelTagTaskInf(oldCurrentTag.Title));
                }
            }
            else
            {
                MessageBox.Show(string.Format("[{0}]下还有其他子节点，如果确实需要删除该标签，请先删除所有子节点", SelectedTag), "提示：", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void GetNextTag(out GUTag oldCurrentTag, out GUTag newCurrentTag)
        {
            oldCurrentTag = SelectedTag;
            List<GUTag> parents = TagDB.QueryTagParent(oldCurrentTag);

            //当前节点删除了，找到一个合适的节点作为删除后的新的当前节点
            newCurrentTag = NavigateTagBox(Key.Left);
            if (parents.Count == 1)
            {
                int i = parents[0].GetChildPos(oldCurrentTag);
                newCurrentTag = null;
                if (parents[0].Children.Count > 1)
                {
                    if (i >= parents[0].Children.Count - 1)//当前节点是所在父节点的最后一个节点
                    {
                        newCurrentTag = TagDB.GetTag(parents[0].Children[i - 1]);
                    }
                    else //i < count - 1 == 》 i<=count-2
                    {
                        newCurrentTag = TagDB.GetTag(parents[0].Children[i + 1]);
                    }
                }

                if (newCurrentTag == null)
                {
                    newCurrentTag = parents[0];
                }
            }
            if (newCurrentTag == null)
            {
                newCurrentTag = TagDB.GetTag(StaticCfg.Ins.DefaultTagID);
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
        private void EnsureVisible(GUTag newTag,GUTag currentRoot)
        {
            int MaxLevel = GTagBoxTree.CalcMaxLevel(MyLayoutMode);
            GUTag tmp = newTag;
            for(int i = 0;i<MaxLevel;i++)
            {
                List<GUTag> tmpP = TagDB.QueryTagParent(tmp);
                if(tmpP.Count>0)
                {
                    tmp = tmpP[0];
                }
                else //无法回溯，直接返回
                {
                    return;
                }

                if(tmp==currentRoot) //根节点已经可见，直接返回。
                {
                    return;
                }
            }
            ChangeRoot(tmp, newTag);
        }
        private void miNewTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            if (SelectedTag == null) return;
            //TODO 如果有多个创建子标签如何正确处理？
            GUTag newTag = TagDB.NewTag(StaticCfg.Ins.DefaultNewTag);
            TagDB.SetParent(SelectedTag, newTag);
            //完善：如果新建Tag不在可见范围内，更新根节点。

            //RedrawGraph();
            EnsureVisible(newTag, RootTag);

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
                                TagDB.SetParent(SelectedTag, argTag);
                                RedrawGraph();
                            }
                        }
                        break;
                    case ClipboardConst.KUMMERWU_TAG_CUT:
                        {
                            GUTag argTag = TagDB.GetTag(Guid.Parse(arg));
                            if (argTag != null)
                            {
                                TagDB.ResetParent(SelectedTag, argTag);
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
            string dir = CfgPath.GetDirByTag(SelectedTag.Title,true);//拷贝目录名，需要保证路径存在
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
        private void PasteDownloadFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PasteFiles(false, true);
        }
        private void LinkFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miLinkInFile_Click(sender, e);
        }
        private void EditFile(string dotPostfix)
        {
            UpdateCurrentTagByContextMenu();
            if (SelectedTag == null ) return;

            string defaultFile = CfgPath.GetTemplateFileByTag(SelectedTag.Title, dotPostfix);
            if (defaultFile == null) return;

            FileShell.OpenFile(defaultFile);
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
                    TagBox t = FindTagBox(SelectedTag);
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
            if (SelectedTag == null ) return;
            TagDB.ChangeChildPos(SelectedTag, direct);
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
            if (SelectedTag == null ) return;
            List<GUTag> ps = TagDB.QueryTagParent(SelectedTag);
            if (ps.Count == 0) return;

            GUTag parent = ps[0];
            GUTag newTag = TagDB.NewTag(StaticCfg.Ins.DefaultNewTag);
            TagDB.SetParent(parent, newTag);//TODO 如果有多个创建子标签如何正确处理？
            RedrawGraph();
            TagBox b = ChangeSelectd(newTag);
            FloatTextBox.Ins.ShowEdit(canvas, b);
        }

        #region 修改Tag布局
        private void ChangeLayoutMode(LayoutMode m)
        {
            SaveLayoutMode(m);
            UpdateMenuItemCheckStatus();
            RedrawGraph();
        }

        

        private void UpdateMenuItemCheckStatus()
        {
            LayoutMode m = MyLayoutMode;
            ContextMenu cm = Resources["layoutModeMenu"] as ContextMenu;
            for (int i = 0; i < cm.Items.Count; i++)
            {
                MenuItem it = cm.Items[i] as MenuItem;
                if (it != null)
                {
                    it.IsChecked = (
                                    (m == LayoutMode.TREE_NO_COMPACT && it.Name == "miNormalTree") ||
                                    (m == LayoutMode.TREE_COMPACT && it.Name == "miCompactTree") ||
                                    (m == LayoutMode.TREE_COMPACT_MORE && it.Name == "miCompactMoreTree") ||

                                    (m == LayoutMode.LRTREE_NO_COMPACT && it.Name == "miNormalLRTree") ||
                                    (m == LayoutMode.LRTREE_COMPACT && it.Name == "miCompactLRTree") ||
                                    (m == LayoutMode.LRTREE_COMPACT_MORE && it.Name == "miCompactMoreLRTree")
                                  );
                }
            }
        }

        private void miNormalTree_Click(object sender, RoutedEventArgs e)
        {
            ChangeLayoutMode(LayoutMode.TREE_NO_COMPACT);
        }
        private void miCompactTree_Click(object sender, RoutedEventArgs e)
        {
            ChangeLayoutMode(LayoutMode.TREE_COMPACT);
        }
        private void miCompactMoreTree_Click(object sender, RoutedEventArgs e)
        {
            ChangeLayoutMode(LayoutMode.TREE_COMPACT_MORE);
        }
        private void miNormalLRTree_Click(object sender, RoutedEventArgs e)
        {
            ChangeLayoutMode(LayoutMode.LRTREE_NO_COMPACT);
        }
        private void miCompactLRTree_Click(object sender, RoutedEventArgs e)
        {
            ChangeLayoutMode(LayoutMode.LRTREE_COMPACT);
        }
        private void miCompactMoreLRTree_Click(object sender, RoutedEventArgs e)
        {
            ChangeLayoutMode(LayoutMode.LRTREE_COMPACT_MORE);
        }

        #endregion

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
    }
    
    
}
