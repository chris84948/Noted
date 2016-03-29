﻿using NotedUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NotedUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NotedWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            //List<Group> items = new List<Group>()
            //{
            //    new Group()
            //    {
            //        Name = "GROUP 1",
            //        Notes = new List<Note>()
            //        {
            //            new Note() { Title = "Note 1", LastModified = DateTime.Now },
            //            new Note() { Title = "Note 2", LastModified = DateTime.Now.Subtract(TimeSpan.FromDays(1)) },
            //            new Note() { Title = "Note 3", LastModified = DateTime.Now.Subtract(TimeSpan.FromMinutes(34)) },
            //        }
            //    },
            //    new Group()
            //    {
            //        Name = "GROUP 2",
            //        Notes = new List<Note>()
            //        {
            //            new Note() { Title = "Note 4", LastModified = DateTime.Now },
            //            new Note() { Title = "Note 4", LastModified = DateTime.Now },
            //            new Note() { Title = "Note 4", LastModified = DateTime.Now },
            //            new Note() { Title = "Note 4", LastModified = DateTime.Now },
            //            new Note() { Title = "Note 4", LastModified = DateTime.Now },
            //            new Note() { Title = "Note 4", LastModified = DateTime.Now },
            //            new Note() { Title = "Note 4", LastModified = DateTime.Now },
            //            new Note() { Title = "Note 4", LastModified = DateTime.Now },
            //            new Note() { Title = "Note 4", LastModified = DateTime.Now },
            //            new Note() { Title = "Note 5", LastModified = DateTime.Now.Subtract(TimeSpan.FromDays(14)) },
            //            new Note() { Title = "Note 6", LastModified = DateTime.Now.Subtract(TimeSpan.FromHours(7)) },
            //        }
            //    }
            //};

            //lvNotes.ItemsSource = items;

            List<Note> notes = new List<Note>()
            {
                new Note() { Group = "GROUP 1", Title = "Note 1", LastModified = DateTime.Now },
                new Note() { Group = "GROUP 1", Title = "Note 2", LastModified = DateTime.Now.Subtract(TimeSpan.FromDays(1)) },
                new Note() { Group = "GROUP 2", Title = "Note 3", LastModified = DateTime.Now.Subtract(TimeSpan.FromMinutes(34)) },
                new Note() { Group = "GROUP 2", Title = "Note 4", LastModified = DateTime.Now },
                new Note() { Group = "GROUP 2", Title = "Note 5", LastModified = DateTime.Now.Subtract(TimeSpan.FromDays(14)) },
                new Note() { Group = "GROUP 2", Title = "Note 6", LastModified = DateTime.Now.Subtract(TimeSpan.FromHours(7)) }
            };

            lvNotes2.ItemsSource = notes;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvNotes2.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Group");
            view.GroupDescriptions.Add(groupDescription);
        }
    }

    public class Group
    {
        public string Name { get; set; }
        public List<Note> Notes { get; set; }
    }

    public class Note
    {
        public string Group { get; set; }
        public string Title { get; set; }
        public DateTime LastModified { get; set;}
    }
}
