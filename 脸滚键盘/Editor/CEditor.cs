﻿using ICSharpCode.AvalonEdit;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace NSMain.Editor
{
    public class CEditor
    {
        /// <summary>
        /// 编辑面板关闭事件
        /// </summary>
        /// <param name="tabItem"></param>
        /// <param name="e"></param>
        public static void TabItemClosing(HandyControl.Controls.TabItem tabItem, EventArgs e)
        {
            UEditor ucEditor = tabItem.Content as UEditor;
            if (ucEditor.BtnSaveDoc.IsEnabled == true)
            {
                MessageBoxResult dr = MessageBox.Show("该章节尚未保存\n要在退出前保存更改吗？", "Tip", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (dr == MessageBoxResult.Yes)
                {
                    ucEditor.BtnSaveText_Click(null, null);
                }
                if (dr == MessageBoxResult.No)
                {

                }
                if (dr == MessageBoxResult.Cancel)
                {
                    (e as HandyControl.Data.CancelRoutedEventArgs).Cancel = true;
                }
            }
        }

        /// <summary>
        /// 字符串转化为字节流
        /// </summary>
        /// <param name="strContent"></param>
        /// <returns></returns>
        public static MemoryStream ConvertStringToStream(string strContent)
        {
            byte[] array = Encoding.UTF8.GetBytes(strContent);
            MemoryStream stream = new MemoryStream(array);
            return stream;
        }

        /// <summary>
        /// 获取最大行数
        /// </summary>
        public static int GetMaxLineNum(TextBox tb)
        {
            int n = 0;
            for (var i = 0; i < tb.LineCount; i++)
            {
                //存在非空字符且不存在换行符（统计段落）
                if (IsHasWords(tb.GetLineText(i)))
                {
                    if (i > 0)
                    {
                        //第二行的情况，判断上一行是否存在换行
                        if (tb.GetLineText(i - 1).Contains("\n"))
                        {
                            n++;
                        }
                    }
                    else
                    {
                        //第一行的情况
                        n++;
                    }
                }
            }
            return n;
        }

        /// <summary>
        /// 文字排版，并重新赋值给编辑框
        /// </summary>
        /// <param name="tb"></param>
        public static void TypeSetting(TextEditor tb)
        {
            string reText = "　　"; //开头是两个全角空格
            string[] sArray = tb.Text.Split(new char[] { '\r', '\n', '\t'});
            string[] sArrayNoEmpty = sArray.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            foreach (string lineStr in sArrayNoEmpty)
            {
                //当前段落非空时，注意，这里的长度需要-1才是最后一个索引号
                if (Array.IndexOf(sArrayNoEmpty, lineStr) != sArrayNoEmpty.Length - 1)
                {
                    //非末尾的情况
                    reText += lineStr.Trim() + "\n\n　　";
                }
                else
                {
                    //末尾时不添加新行
                    reText += lineStr.Trim();
                }
            }
            //排版完成，重新赋值给文本框
            tb.Text = reText;
            //光标移动至文末 
            tb.ScrollToLine(tb.LineCount);
            tb.SelectionLength = 0;
            tb.SelectionStart = tb.Text.Length;
            tb.ScrollToEnd();
            tb.ScrollToEnd();
        }

        /// <summary>
        /// 字数统计
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static int CountWords(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return 0;
            }
            int total = 0;
            char[] q = content.ToCharArray();
            for (int i = 0; i < q.Length; i++)
            {
                if (q[i] > 32 && q[i] != 0xA0 && q[i] != 0x3000) // 非空字符，Unicode编码0x3000为全角空格，
                {
                    total += 1;
                }
            }
            return total;
        }

        /// <summary>
        /// 判断是否存在可见字符串
        /// </summary>
        /// <param name="StrContert"></param>
        /// <returns></returns>
        public static bool IsHasWords(string StrContert)
        {
            if (string.IsNullOrEmpty(StrContert))
            {
                return false;
            }
            char[] q = StrContert.ToCharArray();
            for (int i = 0; i < q.Length; i++)
            {
                if (q[i] > 32 && q[i] != 0xa0 && q[i] != 0x3000) // 非空字符，Unicode编码0x3000为全角空格，
                {
                    return true;
                }
            }
            return false;
        }
    }
}
