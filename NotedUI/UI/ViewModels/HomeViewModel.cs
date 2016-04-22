﻿using System;
using JustMVVM;
using System.Collections.ObjectModel;
using NotedUI.UI.Screens;
using NotedUI.Controls;
using System.Collections.Generic;
using NotedUI.Models;
using System.Linq;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.ComponentModel;
using System.IO;

namespace NotedUI.UI.ViewModels
{
    public class HomeViewModel : MVVMBase, IScreen, IDropTarget
    {
        public event Action<IScreen, eTransitionType> ChangeScreen;

        public FormatCommands Formatting { get; set; }
        public MainCommands MainCommands { get; set; }
        public AllNotesViewModel AllNotes { get; set; }
        public string CSSStyle { get; set; }

        public bool ShowPreviewOnLoad { get; set; }

        // TODO Don't think I'll need this once the list of notes comes in, but for now it works
        private string _noteText;
        public string NoteText
        {
            get { return _noteText; }
            set
            {
                _noteText = value;
                OnPropertyChanged();
            }
        }

        // TODO Delete this when done testing
        public IEnumerable<eNoteState> NoteStates
        {
            get
            {
                return Enum.GetValues(typeof(eNoteState))
                    .Cast<eNoteState>();
            }
        }

        public HomeViewModel()
        {
            Formatting = new FormatCommands();
            MainCommands = new MainCommands();
            AllNotes = new AllNotesViewModel();
            CSSStyle = File.ReadAllText("CSS\\Github.css");
        }

        public void InvokeChangeScreen(IScreen screen)
        {
            ChangeScreen?.Invoke(screen, eTransitionType.SlideInFromLeft);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            NoteViewModel sourceItem = dropInfo.Data as NoteViewModel;

            if (sourceItem != null && dropInfo.TargetGroup != null)
            {
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            NoteViewModel sourceItem = dropInfo.Data as NoteViewModel;
            NoteViewModel targetItem = dropInfo.TargetItem as NoteViewModel;

            sourceItem.Folder = dropInfo.TargetGroup.Name.ToString();

            // Changing group data at runtime isn't handled well: force a refresh on the collection view.
            if (dropInfo.TargetCollection is IEditableCollectionView)
            {
                ((IEditableCollectionView)dropInfo.TargetCollection).EditItem(sourceItem);
                ((IEditableCollectionView)dropInfo.TargetCollection).CommitEdit();
            }
        }
    }
}
