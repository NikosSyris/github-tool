﻿using System;
using Octokit;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using GitHub_Tool.Action;
using GitHub_Tool.Model;
using Model = GitHub_Tool.Model;
using GitHub_Tool.Action.Validation;

namespace GitHub_Tool.GUI
{

    public partial class SearchCodeUserControl : UserControl
    {

        public string Owner { get; set; }
        public string FileName { get; set; }   
        public string Term { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public int Size { get; set; }
        CodeSearch codeSearch;
        DateValidator DateValidator;

        public SearchCodeUserControl()
        {
            InitializeComponent();
            codeSearch = new CodeSearch();
            DateValidator = new DateValidator();
            languageComboBox.ItemsSource = Enum.GetValues(typeof(Language));
            languageComboBox.SelectedIndex = (int)Enum.Parse(typeof(Language), "Unknown");
            DataContext = this;
        }


        private async void searchCodeButtonClick(object sender, RoutedEventArgs e)
        {
            if ( isValid(termTextBox, ownerTextBox, fileNameTextBox, extensionTextBox, pathTextBox, sizeTextBox))
            {
                searchRepos.IsEnabled = false;
                GlobalVariables.accessToken = accessTokenTextBox.Text;
                GlobalVariables.client = GlobalVariables.createGithubClient();
                noResultsLabel.Visibility = Visibility.Hidden;

                SearchCodeRequestParameters requestParameters = new SearchCodeRequestParameters(termTextBox.Text, extensionTextBox.Text, ownerTextBox.Text,
                                                                    Int32.Parse(sizeTextBox.Text), languageComboBox.Text, pathIncludedCheckBox.IsChecked, forkComboBox.Text,
                                                                    fileNameTextBox.Text, pathTextBox.Text, sizeComboBox.Text);
                try
                {
                    var result = await codeSearch.searchCode(requestParameters);
                    noResultsLabel.Visibility = pickVisibility(result.Count);
                    filesDataGrid.ItemsSource = result;
                    searchRepos.IsEnabled = true;
                    numberOfResults.Text = result.Count.ToString();
                }
                catch (ApiValidationException)
                {
                    MessageBox.Show("Searches that use qualifiers only are not allowed. Include one of the: search term," +
                        "owner or file name", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (RateLimitExceededException exception)
                {
                    MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

        }


        private bool isValid(TextBox term, TextBox owner, TextBox fileName, TextBox extension, TextBox path, TextBox size)
        {
            return !Validation.GetHasError(term) && !Validation.GetHasError(owner) && !Validation.GetHasError(path)
                    && !Validation.GetHasError(fileName) && !Validation.GetHasError(size) && !Validation.GetHasError(extension);
        }

        private Visibility pickVisibility(int numberOfResults)
        {
            if (numberOfResults == 0)
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }


        private async void showCommitsOnClick(object sender, RoutedEventArgs e)
        {

            CommitWindow commitWindow = new CommitWindow();
            var selectedFile = (File)filesDataGrid.CurrentCell.Item;
            List<Model.Commit> commitList = await codeSearch.getCommitsForFIle(selectedFile.Owner, selectedFile.RepoName, selectedFile.Path).ConfigureAwait(false);

            this.Dispatcher.Invoke(() =>
            {
                commitWindow.CommitsDataGrid.ItemsSource = commitList;
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
