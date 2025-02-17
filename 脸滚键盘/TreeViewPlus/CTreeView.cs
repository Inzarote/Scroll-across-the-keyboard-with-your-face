﻿using NSMain.Bricks;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static NSMain.TreeViewPlus.CNodeModule;

namespace NSMain.TreeViewPlus
{
    public class CTreeView
    {
      
        #region 向上/向下调整节点


        /// <summary>
        /// 方法：向上调整节点位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void NodeMoveUp(string CurBookName, string TypeOfTree, TreeViewNode selectedNode, ObservableCollection<TreeViewNode> treeViewNodeList)
        {
            if (selectedNode.ParentNode.ChildNodes.IndexOf(selectedNode) == 0)
            {
                return;
            }

            int n = selectedNode.ParentNode.ChildNodes.IndexOf(selectedNode);
            TreeViewNode neighboringNode = selectedNode.ParentNode.ChildNodes[n - 1];
            if (selectedNode == null || neighboringNode == null)
            {
                return;
            }

            //数据库中的处理
            SwapNodeBySql(CurBookName, TypeOfTree, selectedNode, neighboringNode);

            //节点索引交换位置
            SwapNode(n - 1, selectedNode, neighboringNode.ParentNode, treeViewNodeList);
        }

        /// <summary>
        /// 方法：向下调整节点位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void NodeMoveDown(string CurBookName, string TypeOfTree, TreeViewNode selectedNode, ObservableCollection<TreeViewNode> treeViewNodeList)
        {
            if (selectedNode.ParentNode.ChildNodes.IndexOf(selectedNode) == selectedNode.ParentNode.ChildNodes.Count - 1)
            {
                return;
            }
            int n = selectedNode.ParentNode.ChildNodes.IndexOf(selectedNode);
            TreeViewNode neighboringNode = selectedNode.ParentNode.ChildNodes[n + 1];
            if (selectedNode == null || neighboringNode == null)
            {
                return;
            }

            //数据库中的处理
            SwapNodeBySql(CurBookName, TypeOfTree, selectedNode, neighboringNode);

            //节点索引交换位置
            SwapNode(n + 1, selectedNode, neighboringNode.ParentNode, treeViewNodeList);
        }


        #endregion

        #region 节点拖曳/移动（改变索引）

        public static void SwapNode(int m, TreeViewNode dragNode, TreeViewNode parentNode, ObservableCollection<TreeViewNode> treeViewNodeList)
        {
            TreeViewNode tempNode = dragNode;
            if (tempNode.Pid == "")
            {
                treeViewNodeList.Remove(dragNode);
                treeViewNodeList.Insert(m, dragNode);
            }
            tempNode.ParentNode.ChildNodes.Remove(dragNode);
            parentNode.ChildNodes.Insert(m, tempNode);
        }

        public static void MouseMoveMethod(TreeView Tv, MouseEventArgs e, Point _lastMouseDown)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    //获取鼠标移动的距离
                    Point currentPosition = e.GetPosition(Tv);

