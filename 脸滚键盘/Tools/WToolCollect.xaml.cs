﻿using JiebaNet.Analyser;
using NSMain.Bricks;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using static NSMain.TreeViewPlus.CNodeModule;

namespace NSMain.Tools
{
    /// <summary>
    /// CollectToolWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WToolCollect : Window
    {
        public WToolCollect()
        {
            InitializeComponent();
        }

        public TreeViewNode TopNode
        {
            get { return (TreeViewNode)GetValue(TopNodeProperty); }
            set { SetValue(TopNodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TopNode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopNodeProperty =
            DependencyProperty.Register("TopNode", typeof(TreeViewNode), typeof(WToolCollect), new PropertyMetadata(new TreeViewNode { Uid = "", IsDir = true }));




        public TreeViewNode CurNode
        {
            get { return (TreeViewNode)GetValue(CurNodeProperty); }
            set { SetValue(CurNodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurNode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurNodeProperty =
            DependencyProperty.Register("CurNode", typeof(TreeViewNode), typeof(WToolCollect), new PropertyMetadata(null));


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CurNode = GlobalVal.Uc.TreeBook.CurNode;
            TopNode = GlobalVal.Uc.TreeBook.TopNode;
        }

        private void BtnCollect_Click(object sender, RoutedEventArgs e)
        {
            if (CurNode == null)
            {
                return;
            }

            var idf = new TfidfExtractor();
            foreach (string item in idf.ExtractTags(CurNode.NodeContent, 10))
            {
                TbKeyWords.Text += item + " ";
            }
            List<string> names = new List<string>();
            if (CurNode.IsDir == true)
            {
                foreach (TreeViewNode node in CurNode.ChildNodes)
                {
                    GetNames(names, node);
                }
            }
            else
            {
                GetNames(names, CurNode);
            }

            names = names.OrderBy(s => s.Length).ToList();
            foreach (string name in names)
            {
                if (IsName(names, name))
                {
                    TbNames.Text += name + " ";
                }
            }
        }

        void GetNames(List<string> names, TreeViewNode node)
        {
            string text = node.NodeContent;
            text = Regex.Replace(text, "　", "", RegexOptions.Multiline);

            string[] pArray =
            {
                "^(.{2,4})(?=(豪气..|淡淡说|幽幽说|笑着说|仍自笑|默默.|笑呵呵|呵呵.|肃然说)道)",
                "^(.{2,4})(?=已然坐在|睁开眼睛|面色苍白|微微一.|深以为.|面露..之色|默然..)",
                "^(.{2,4})(?=(淡淡|冷笑|好笑|干笑|苦笑|呵呵|含.|不甘|恐慌|愤.|惊.|提醒|豪.)道)",
                "^(.{2,4})(?=(亦|也|说|笑|恨|惊|怒)道)",
                };
            foreach (string p in pArray)
            {
                MatchCollection ms = Regex.Matches(text, p, RegexOptions.Multiline);
                if (ms.Count > 0)
                {
                    foreach (Match m in ms)
                    {
                        text = Regex.Replace(text, m.Value, "【【【【" + m.Value + "】】】】", RegexOptions.Multiline);
                    }
                }
            }
            MatchCollection ms2 = Regex.Matches(text, "(?<=^【【【【)(.{2,4})(?=】】】】)", RegexOptions.Multiline);
            if (ms2.Count > 0)
            {
                foreach (Match m in ms2)
                {
                    if (false == names.Contains(m.Value))
                    {
                        names.Add(m.Value);
                    }
                }
            }
        }

        bool IsName(List<string> names, string value)
        {
            foreach (string name in names)
            {
                if (name != value && name.Contains(value))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
