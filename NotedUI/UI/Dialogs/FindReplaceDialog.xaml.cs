﻿using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using JustMVVM;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NotedUI.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for FindReplaceDialog.xaml
    /// </summary>
    public partial class FindReplaceDialog : UserControl
    {
        public ICommand SearchNextCommand { get { return new RelayCommand(SearchNextExec, () => Editor?.Text != null); } }
        public ICommand SearchPreviousCommand { get { return new RelayCommand(SearchPreviousExec, () => Editor?.Text != null); } }
        public ICommand ReplaceCommand { get { return new RelayCommand(ReplaceExec, () => Editor?.Text != null); } }
        public ICommand ReplaceAllCommand { get { return new RelayCommand(ReplaceAllExec, () => Editor?.Text != null); } }
        public ICommand MatchCaseToggleCommand { get { return new RelayCommand(MatchCaseToggleExec, () => Editor?.Text != null); } }
        public ICommand MatchWordToggleCommand { get { return new RelayCommand(MatchWordToggleExec, () => Editor?.Text != null); } }
        public ICommand RegexToggleCommand { get { return new RelayCommand(RegexToggleExec, () => Editor?.Text != null); } }
        public ICommand FocusDialogCommand { get { return new RelayCommand(DialogShownExec); } }

        public ICommand DialogShownCommand { get { return new RelayCommand(DialogShownExec); } }

        public static readonly DependencyProperty EditorProperty =
            DependencyProperty.Register("Editor",
                                        typeof(TextEditor),
                                        typeof(FindReplaceDialog),
                                        new FrameworkPropertyMetadata(null));

        public TextEditor Editor
        {
            get { return (TextEditor)GetValue(EditorProperty); }
            set { SetValue(EditorProperty, value); }
        }

        public static readonly DependencyProperty ShowReplaceDialogProperty =
            DependencyProperty.Register("ShowReplaceDialog",
                                        typeof(bool),
                                        typeof(FindReplaceDialog),
                                        new FrameworkPropertyMetadata(false));

        public bool ShowReplaceDialog
        {
            get { return (bool)GetValue(ShowReplaceDialogProperty); }
            set { SetValue(ShowReplaceDialogProperty, value); }
        }
        
        private string regexMem = "";
        private MatchCollection allMatches;
        private int matchIndex;


        public FindReplaceDialog()
        {
            InitializeComponent();
        }

        private void tbReplace_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ReplaceExec();
        }

        private void tbFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchNextExec();
        }

        private void SearchNextExec()
        {
            if (buttonRegex.IsChecked == true || buttonMatchWord.IsChecked == true)
                SearchPositionNextRegex();
            else
                 SearchPositionNext();
        }

        private void SearchPreviousExec()
        {
            if (String.IsNullOrEmpty(tbFind.Text))
                return;

            if (buttonRegex.IsChecked == true || buttonMatchWord.IsChecked == true)
                SearchPositionPreviousRegex();
            else
                SearchPositionPrevious();
        }

        private void ReplaceExec()
        {
            if (String.IsNullOrEmpty(tbFind.Text))
                return;

            if (buttonMatchWord.IsChecked == true || buttonRegex.IsChecked == true)
                ReplaceUsingRegex();
            else
                ReplaceNormally();
        }

        private void ReplaceAllExec()
        {
            Editor.Document.UndoStack.StartUndoGroup();

            if (buttonMatchWord.IsChecked == true || buttonRegex.IsChecked == true)
                ReplaceUsingRegex();
            else
                ReplaceNormally();

            while (Editor.SelectionLength > 0)
            {
                if (buttonMatchWord.IsChecked == true || buttonRegex.IsChecked == true)
                    ReplaceUsingRegex();
                else
                    ReplaceNormally();
            }

            Editor.Document.UndoStack.EndUndoGroup();
        }

        private void MatchCaseToggleExec()
        {
            buttonMatchCase.IsChecked = !buttonMatchCase.IsChecked;
        }

        private void MatchWordToggleExec()
        {
            buttonMatchWord.IsChecked = !buttonMatchWord.IsChecked;
        }

        private void RegexToggleExec()
        {
            buttonRegex.IsChecked = !buttonRegex.IsChecked;
        }

        private void DialogShownExec()
        {
            tbFind.Focus();

            if (Editor.SelectionLength == 0)
                tbFind.SelectAll();
            else
                tbFind.Text = Editor.SelectedText;

            tbFind.SelectAll();
        }

        private void SearchPositionNextRegex()
        {
            string regex = "";

            if (buttonMatchCase.IsChecked == true)
                regex = $@"\b{ tbFind.Text }\b";
            else
                regex = tbFind.Text;

            try
            {
                if (allMatches == null || regexMem != regex)
                {
                    allMatches = GetAllRegexMatches(regex);
                    regexMem = regex;
                }
            }
            catch (ArgumentException)
            {
                // This is thrown when an invalid regex string is provided
                // For example, a single backslash at the end
                // It doesn't matter, it just won't work, so ignore it
                matchIndex = -1;
            }

            matchIndex = GetNextMatchIndex(allMatches);

            if (matchIndex == -1)
                SelectTextForMatch(-1, 0);
            else
                SelectTextForMatch(allMatches[matchIndex].Index, allMatches[matchIndex].Value.Length);
        }

        private void SearchPositionPreviousRegex()
        {
            string regex = "";

            if (buttonMatchCase.IsChecked == true)
                regex = $@"\b{ tbFind.Text }\b";
            else
                regex = tbFind.Text;

            try
            {
                if (allMatches == null || regexMem != regex)
                {
                    allMatches = GetAllRegexMatches(regex);
                    regexMem = regex;
                }
            }
            catch (ArgumentException)
            {
                // This is thrown when an invalid regex string is provided
                // For example, a single backslash at the end
                // It doesn't matter, it just won't work, so ignore it
                matchIndex = -1;
            }

            matchIndex = GetPreviousMatchIndex(allMatches);

            if (matchIndex == -1)
                SelectTextForMatch(-1, 0);
            else
                SelectTextForMatch(allMatches[matchIndex].Index, allMatches[matchIndex].Value.Length);
        }

        private MatchCollection GetAllRegexMatches(string regex)
        {
            return Regex.Matches(Editor.Text, 
                                 regex,
                                 buttonMatchCase.IsChecked == true ? RegexOptions.None : RegexOptions.IgnoreCase);
        }

        private int GetNextMatchIndex(MatchCollection matches)
        {
            if (matches?.Count > 0)
            {
                for (int i = 0; i < matches.Count; i++)
                {
                    if (matches[i].Index > Editor.SelectionStart)
                        return i;
                }

                // Grab the first match again, basically wrap around to the first one again
                return 0;
            }

            return -1;
        }

        private int GetPreviousMatchIndex(MatchCollection matches)
        {
            if (matches?.Count > 0)
            {
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    if (matches[i].Index < Editor.SelectionStart)
                        return i;
                }

                // Grab the last match again, basically wrap around to the last one again
                return matches.Count - 1;
            }

            return -1;
        }

        private void SearchPositionNext()
        {
            int foundPosition = Editor.Text.IndexOf(tbFind.Text, 
                                                Editor.SelectionStart + Editor.SelectionLength,
                                                buttonMatchCase.IsChecked == true ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);

            if (foundPosition == -1)
                // Start again from the start
                foundPosition = Editor.Text.IndexOf(tbFind.Text,
                                                    buttonMatchCase.IsChecked == true ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);

            SelectTextForMatch(foundPosition, tbFind.Text.Length);
        }

        private void SearchPositionPrevious()
        {
            int foundPosition = Editor.Text.LastIndexOf(tbFind.Text,
                                                        Editor.SelectionStart,
                                                        buttonMatchCase.IsChecked == true ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);

            if (foundPosition == -1)
                // Start again from the end
                foundPosition = Editor.Text.LastIndexOf(tbFind.Text,
                                                        Editor.Text.Length - 1,
                                                        buttonMatchCase.IsChecked == true ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);

            SelectTextForMatch(foundPosition, tbFind.Text.Length);
        }

        private void SelectTextForMatch(int position, int length)
        {
            if (position != -1)
            {
                Editor.Select(position, length);
                Editor.ScrollToLine(Editor.TextArea.Caret.Line);
            }
            else // Unhighlight text when no matches are found
            {
                Editor.Select(Editor.SelectionStart, 0);
            }
        }

        private void ReplaceUsingRegex()
        {
            SearchPositionNextRegex();

            if (Editor.SelectionLength > 0)
            {
                string replaceText = Regex.Replace(Editor.SelectedText, tbFind.Text, tbReplace.Text);
                Editor.Document.Replace(Editor.SelectionStart, Editor.SelectionLength, replaceText, OffsetChangeMappingType.RemoveAndInsert);

                allMatches = null;
            }
        }

        private void ReplaceNormally()
        {
            if (Editor.SelectionLength == 0)
                SearchNextExec();

            if (Editor.SelectionLength > 0)
                Editor.Document.Replace(Editor.SelectionStart, Editor.SelectionLength, tbReplace.Text, OffsetChangeMappingType.KeepAnchorBeforeInsertion);

            SearchNextExec();
        }
    }
}