                    //判断鼠标是否移动
                    if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                        (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                    {
                        //获取鼠标选中的节点数据
                        TreeViewNode draggedNode = (TreeViewNode)Tv.SelectedItem;
                        if (draggedNode != null && draggedNode.IsButton == false)
                        {
                            //启动拖放操作
                            //DragDropEffects finalDropEffect = DragDrop.DoDragDrop(treeView, treeView.SelectedValue,System.Windows.DragDropEffects.Move);
                            DragDrop.DoDragDrop(Tv, Tv.SelectedValue, System.Windows.DragDropEffects.Move);
                            e.Handled = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region 添加或者删除节点/按钮
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="baseNode"></param>
        public static TreeViewNode AddNewNode(ObservableCollection<TreeViewNode> treeViewNodeList, TreeViewNode baseNode, string typeOfTree)
        {

            string guid = Guid.NewGuid().ToString();
            TreeViewNode newNode = new TreeViewNode(guid, "新节点")
            {
                Pid = baseNode.Uid,
                NodeContent = ""
            };
            if (string.IsNullOrEmpty(baseNode.Uid))

            {
                newNode.IsDir = true;
                //AddButtonNode(treeViewNodeList, newNode);
                treeViewNodeList.Add(newNode);
                baseNode.ChildNodes.Add(newNode);
            }
            else
            {
                newNode.IsDir = false;
                baseNode.ChildNodes.Add(newNode);
            }
            if (newNode.IsDir == true)
            {
                if (typeOfTree == "history")
                {
                    newNode.IconPath = GlobalVal.Path.Resourses + "/图标/目录树/ic_action_knight.png";
                }
                else
                {
                    newNode.IconPath = GlobalVal.Path.Resourses + "/图标/目录树/ic_action_folder_closed.png";
                }

            }
            else
            {
                newNode.IconPath = GlobalVal.Path.Resourses + "/图标/目录树/ic_action_document.png";
                if (typeOfTree == "book")
                {
                    newNode.NodeContent = "　　";
                }
            }
            return newNode;
        }

        ///// <summary>
        ///// 添加按钮
        ///// </summary>
        ///// <param name="TreeViewNodeList"></param>
        ///// <param name="baseNode"></param>
        ///// <returns></returns>
        //public static TreeViewNode AddButtonNode(ObservableCollection<TreeViewNode> TreeViewNodeList, TreeViewNode baseNode)
        //{

        //    TreeViewNode button;
        //    if (baseNode.Uid == "")
        //    {
        //        button = new TreeViewNode(null, "双击添加根节点", true, true);
        //    }
        //    else
        //    {
        //        button = new TreeViewNode(null, "双击添加子节点", true, true);
        //    }
        //    if (string.IsNullOrEmpty(baseNode.Uid))
        //    {
        //        TreeViewNodeList.Add(button);
        //    }
        //    button.IconPath = GlobalVal.Path.Resourses + "/图标/目录树/ic_action_add.png";
        //    baseNode.ChildNodes.Add(button);
        //    return button;
        //}

        /// <summary>
        /// 在数据库中添加节点记录
        /// </summary>
        /// <param name="curBookName"></param>
        /// <param name="typeOfTree"></param>
        /// <param name="selectedNode"></param>
        /// <param name="treeViewNodeList"></param>
        public static void AddNodeBySql(string curBookName, string typeOfTree, TreeViewNode newNode)
        {
            string tableName = typeOfTree;
            CSqlitePlus cSqlite = GlobalVal.SQLClass.Pools[curBookName];
            string sql = string.Format("INSERT INTO Tree_{0} (Uid, Pid, NodeName, isDir, NodeContent, WordsCount, IsExpanded, IsChecked) VALUES ('{1}', '{2}', '{3}', {4}, '{5}', {6}, {7}, {8});", tableName, newNode.Uid, newNode.Pid, newNode.NodeName.Replace("'", "''"), newNode.IsDir, newNode.NodeContent.Replace("'", "''"), newNode.WordsCount, newNode.IsExpanded, newNode.IsChecked);
            cSqlite.ExecuteNonQuery(sql);

        }
        #endregion



        #region 载入书籍
        //public static void SaveBySql(string curBookName, TreeViewNode baseNode)
        //{
        //    SqliteOperate cSqlite = GlobalVal.SQLClass.Pools[curBookName];
        //    foreach (TreeViewNode node in baseNode.ChildNodes)
        //    {
        //        SaveBySql(cSqlite, node);
        //    }

        //}

        //public static void SaveBySql(SqliteOperate cSqlite, TreeViewNode baseNode)
        //{
        //    foreach (TreeViewNode node in baseNode.ChildNodes)
        //    {
        //        SaveBySql(cSqlite, node);
        //    }
        //}

        /// <summary>
        /// 从数据库中递归载入节点记录
        /// </summary>
        /// <param name="curBookName"></param>
        /// <param name="typeOfTree"></param>
        /// <param name="TreeViewNodeList"></param>
        /// <param name="TopNode"></param>
        public static void LoadBySql(string curBookName, string typeOfTree, ObservableCollection<TreeViewNode> TreeViewNodeList, TreeViewNode TopNode)
        {
            string tableName = typeOfTree;
            CSqlitePlus cSqlite = GlobalVal.SQLClass.Pools[curBookName];
            string sql = string.Format("SELECT * FROM Tree_{0} where Pid='';", tableName);
            SQLiteDataReader reader = cSqlite.ExecuteQuery(sql);
            while (reader.Read())
            {
                if ((bool)reader["IsDel"] == true)
                {
                    continue;
                }
                TreeViewNode node = new TreeViewNode
                {
                    Uid = reader["Uid"].ToString(),
                    Pid = reader["Pid"].ToString(),
                    NodeName = reader["NodeName"].ToString(),
                    IsDir = (bool)reader["IsDir"],
                    NodeContent = reader["NodeContent"].ToString(),
                    WordsCount = Convert.ToInt32(reader["WordsCount"]),
                    IsExpanded = (bool)reader["IsExpanded"],
                    IsChecked = (bool)reader["IsChecked"],
                };
                if (node.IsExpanded == true)
                {
                    node.IconPath = GlobalVal.Path.Resourses + "/图标/目录树/ic_action_folder_open.png";
                }
                else
                {
                    node.IconPath = GlobalVal.Path.Resourses + "/图标/目录树/ic_action_folder_closed.png";
                }
                if (typeOfTree == "history")
                {
                    node.IconPath = GlobalVal.Path.Resourses + "/图标/目录树/ic_action_knight.png";
                }
                LoadNode(node, TreeViewNodeList, TopNode);
                ShowTree(cSqlite, curBookName, typeOfTree, TreeViewNodeList, node);
            }
            reader.Close();

        }

        /// <summary>
        /// 在视图中载入节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="TreeViewNodeList"></param>
        /// <param name="baseNode"></param>
        static void LoadNode(TreeViewNode node, ObservableCollection<TreeViewNode> TreeViewNodeList, TreeViewNode baseNode)
        {
            if (string.IsNullOrEmpty(baseNode.Uid))
            {
                TreeViewNodeList.Add(node);
                //AddButtonNode(TreeViewNodeList, node);
            }
            baseNode.ChildNodes.Add(node);
        }

        /// <summary>
        /// 在视图中载入节点
        /// </summary>
        /// <param name="curBookName"></param>
        /// <param name="typeOfTree"></param>
        /// <param name="TreeViewNodeList"></param>
        /// <param name="parentNode"></param>
        static void ShowTree(CSqlitePlus cSqlite, string curBookName, string typeOfTree, ObservableCollection<TreeViewNode> TreeViewNodeList, TreeViewNode parentNode)
        {
            string tableName = typeOfTree;
            string sql = string.Format("SELECT * FROM Tree_{0} where Pid='{1}';", tableName, parentNode.Uid);
            SQLiteDataReader reader = cSqlite.ExecuteQuery(sql);
            while (reader.Read())
            {
                if ((bool)reader["IsDel"] == true)
                {
                    continue;
                }
                TreeViewNode node = new TreeViewNode
                {
                    Uid = reader["Uid"].ToString(),
                    Pid = reader["Pid"].ToString(),
                    NodeName = reader["NodeName"].ToString(),
                    IsDir = (bool)reader["IsDir"],
                    NodeContent = reader["NodeContent"].ToString(),
                    WordsCount = Convert.ToInt32(reader["WordsCount"]),
                    IsExpanded = (bool)reader["IsExpanded"],
                    IsChecked = (bool)reader["IsChecked"],
                };
                node.IconPath = GlobalVal.Path.Resourses + "/图标/目录树/ic_action_document.png";
                LoadNode(node, TreeViewNodeList, parentNode);
                ShowTree(cSqlite, curBookName, typeOfTree, TreeViewNodeList, node);
            }
            reader.Close();
        }
        #endregion

        #region 在数据库中的其他操作

        /// <summary>
        /// 同级节点记录在数据库中对调顺序
        /// </summary>
        /// <param name="curBookName"></param>
        /// <param name="typeOfTree"></param>
        /// <param name="selectedNode"></param>
        /// <param name="neighboringNode"></param>
        public static void SwapNodeBySql(string curBookName, string typeOfTree, TreeViewNode selectedNode, TreeViewNode neighboringNode)
        {
            string tableName = typeOfTree;
            CSqlitePlus cSqlite = GlobalVal.SQLClass.Pools[curBookName];
            //更新数据库中临近节点记录集
            string sql = string.Format("UPDATE Tree_{0} set Uid='{1}', Pid='{2}', NodeName='{3}', isDir={4}, NodeContent='{5}', WordsCount={6}, IsExpanded={7}, IsChecked={8}, IsDel={9} where Uid = '{10}';", tableName, "temp", neighboringNode.Pid, selectedNode.NodeName, selectedNode.IsDir, selectedNode.NodeContent, selectedNode.WordsCount, selectedNode.IsExpanded, selectedNode.IsChecked, selectedNode.IsDel, neighboringNode.Uid);
            cSqlite.ExecuteNonQuery(sql);
            sql = string.Format("UPDATE Tree_{0} set Uid='{1}', Pid='{2}', NodeName='{3}', isDir={4}, NodeContent='{5}', WordsCount={6}, IsExpanded={7}, IsChecked={8}, IsDel={9}  where Uid = '{10}';", tableName, neighboringNode.Uid, neighboringNode.Pid, neighboringNode.NodeName, neighboringNode.IsDir, neighboringNode.NodeContent, neighboringNode.WordsCount, neighboringNode.IsExpanded, neighboringNode.IsChecked, neighboringNode.IsDel, selectedNode.Uid);
            cSqlite.ExecuteNonQuery(sql);
            sql = string.Format("UPDATE Tree_{0} set Uid='{1}' where Uid = 'temp';", tableName, selectedNode.Uid);
            cSqlite.ExecuteNonQuery(sql);

        }
        static string delSql;
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="curBookName"></param>
        /// <param name="typeOfTree"></param>
        /// <param name="selectedNode"></param>
        /// <param name="treeViewNodeList"></param>
        public static void DelNodeBySql(string curBookName, string typeOfTree, TreeViewNode selectedNode, ObservableCollection<TreeViewNode> treeViewNodeList)
        {
            string tableName = typeOfTree;
            CSqlitePlus cSqlite = GlobalVal.SQLClass.Pools[curBookName];
            delSql = string.Empty;
            RecursionDelBySql(curBookName, typeOfTree, selectedNode, cSqlite, treeViewNodeList);
            //回收站：delSql += string.Format("DELETE FROM Tree_{0} where Uid='{1}';", tableName, selectedNode.Uid);
            delSql += string.Format("update Tree_{0} set IsDel=True where Uid='{1}';", tableName, selectedNode.Uid);
            cSqlite.ExecuteNonQuery(delSql);
            treeViewNodeList.Remove(selectedNode);
        }

        /// <summary>
        /// 方法：递归删除子节点
        /// </summary>
        /// <param name="baseNode"></param>
        /// <param name="cSqlite"></param>
        static void RecursionDelBySql(string curBookName, string typeOfTree, TreeViewNode baseNode, CSqlitePlus cSqlite, ObservableCollection<TreeViewNode> treeViewNodeList)
        {
            if (baseNode.IsDir == true)
            {
                for (int i = 0; i < baseNode.ChildNodes.Count; i++)
                {
                    string tableName = typeOfTree;
                    //回收站：delSql += string.Format("DELETE FROM Tree_{0} where Uid = '{1}';", tableName, baseNode.ChildNodes[i].Uid);
                    delSql += string.Format("update Tree_{0} set IsDel=True where Uid='{1}';", tableName, baseNode.ChildNodes[i].Uid);
                    RecursionDelBySql(curBookName, typeOfTree, baseNode.ChildNodes[i], cSqlite, treeViewNodeList);
                    treeViewNodeList.Remove(baseNode.ChildNodes[i]);
                }
            }
        }


        /// <summary>
        /// 移动节点所需的方法
        /// </summary>
        /// <param name="curBookName"></param>
        /// <param name="typeOfTree"></param>
        /// <param name="dragNode"></param>
        /// <param name="treeViewNodeList"></param>
        public static void MoveNodeBySql(string curBookName, string typeOfTree, TreeViewNode dragNode, ObservableCollection<TreeViewNode> treeViewNodeList)
        {
            string tableName = typeOfTree;
            CSqlitePlus cSqlite = GlobalVal.SQLClass.Pools[curBookName];
            delSql = string.Empty;
            RecursionMoveBySql(curBookName, typeOfTree, dragNode, cSqlite, treeViewNodeList);
            delSql += string.Format("DELETE FROM Tree_{0} where Uid='{1}';", tableName, dragNode.Uid);
            cSqlite.ExecuteNonQuery(delSql);
            treeViewNodeList.Remove(dragNode);
            AddNodeBySql(curBookName, typeOfTree, dragNode);
        }

        /// <summary>
        /// 方法：递归移动子节点
        /// </summary>
        /// <param name="baseNode"></param>
        /// <param name="cSqlite"></param>
        static void RecursionMoveBySql(string curBookName, string typeOfTree, TreeViewNode baseNode, CSqlitePlus cSqlite, ObservableCollection<TreeViewNode> treeViewNodeList)
        {
            if (baseNode.IsDir == true)
            {
                for (int i = 0; i < baseNode.ChildNodes.Count; i++)
                {
                    string tableName = typeOfTree;
                    delSql += string.Format("DELETE FROM Tree_{0} where Uid = '{1}';", tableName, baseNode.ChildNodes[i].Uid);
                    RecursionDelBySql(curBookName, typeOfTree, baseNode.ChildNodes[i], cSqlite, treeViewNodeList);
                    treeViewNodeList.Remove(baseNode.ChildNodes[i]);
                }
            }
        }

        /// <summary>
        /// 节点伸展/缩回
        /// </summary>
        /// <param name="curBookName"></param>
        /// <param name="typeOfTree"></param>
        /// <param name="selectedNode"></param>
        public static void ExpandedCollapsedBySql(string curBookName, string typeOfTree, TreeViewNode selectedNode)
        {
            string tableName = typeOfTree;
            CSqlitePlus cSqlite = GlobalVal.SQLClass.Pools[curBookName];
            string sql = string.Format("UPDATE Tree_{0} set IsExpanded={1} where Uid = '{2}';", tableName, selectedNode.IsExpanded, selectedNode.Uid);
            cSqlite.ExecuteNonQuery(sql);

        }

        /// <summary>
        /// 节点选中/取消选中
        /// </summary>
        /// <param name="curBookName"></param>
        /// <param name="typeOfTree"></param>
        /// <param name="selectedNode"></param>
        public static void CheckedBySql(string curBookName, string typeOfTree, TreeViewNode selectedNode)
        {
            string tableName = typeOfTree;
            CSqlitePlus cSqlite = GlobalVal.SQLClass.Pools[curBookName];
            string sql = string.Format("UPDATE Tree_{0} set IsChecked={1} where Uid = '{2}';", tableName, selectedNode.IsChecked, selectedNode.Uid);
            cSqlite.ExecuteNonQuery(sql);

        }
        #endregion

        /// <summary>
        /// 获取节点所在的层级，无选中或者不在TreeView内的为-1
        /// </summary>
        /// <param name="selectedNode"></param>
        /// <returns></returns>
        public static int GetLevel(TreeViewNode selectedNode)
        {
            int level = -1;
            if (selectedNode != null)
            {
                while ((selectedNode.ParentNode as TreeViewNode) != null)
                {
                    selectedNode = selectedNode.ParentNode as TreeViewNode;
                    level--;
                }
                level = System.Math.Abs(level);
                return level;
            }
            else
            {
                return level;
            }

        }

        /// <summary>
        /// 根据节点对应的路径
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string GetPath(TreeViewNode selectedNode)
        {
            ArrayList nodes = new ArrayList();
            string nodePath = string.Empty;
            TreeViewNode curNode = selectedNode;

            if (selectedNode != null)
            {
                do
                {
                    nodes.Add(curNode);
                    curNode = curNode.ParentNode;
                } while (curNode.ParentNode != null);
                nodes.Reverse();//倒序操作
                foreach (TreeViewNode node in nodes)
                {
                    if (node.IsDir == true)
                    {
                        nodePath += "/" + node.NodeName;
                    }
                    else
                    {
                        nodePath += "/" + node.NodeName + ".txt";
                    }

                }

            }
            return nodePath;
        }

        #region 获取控件


        /// <summary>
        /// 获取子控件
        /// </summary>
        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            if (obj == null)
            {
                return null;
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        /// <summary>
        /// 按名称查找子控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // 如果子控件不是需查找的控件类型
                T childType = child as T;
                if (childType == null)
                {
                    // 在下一级控件中递归查找
                    foundChild = FindChild<T>(child, childName);

                    // 找到控件就可以中断递归操作 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // 如果控件名称符合参数条件
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                    // 在下一级控件中递归查找
                    foundChild = FindChild<T>(child, childName);

                    // 找到控件就可以中断递归操作 
                    if (foundChild != null) break;
                }
                else
                {
                    // 查找到了控件
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// 获取父控件
        /// </summary>
        public static T GetParentObjectEx<T>(DependencyObject obj) where T : FrameworkElement
        {
            if (obj == null)
            {
                return null;
            }
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T)
                {
                    return (T)parent;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        #endregion

    }
}
