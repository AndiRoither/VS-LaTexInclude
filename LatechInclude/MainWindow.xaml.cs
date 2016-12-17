﻿
using LatechInclude.HelperClasses;
using LatechInclude.ViewModel;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Data;
using System.Diagnostics;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System;

namespace LatechInclude
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        TrulyObservableCollection<MyFile> _fileList = new TrulyObservableCollection<MyFile>();
        public delegate Point GetPosition(IInputElement element);
        int rowIndex = -1;

        private MainViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {

            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                MessageBoxResult result = MessageBox.Show("An Instance is already open, do you want to close it?", "Instance already open", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        var current = Process.GetCurrentProcess();
                        Process.GetProcessesByName(current.ProcessName).Where(t => t.Id != current.Id).ToList().ForEach(t => t.Kill());
                        break;
                    case MessageBoxResult.No:
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                        break;
                }   
            }            

            InitializeComponent();

            Closing += (s, e) => ViewModelLocator.Cleanup();

            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            MainView_DataGrid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(productsDataGrid_PreviewMouseLeftButtonDown);
            MainView_DataGrid.Drop += new System.Windows.DragEventHandler(MainView_DataGrid_Drop);
        }

        void MainView_DataGrid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            _fileList = _viewModel.List;

            if (rowIndex < 0)
                return;
            int index = this.GetCurrentRowIndex(e.GetPosition);
            if (index < 0)
                return;
            if (index == rowIndex)
                return;
            if (index == MainView_DataGrid.Items.Count - 1)
            {
                System.Windows.MessageBox.Show("This row-index cannot be dropped");
                return;
            }

            MyFile tempFile = MainView_DataGrid.SelectedItem as MyFile;
            _fileList.RemoveAt(rowIndex);
            _fileList.Insert(index, tempFile);

            int i = 1;
            foreach (MyFile file in _fileList)
            {
                file.Position = i;
                i++;
            }
            _viewModel.List = _fileList;
            MainView_DataGrid.ItemsSource = null;
            MainView_DataGrid.ItemsSource = _viewModel.List;
        }

        void productsDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            rowIndex = GetCurrentRowIndex(e.GetPosition);
            if (rowIndex < 0)
                return;

            MainView_DataGrid.SelectedIndex = rowIndex;
            MyFile tempFile = MainView_DataGrid.Items[rowIndex] as MyFile;

            if (tempFile == null)
                return;

            System.Windows.DragDropEffects dragdropeffects = System.Windows.DragDropEffects.Move;
            if (DragDrop.DoDragDrop(MainView_DataGrid, tempFile, dragdropeffects)
                                != System.Windows.DragDropEffects.None)
            {
                MainView_DataGrid.SelectedItem = tempFile;
            }
        }

        private bool GetMouseTargetRow(Visual theTarget, GetPosition position)
        {
            if (theTarget != null)
            {
                Rect rect = VisualTreeHelper.GetDescendantBounds(theTarget);
                Point point = position((IInputElement)theTarget);
                return rect.Contains(point);
            }
            else
            {
                return false;
            }
            
        }
        private DataGridRow GetRowItem(int index)
        {
            if (MainView_DataGrid.ItemContainerGenerator.Status
                    != GeneratorStatus.ContainersGenerated)
                return null;
            return MainView_DataGrid.ItemContainerGenerator.ContainerFromIndex(index)
                                                            as DataGridRow;
        }
        private int GetCurrentRowIndex(GetPosition pos)
        {
            int curIndex = -1;
            for (int i = 0; i < MainView_DataGrid.Items.Count; i++)
            {
                DataGridRow itm = GetRowItem(i);
                if (GetMouseTargetRow(itm, pos))
                {
                    curIndex = i;
                    break;
                }
            }
            return curIndex;
        }

        private void columnHeader_Click(object sender, RoutedEventArgs e)
        {
            var columnHeader = sender as DataGridColumnHeader;
            if(columnHeader != null)
            {
                int i;

                switch (columnHeader.Content.ToString())
                {
                    case "Name":

                        _fileList = _viewModel.List;
                        _fileList = new TrulyObservableCollection<MyFile>(from file in _fileList orderby file.FileName select file);

                        i = 1;
                        foreach (MyFile file in _fileList)
                        {
                            file.Position = i;
                            i++;
                        }

                        _viewModel.List = _fileList;
                        MainView_DataGrid.ItemsSource = null;
                        MainView_DataGrid.ItemsSource = _viewModel.List;

                        break;
                    case "Extension":
                        
                        _fileList = _viewModel.List;
                        _fileList = new TrulyObservableCollection<MyFile>(from file in _fileList orderby file.Extension select file);

                        i = 1;
                        foreach (MyFile file in _fileList)
                        {
                            file.Position = i;
                            i++;
                        }

                        _viewModel.List = _fileList;
                        MainView_DataGrid.ItemsSource = null;
                        MainView_DataGrid.ItemsSource = _viewModel.List;
                        break;
                }
            }
        }

        private void OnLanguageBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.currentLanguage = (sender as ComboBox).SelectedItem as string;
            WhiteList_Grid.ItemsSource = null;

            if (_viewModel.currentLanguage == "All")
            {
                _viewModel.currentWhiteList = _viewModel.whiteList;
            }
            else
            {
                List<WhiteList> tempList = new List<WhiteList>();

                foreach (WhiteList wl in _viewModel.whiteList)
                {
                    if (wl.Language == _viewModel.currentLanguage)
                    {
                        tempList.Add(new WhiteList
                        {
                            Language = wl.Language,
                            Extension = wl.Extension
                        });
                    }
                }

                _viewModel.currentWhiteList = tempList;
            }
            WhiteList_Grid.ItemsSource = _viewModel.currentWhiteList;
        }

        private void OnMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<WhiteList> tempWList = _viewModel.whiteList;
            string outputString = "";
            string compareLanguage = "";

            foreach(WhiteList wl in tempWList)
            {
                if (compareLanguage != wl.Language)
                {
                    compareLanguage = wl.Language;
                    outputString += "#" + wl.Language;
                    outputString += Environment.NewLine;
                    outputString += wl.Extension;
                    
                }
                else
                {
                    outputString += wl.Extension;
                }
                outputString += Environment.NewLine;
            }

            try
            {
                System.IO.File.WriteAllText((System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\Resources\\WhiteList.txt"), outputString);
            }
            catch(Exception ex)
            {
                outputString = "";
                outputString = ex.ToString();
                System.IO.File.WriteAllText((System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\CrashLog.txt"), outputString);
                outputString = null;
            }
            
        }
    }
}