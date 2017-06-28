﻿using AnyTag.BL;
using AnyTags.Net;
using LuceneTest.TagLayout;
using LuceneTest.TagMgr;
using LuceneTest.UriMgr;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace LuceneTest.TagGraph
{
    /// <summary>
    /// TagCanvas.xaml 的交互逻辑
    /// </summary>
    public partial class TagCanvas : UserControl
    {
        public TagCanvas()
        {
            InitializeComponent();
        }
        public double CanvasMinHeight
        {
            set
            {
                canvasMinHeight = value;
                SetHeight();
                
            }
        }
        private double canvasMinHeight = 0;
        private double realHeight = 0;
        private void SetHeight()
        {
            if(double.IsNaN(canvasMinHeight) || double.IsInfinity(canvasMinHeight) )
            {
                canvasMinHeight = 0;
            }
            canvas.Height = Math.Max(realHeight, canvasMinHeight);
        }
        private ITagDB tagDB = null;
        public IUriDB UriDB = null;
        private string root;
        
        public void Update()
        {
            if(tagDB!=null && root!=null)
            {
                Update(tagDB, root);
            }
        }
        public void Update(string root)
        {
            if (tagDB != null)
            {
                Update(tagDB, root);
            }
        }
        public void Update(ITagDB tagDB,string root)
        {
            this.tagDB = tagDB;
            this.root = root;
            
            canvas.Children.Clear();
            ITagLayout tagLayout = TagLayoutFactory.CreateLayout();
            tagLayout.Layout(tagDB, root);
            IEnumerable<UIElement> lines = tagLayout.Lines;
            IEnumerable<UIElement> allTxt = tagLayout.TagArea;
            canvas.Width = tagLayout.Size.Width;
            realHeight = tagLayout.Size.Height;
            SetHeight();
            
            foreach (Line l in lines)
            {
                canvas.Children.Add(l);
            }
            foreach (TagBox t in allTxt)
            {
                t.ContextMenu = TagAreaMenu;
                t.MouseLeftButtonDown += T_MouseLeftButtonDown;
                t.MouseDoubleClick += T_MouseDoubleClick;
                canvas.Children.Add(t);
            }
            SetCurrentTag(root);
        }

        private void T_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TagBox b = sender as TagBox;
            if(b!=null)
                Update(b.Text);
        }

        private void T_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is TagBox)
            {
                SetCurrentTag((sender as TagBox).Text);
            }
        }

        private void scrollViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TagBox)
            {
                TagBox b = (TagBox)e.OriginalSource;
                Update(b.Text);
            }
        }

        private void canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void canvas_Click(object sender, RoutedEventArgs e)
        {
            
        }
        ContextMenu TagAreaMenu
        {
            get
            {
                return Resources["tagAreaMenu"] as ContextMenu;
            }
        }
        private void tagAreaNode_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            FileOperator.OpenTagDir(currentTag);
            
        }
        Border border = null;
        private void SetBorder(string tag)
        {
            foreach(UIElement u in canvas.Children)
            {
                TagBox tb = u as TagBox;

                if(tb!=null)
                {
                    if (tb.Selected)
                    {
                        tb.Selected = false;
                    }
                    if (tb.Text == tag)
                    {
                        tb.Selected = true;
                    }
                }
            }
            
            //TextBlock tagBlock = null;

            //foreach(UIElement u in canvas.Children)
            //{
            //    if(u is TextBlock)
            //    {
            //        TextBlock t = u as TextBlock;
            //        if (t != null && t.Text == tag)
            //        {
            //            tagBlock = t;
            //        }
            //    }
                
            //}

            //if (tagBlock != null)
            //{
            //    TextBlock old = null;
            //    if (border == null)
            //    {
            //        border = new Border();
            //        border.BorderBrush = new SolidColorBrush(Colors.Black);
            //        border.BorderThickness = new Thickness(2);

            //    }
            //    else
            //    {
            //        old = border.Child as TextBlock;
            //        old.Margin = new Thickness(border.Margin.Left + 2, border.Margin.Top + 2, 0, 0);
            //        canvas.Children.Remove(border);
            //        border.Child = null;
            //        canvas.Children.Add(old);
            //    }

            //    canvas.Children.Remove(tagBlock);
            //    border.Width = tagBlock.Width + 4;
            //    border.Height = tagBlock.Height + 4;
            //    border.Margin = new Thickness(tagBlock.Margin.Left-2, tagBlock.Margin.Top-2, 0,0);
            //    tagBlock.Margin = new Thickness();
            //    border.Child = tagBlock;
            //    canvas.Children.Add(border);

            //}
        }
        private string currentTag = "";
        public void SetCurrentTag(string tag)
        {
            SetBorder(tag); //这一句必须放在下面检查并return之前，
                            //即无论currentTag是否变化，都需要更新一下border，否则会有bug；
                            //bug现象：在curtag没有变化的时候，重新绘制整个graph，
                            //会出现所有的tag都不显示边框（包括curtag），因为直接返回了。
            
            //if (currentTag == tag) return;
            string oldTag = currentTag;
            currentTag = tag;

            
            CurrentTagInf.Text = currentTag;
            if(TagChangedHandler!=null)
            {
                TagChangedHandler(tag);
            }
        }

        public delegate void CurrentTagChanged(string tag);
        public CurrentTagChanged TagChangedHandler = null;

        private void miPaste_Click(object sender, RoutedEventArgs e)
        {
            Paste();
        }
        public void Paste()
        {
            UpdateCurrentTagByContextMenu();
            AddUri(FileOperator.GetFileListFromClipboard());
        }
        private void UpdateCurrentTagByContextMenu()
        {
            TagBox t = TagAreaMenu.PlacementTarget as TagBox;
            if (t != null && t.Text.Length > 0)
            {
                SetCurrentTag(t.Text);
            }
        }

        private void AddUri(List<string> files)
        {
            if (UriDB == null || currentTag==null || currentTag.Length==0) return;

            List<string> tags = new List<string>() { currentTag };
            foreach(string f in files)
            {
                string dstFile = CopyToHouse(f, currentTag);
                if (dstFile != null)
                {
                    UriDB.AddUri(dstFile, tags);
                }
                
            }
        }
        private string  CopyToHouse(string f, string tag)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(f);
            string dstDir = MyPath.GetDirPath(tag);
            string dstFile = System.IO.Path.Combine(dstDir, fi.Name);
            if (!System.IO.File.Exists(dstFile))//TODO 已经存在的需要提示覆盖、放弃、重命名
            {
                if(FileOperator.CopyFile(f, dstFile))
                {
                    return dstFile;
                }
            }
            return null;
        }
        private void copyAreaNode_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            Clipboard.SetText(currentTag);
        }

        private void copyFullPath_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            Clipboard.SetText(MyPath.GetDirPath(currentTag));
        }

        private void tagAreaMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
        }

        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double h = e.NewSize.Height;
            Grid p = scrollViewer.Parent as Grid;

            canvasMinHeight = p.ActualHeight;
            SetHeight();
        }
    }
}
