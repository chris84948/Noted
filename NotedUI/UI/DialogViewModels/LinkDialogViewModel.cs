﻿using JustMVVM;
using NotedUI.UI.ViewModels;
using System;
using System.Windows.Input;

namespace NotedUI.UI.DialogViewModels
{
    public class LinkDialogViewModel : MVVMBase, IDialog
    {
        public event Action<IDialog> DialogClosed;
        public ICommand OKCommand { get { return new RelayCommand(OKExec); } }
        public ICommand CancelCommand { get { return new RelayCommand(CancelExec); } }

        public System.Windows.Forms.DialogResult Result { get; set; }

        private string _link;
        public string Link
        {
            get { return _link; }
            set
            {
                _link = value;
                OnPropertyChanged();
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public LinkDialogViewModel(string description)
        {
            Description = description;
            Link = "https://";
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
