﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using GitHub_Tool.Action;
using GitHub_Tool.Model;

namespace GitHub_Tool.GUI
{


    public partial class RepositoryWindow : Window
    {

        private Folder root;
        CodeSearch codeSearch;

        public RepositoryWindow(Folder rootFolder)
        {
            InitializeComponent();
            root = rootFolder;
            codeSearch = new CodeSearch();
        }


        private void loadTreeView(object sender, RoutedEventArgs e)
        {

            //Image image = new Image();
            //image.Height = 16;
            //image.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/folder.png"));

            TreeViewItem item = new TreeViewItem();
            item.Header = root.Name;

            item = getFolder(root,item);

            var tree = sender as TreeView;
            tree.Items.Add(item);
        }


        private  TreeViewItem  getFolder(Folder folder, TreeViewItem item)
        {

            foreach (Folder tempFolder in folder.FolderList)
            {
                TreeViewItem tempItem = new TreeViewItem();
                tempItem.Header = tempFolder.Name;
                item.Items.Add(tempItem);
                tempItem = getFolder(tempFolder, tempItem);
            }

            foreach (File file in folder.FileList)
            {
                item.Items.Add(new TreeViewItem() { Header = file.Name });
            }

            return item;
        }


        private async void showCommitsOnClick(object sender, RoutedEventArgs e)
        {
            
            CommitWindow commitWindow = new CommitWindow();

            var selectedFile = (File)filesDataGrid.CurrentCell.Item;
            List<Commit> commitList = await codeSearch.getCommitsForFIle(selectedFile.Owner, selectedFile.RepoName, selectedFile.Path).ConfigureAwait(false);

            this.Dispatcher.Invoke(() =>
            {
                commitWindow.commitsDataGrid.ItemsSource = commitList;
                commitWindow.Show();
            });
        }

        private void enableDataGridCopying(object sender, DataGridRowClipboardEventArgs e)
        {

            var currentCell = e.ClipboardRowContent[filesDataGrid.CurrentCell.Column.DisplayIndex];
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(currentCell);

        }

        private void HyperlinkOnClick(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)e.OriginalSource;
            Process.Start(link.NavigateUri.AbsoluteUri);
        }
    }
}
