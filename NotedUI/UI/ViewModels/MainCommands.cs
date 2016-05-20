﻿using JustMVVM;
using NotedUI.Models;
using NotedUI.UI.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace NotedUI.UI.ViewModels
{
    public class MainCommands : MVVMBase
    {
        public ICommand AddNoteCommand { get { return new RelayCommand<AddNoteParams>(AddNoteExec, CanAddNoteExec); } }
        public ICommand DeleteNoteCommand { get { return new RelayCommand<AllNotesViewModel>(DeleteNoteExec, CanDeleteNoteExec); } }

        public ICommand AddGroupCommand { get { return new RelayCommand<GroupCmdParams>(AddGroupExec, CanAddGroupExec); } }
        public ICommand RenameGroupCommand { get { return new RelayCommand<GroupCmdParams>(RenameGroupExec); } }
        public ICommand DeleteGroupCommand { get { return new RelayCommand<GroupCmdParams>(DeleteGroupExec, CanDeleteGroupExec); } }

        public ICommand ExportHTMLCommand { get { return new RelayCommand<AllNotesViewModel>(ExportHTMLExec, CanExportHTMLExec); } }
        public ICommand ExportPDFCommand { get { return new RelayCommand<AllNotesViewModel>(ExportPDFExec, CanExportPDFExec); } }
        public ICommand ShowSettingsCommand { get { return new RelayCommand<HomeViewModel>(ShowSettingsExec, CanShowSettingsExec); } }

        public ICommand ToggleFormattingCommand { get { return new RelayCommand<MainMenu>(ToggleFormattingExec); } }
        public ICommand TogglePreviewCommand { get { return new RelayCommand<MainMenu>(TogglePreviewExec); } }
        public ICommand ToggleSearchCommand { get { return new RelayCommand<MainMenu>(ToggleSearchExec); } }

        private bool _showPreview;
        public bool ShowPreview
        {
            get { return _showPreview; }
            set
            {
                _showPreview = value;
                OnPropertyChanged();
            }
        }

        public bool CanNotedExec(AllNotesViewModel allNotes)
        {
            return true;
        }

        public void NotedExec(AllNotesViewModel allNotes)
        {
            
        }

        public bool CanAddNoteExec(AddNoteParams cmdArgs)
        {
            return true;
        }

        public async void AddNoteExec(AddNoteParams cmdArgs)
        {
            int id = (int)await cmdArgs.AllNotes.LocalStorage.AddNote(cmdArgs.GroupName);

            var newNote = new NoteViewModel(id, DateTime.Now, "", cmdArgs.GroupName);
            (cmdArgs.AllNotes.View.SourceCollection as ObservableCollection<NoteViewModel>).Add(newNote);
            cmdArgs.AllNotes.SelectedNote = newNote;
        }

        public bool CanDeleteNoteExec(AllNotesViewModel allNotes)
        {
            return allNotes.SelectedNote != null;
        }

        public void DeleteNoteExec(AllNotesViewModel allNotes)
        {
            var noteList = allNotes.View.SourceCollection as ObservableCollection<NoteViewModel>;
            int noteIndex = noteList.IndexOf(allNotes.SelectedNote);

            allNotes.LocalStorage.DeleteNote(allNotes.SelectedNote.NoteData);
            (allNotes.View.SourceCollection as ObservableCollection<NoteViewModel>).Remove(allNotes.SelectedNote);

            if (noteList.Count == 0)
                return;

            allNotes.SelectedNote = noteList[noteIndex < noteList.Count ? noteIndex : noteIndex - 1];
        }

        public bool CanAddGroupExec(GroupCmdParams cmdArgs)
        {
            return true;
        }

        public void AddGroupExec(GroupCmdParams cmdArgs)
        {
            var dialog = new GroupNameDialogViewModel(new List<GroupViewModel>(cmdArgs.AllNotes.Groups), String.Empty, "Add New Group");

            dialog.DialogClosed += async () =>
            {
                if (dialog.Result == System.Windows.Forms.DialogResult.Cancel)
                    return;

                // Get ID, add group to local storage
                var id = await cmdArgs.AllNotes.LocalStorage.AddGroup(dialog.GroupName);
                cmdArgs.AllNotes.AddGroup(new GroupViewModel(id, dialog.GroupName));
            };

            cmdArgs.HomeVM.InvokeShowDialog(dialog);
        }

        private void RenameGroupExec(GroupCmdParams cmdArgs)
        {
            var dialog = new GroupNameDialogViewModel(new List<GroupViewModel>(cmdArgs.AllNotes.Groups), cmdArgs.GroupName, "Rename Group");

            dialog.DialogClosed += async () =>
            {
                if (dialog.Result == System.Windows.Forms.DialogResult.Cancel)
                    return;

                // Update the group name in the DB then update the notes in the VM
                await cmdArgs.AllNotes.LocalStorage.UpdateGroup(cmdArgs.GroupName, dialog.GroupName);

                // Update the notes with this group name
                foreach (var note in (cmdArgs.AllNotes.View.SourceCollection as ObservableCollection<NoteViewModel>))
                {
                    string oldGroupName = cmdArgs.GroupName.ToUpper();
                    if (note.Group.ToUpper() == oldGroupName)
                        note.Group = dialog.GroupName;
                }

                // Update the group name
                cmdArgs.AllNotes.UpdateGroup(cmdArgs.GroupName, dialog.GroupName);
            };

            cmdArgs.HomeVM.InvokeShowDialog(dialog);
        }

        private bool CanDeleteGroupExec(GroupCmdParams cmdArgs)
        {
            if (cmdArgs == null)
                return false;

            var notes = (cmdArgs?.AllNotes?.View?.SourceCollection
                            as ObservableCollection<NoteViewModel>).ToList()
                                                                   .Where(x => x?.Group?.ToUpper() == cmdArgs?.GroupName?.ToUpper());
            // Can only delete a group if it's empty!
            return notes?.Count() == 0;
        }

        private async void DeleteGroupExec(GroupCmdParams cmdArgs)
        {
            await cmdArgs.AllNotes.LocalStorage.DeleteGroup(cmdArgs.GroupName);

            cmdArgs.AllNotes.DeleteGroup(cmdArgs.GroupName);
        }

        public bool CanExportHTMLExec(AllNotesViewModel allNotes)
        {
            return true;
        }

        public void ExportHTMLExec(AllNotesViewModel allNotes)
        {
            //var html = CommonMarkConverter.Convert(_markdown);

            //HTMLExporter.Export(@"c:\github\notedui\htmltestexport.html", "github", html);
        }

        public bool CanExportPDFExec(AllNotesViewModel allNotes)
        {
            return true;
        }

        public void ExportPDFExec(AllNotesViewModel allNotes)
        {
            //var html = CommonMarkConverter.Convert(_markdown);

            //PDFExporter.Export(@"c:\github\notedui\pdftestexport.pdf", "github", html);
        }

        public bool CanShowSettingsExec(HomeViewModel homeVM)
        {
            return true;
        }

        public void ShowSettingsExec(HomeViewModel homeVM)
        {
            if (ShowPreview)
            {
                ShowPreview = false;
                homeVM.ShowPreviewOnLoad = true;
            }

            var settings = new SettingsViewModel(homeVM);
            homeVM.InvokeChangeScreen(settings);
        }

        private void ToggleFormattingExec(MainMenu menu)
        {
            menu.ShowFormatMenu = !menu.ShowFormatMenu;
        }

        private void TogglePreviewExec(MainMenu menu)
        {
            menu.ShowPreview = !menu.ShowPreview;
        }

        private void ToggleSearchExec(MainMenu menu)
        {
            menu.ShowSearch = !menu.ShowSearch;
        }
    }
}
