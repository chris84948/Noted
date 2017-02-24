﻿using JustMVVM;
using NotedUI.DataStorage;
using NotedUI.Export;
using NotedUI.Models;
using NotedUI.UI.Components;
using NotedUI.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NotedUI.UI.ViewModels
{
    public class MainCommands : MVVMBase
    {
        public ICommand AddNoteCommand { get { return new RelayCommand<string>(AddNoteExec); } }
        public ICommand DeleteNoteCommand { get { return new RelayCommand<NoteViewModel>(DeleteNoteExec); } }

        public ICommand AddGroupCommand { get { return new RelayCommand(AddGroupExec); } }
        public ICommand RenameGroupCommand { get { return new RelayCommand<string>(RenameGroupExec); } }
        public ICommand DeleteGroupCommand { get { return new RelayCommand<string>(DeleteGroupExec); } }

        public ICommand ExportTextCommand { get { return new RelayCommand(ExportTextExec); } }
        public ICommand ExportHTMLCommand { get { return new RelayCommand(ExportHTMLExec); } }
        public ICommand ExportPDFCommand { get { return new RelayCommand(ExportPDFExec); } }
        public ICommand ExportDocCommand { get { return new RelayCommand(ExportDocExec); } }
        public ICommand ShowSettingsCommand { get { return new RelayCommand(ShowSettingsExec); } }

        public ICommand ToggleFormattingCommand { get { return new RelayCommand(ToggleFormattingExec); } }
        public ICommand ToggleSearchCommand { get { return new RelayCommand(ToggleSearchExec); } }

        private bool _showFormatting;
        public bool ShowFormatting
        {
            get { return _showFormatting; }
            set
            {
                _showFormatting = value;
                OnPropertyChanged();
            }
        }

        private bool _showSearch;
        public bool ShowSearch
        {
            get { return _showSearch; }
            set
            {
                _showSearch = value;
                OnPropertyChanged();
            }
        }

        // This only exists so that the Markdown isn't always updating
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

        private HomeViewModel _homeVM;
        private AllNotesViewModel _allNotesVM;
        private ILocalStorage _localStorage;

        public MainCommands(HomeViewModel homeVM, AllNotesViewModel allNotesVM, ILocalStorage localStorage)
        {
            _homeVM = homeVM;
            _allNotesVM = allNotesVM;
            _localStorage = localStorage;
        }
        
        public async void AddNoteExec(string groupName)
        {
            // Little complicated here
            // First add the note to local storage to get the ID
            int id = (int)await _allNotesVM.LocalStorage.AddNote(groupName);
            var newNote = new NoteViewModel(id, DateTime.Now, "", groupName);

            // Add the note to the screen now so there's no delay
            (_allNotesVM.View.SourceCollection as ObservableCollection<NoteViewModel>).Add(newNote);
            _allNotesVM.SelectedNote = newNote;

            await Task.Run(async () =>
            {
                // Finally add to the cloud, update the local DB with the cloud key and modified date and update the SelectedNote
                await _allNotesVM.CloudStorage.AddNote(_allNotesVM.SelectedNote.NoteData);
                await _allNotesVM.LocalStorage.UpdateNote(_allNotesVM.SelectedNote.NoteData);
            });
        }

        public async void DeleteNoteExec(NoteViewModel noteToDelete)
        {
            var noteList = _allNotesVM.View.SourceCollection as ObservableCollection<NoteViewModel>;
            noteList.Remove(noteToDelete);

            await _allNotesVM.LocalStorage.DeleteNote(noteToDelete.NoteData);
            await _allNotesVM.CloudStorage.DeleteNote(noteToDelete.NoteData);

            if (noteList.Count == 0)
                return;

            // TODO For now, just unselect all notes when it's deleted
            _allNotesVM.SelectedNote = null;
        }

        public void AddGroupExec()
        {
            var dialog = new GroupNameDialogViewModel(_allNotesVM.AllGroups, String.Empty, "Add New Group");

            dialog.DialogClosed += async () =>
            {
                await Task.Delay(300);
                _homeVM.FixAirspace = false;

                if (dialog.Result == System.Windows.Forms.DialogResult.Cancel)
                    return;

                // Get ID, add group to local storage
                var id = await _allNotesVM.LocalStorage.AddGroup(dialog.GroupName);
                _allNotesVM.AddGroup(new GroupViewModel(id, dialog.GroupName));
            };

            _homeVM.InvokeShowDialog(dialog);
        }

        private void RenameGroupExec(string groupName)
        {
            var dialog = new GroupNameDialogViewModel(_allNotesVM.AllGroups, groupName, "Rename Group");

            dialog.DialogClosed += async () =>
            {
                await Task.Delay(300);
                _homeVM.FixAirspace = false;

                if (dialog.Result == System.Windows.Forms.DialogResult.Cancel)
                    return;

                // Update the group name in the DB then update the notes in the VM
                await _allNotesVM.LocalStorage.UpdateGroup(groupName, dialog.GroupName);

                // Update the notes with this group name
                foreach (var note in (_allNotesVM.View.SourceCollection as ObservableCollection<NoteViewModel>))
                {
                    string oldGroupName = groupName.ToUpper();
                    if (note.Group.ToUpper() == oldGroupName)
                        note.Group = dialog.GroupName;
                }

                // Update the group name
                _allNotesVM.UpdateGroup(groupName, dialog.GroupName);
            };

            _homeVM.InvokeShowDialog(dialog);
        }

        private bool CanDeleteGroupExec(string groupName)
        {
            if (String.IsNullOrWhiteSpace(groupName))
                return false;

            var notes = (_allNotesVM.View?.SourceCollection
                            as ObservableCollection<NoteViewModel>).ToList()
                                                                   .Where(x => x?.Group?.ToUpper() == groupName.ToUpper());
            // Can only delete a group if it's empty!
            return notes?.Count() == 0;
        }

        private async void DeleteGroupExec(string groupName)
        {
            await _allNotesVM.LocalStorage.DeleteGroup(groupName);

            _allNotesVM.DeleteGroup(groupName);
        }
        
        public async void ExportTextExec()
        {
            var dialog = new FileSaveDialogViewModel("TEXT", "txt", await _localStorage.GetLastExportedPath("Txt"));

            dialog.DialogClosed += async () =>
            {
                if (dialog.Result == System.Windows.Forms.DialogResult.OK)
                {
                    TextExporter.Export(dialog.ResultFilename, _allNotesVM.SelectedNote.Content);
                    await _localStorage.InsertOrUpdateLastExportedPath("Txt", dialog.ResultFilename);
                }
            };

            _homeVM.InvokeShowDialog(dialog);
        }

        public async void ExportHTMLExec()
        {
            var dialog = new FileSaveDialogViewModel("html", await _localStorage.GetLastExportedPath("Html"));

            dialog.DialogClosed += async () =>
            {
                if (dialog.Result == System.Windows.Forms.DialogResult.OK)
                {
                    HTMLExporter.Export(dialog.ResultFilename, "github", MarkdownParser.Parse(_allNotesVM.SelectedNote.Content));
                    await _localStorage.InsertOrUpdateLastExportedPath("Txt", dialog.ResultFilename);
                }
            };

            _homeVM.InvokeShowDialog(dialog);
        }

        public async void ExportPDFExec()
        {
            var dialog = new FileSaveDialogViewModel("pdf", await _localStorage.GetLastExportedPath("Pdf"));

            dialog.DialogClosed += async () =>
            {
                if (dialog.Result == System.Windows.Forms.DialogResult.OK)
                {
                    PDFExporter.Export(dialog.ResultFilename, "github", MarkdownParser.Parse(_allNotesVM.SelectedNote.Content));
                    await _localStorage.InsertOrUpdateLastExportedPath("Txt", dialog.ResultFilename);
                }
            };

            _homeVM.InvokeShowDialog(dialog);
        }

        public async void ExportDocExec()
        {
            var dialog = new FileSaveDialogViewModel("doc", await _localStorage.GetLastExportedPath("Doc"));

            dialog.DialogClosed += async () =>
            {
                if (dialog.Result == System.Windows.Forms.DialogResult.OK)
                {
                    HTMLExporter.Export(dialog.ResultFilename, "github", MarkdownParser.Parse(_allNotesVM.SelectedNote.Content));
                    await _localStorage.InsertOrUpdateLastExportedPath("Txt", dialog.ResultFilename);
                }
            };

            _homeVM.InvokeShowDialog(dialog);
        }

        public void ShowSettingsExec()
        {
            _homeVM.FixAirspace = true;

            var settings = new SettingsViewModel(_homeVM);
            _homeVM.InvokeChangeScreen(settings);
        }

        private void ToggleFormattingExec()
        {
            ShowFormatting = !ShowFormatting;
        }

        private void ToggleSearchExec()
        {
            ShowSearch = !ShowSearch;
        }
    }
}