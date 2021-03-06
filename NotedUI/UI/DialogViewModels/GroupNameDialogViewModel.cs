﻿using JustMVVM;
using NotedUI.Models;
using NotedUI.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace NotedUI.UI.DialogViewModels
{
    public class GroupNameDialogViewModel : MVVMBase, IDialog
    {
        public event Action<IDialog> DialogClosed;
        public ICommand OKCommand { get { return new RelayCommand(OKExec, CanOKExec); } }
        public ICommand CancelCommand { get { return new RelayCommand(CancelExec); } }

        public System.Windows.Forms.DialogResult Result { get; set; }

        private List<string> _allGroups;

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _groupName;
        public string GroupName
        {
            get { return _groupName; }
            set
            {
                _groupName = value;
                OnPropertyChanged();
            }
        }

        private bool _showWarning;
        public bool ShowWarning
        {
            get { return _showWarning; }
            set
            {
                _showWarning = value;
                OnPropertyChanged();
            }
        }
        
        public GroupNameDialogViewModel(GroupCollection allGroups, string groupName, string dialogTitle)
        {
            _allGroups = allGroups.Groups.Select(x => x.Name).ToList();
            GroupName = groupName;
            Title = dialogTitle;
        }

        private bool CanOKExec()
        {
            ShowWarning = _allGroups.Contains(GroupName.ToUpper());

            return !String.IsNullOrWhiteSpace(GroupName) && !ShowWarning;
        }

        private void OKExec()
        {
            Result = System.Windows.Forms.DialogResult.OK;
            DialogClosed?.Invoke(this);
        }

        private void CancelExec()
        {
            Result = System.Windows.Forms.DialogResult.Cancel;
            DialogClosed?.Invoke(this);
        }
    }
}
