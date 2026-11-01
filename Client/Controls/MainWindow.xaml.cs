using mshtml;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TrayMainWindows _tray;
        HotKeyMainWindows _hotkey;
        WindowLocation _previousWindowLocation;

        public MainWindow()
        {
            Application.Current.DispatcherUnhandledException += (o, e) =>
            {
                    e.Handled = true;
					e.Exception.Handle("An unexpected error occurred");
            };

            InitializeComponent();

            CreateCommands();

            SetupTrayIcon();

			if (User.Default.CheckForUpdates) CheckForUpdates();

			webBrowser1.Navigate("about:blank");

            SetFont();

			SetWindowPosition();

			lbTasks.Focus();
		}

        private void CreateCommands()
        {
            // Sind ursprünglich im XAML definiert, werden aber hier im Code-Behind registriert, damit sie auch über Ribbon funktionieren
            //File Menu CommandBindings erstellen
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("OpenFile"), OpenFileExecuted, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("NewFile"), NewFileExecuted, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("PrintFile"), PrintFileExecuted, WhenTasksLoadedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("PrintPreviewFile"), PrintPreviewFileExecuted, WhenTasksLoadedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ArchiveCompletedTasks"), ArchiveCompletedTasksExecuted, WhenTasksLoadedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ReloadFile"), ReloadFileExecuted, WhenTasksLoadedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("Options"), OptionsExecuted, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ExitApplication"), ExitApplicationExecuted, AlwaysCanExecute));
            //Edit MenuCommandBindings erstellen
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("CopySelectedTaskToNewTask"), CopySelectedTaskToNewTaskExecuted, WhenSingleTaskSelectedCanExecute));
            //Task menu CommandBindings erstellen
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("NewTask"), NewTaskExecuted, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("NewTaskWithPriority"), NewTaskWithPriorityExecuted, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("UpdateTask"), UpdateTaskExecuted, WhenSingleTaskSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("AppendText"), AppendTextExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("DeleteTask"), DeleteTaskExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ToggleCompletion"), ToggleCompletionExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("SetDueDate"), SetDueDateExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("PostponeTask"), PostponeTaskExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("SetThresholdDate"), SetThresholdDateExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ThresholdTask"), ThresholdTaskExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("SetPriority"), SetPriorityExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("IncreasePriority"), IncreasePriorityExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("DecreasePriority"), DecreasePriorityExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("RemovePriority"), RemovePriorityExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("IncrementDueDate"), IncrementDueDateExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("DecrementDueDate"), DecrementDueDateExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("RemoveDueDate"), RemoveDueDateExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("IncrementThresholdDate"), IncrementThresholdDateExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("DecrementThresholdDate"), DecrementThresholdDateExecuted, WhenTasksSelectedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("RemoveThresholdDate"), RemoveThresholdDateExecuted, WhenTasksSelectedCanExecute));
            //Sort menu commandBindings erstellen
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("SortByAlphabetical"), SortByAlphabeticalExecuted, WhenTasksLoadedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("SortByCompleted"), SortByCompletedExecuted, WhenTasksLoadedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("SortByContext"), SortByContextExecuted, WhenTasksLoadedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("SortByDueDate"), SortByDueDateExecuted, WhenTasksLoadedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("SortByCreatedDate"), SortByCreatedDateExecuted, WhenTasksLoadedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("SortByPriority"), SortByPriorityExecuted, WhenTasksLoadedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("SortByProject"), SortByProjectExecuted, WhenTasksLoadedCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("SortByFileOrder"), SortByFileOrderExecuted, WhenTasksLoadedCanExecute));
            // Filter menu commandBindings erstellen
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("DefineFilters"), DefineFiltersExecuted, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ApplyFilterFutureTasks"), ApplyFilterFutureTasksExecuted, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ApplyShowHiddenTasks"), ApplyShowHiddenTasksExecuted, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("RemoveFilter"), RemoveFilterExecuted, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ApplyFilterPreset1"), ApplyFilterPreset1Executed, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ApplyFilterPreset2"), ApplyFilterPreset1Executed, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ApplyFilterPreset3"), ApplyFilterPreset1Executed, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ApplyFilterPreset4"), ApplyFilterPreset1Executed, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ApplyFilterPreset5"), ApplyFilterPreset1Executed, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ApplyFilterPreset6"), ApplyFilterPreset1Executed, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ApplyFilterPreset7"), ApplyFilterPreset1Executed, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ApplyFilterPreset8"), ApplyFilterPreset1Executed, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ApplyFilterPreset9"), ApplyFilterPreset1Executed, AlwaysCanExecute));
            // Help menu commandBindings erstellen
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("HelpAbout"), HelpAboutExecuted, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ViewLog"), ViewLogExecuted, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("Donate"), DonateExecuted, AlwaysCanExecute));
            CommandBindings.Add(new CommandBinding((ICommand)FindResource("ShowCalendar"), ShowCalendarExecuted, AlwaysCanExecute));
            //Task List CommandBindings erstellen
            // CommandBindings.Add(new CommandBinding((ICommand)FindResource("EmulateUpArrow"), EmulateUpArrowExecuted, WhenTasksLoadedCanExecute));
            /// CommandBindings.Add(new CommandBinding((ICommand)FindResource("EmulateDownArrowExecuted"), DonateExecuted, WhenTasksLoadedCanExecute));
        }

        public MainWindowViewModel ViewModel { get; set; }

		private void CheckForUpdates()
                {
			var updateChecker = new UpdateChecker(this);
			updateChecker.BeginCheck();
                }

		private void SetupTrayIcon()
		{
			if (User.Default.MinimiseToSystemTray)
			{
				_tray = new TrayMainWindows(this);
				_hotkey = new HotKeyMainWindows(this, ModifierKeys.Control | ModifierKeys.Alt, System.Windows.Forms.Keys.M);
			}
		}

		private void SetWindowPosition()
            {
			Height = User.Default.WindowHeight;
			Width = User.Default.WindowWidth;
			Left = User.Default.WindowLeft;
			Top = User.Default.WindowTop;
        }

        protected override void OnClosed(EventArgs e)
        {
			if (ViewModel.HelpPage != null)
				ViewModel.HelpPage.Close();

            base.OnClosed(e);
        }

        /// <summary>
        /// Helper function that converts the values stored in the settings into the font values
        /// and then sets the tasklist font values.
        /// </summary>
		public void SetFont()
        {
            var family = new FontFamily(User.Default.TaskListFontFamily);

            double size = User.Default.TaskListFontSize;

            var styleConverter = new FontStyleConverter();

            FontStyle style = (FontStyle)styleConverter.ConvertFromString(User.Default.TaskListFontStyle);

            var stretchConverter = new FontStretchConverter();
            FontStretch stretch = (FontStretch)stretchConverter.ConvertFromString(User.Default.TaskListFontStretch);

            var weightConverter = new FontWeightConverter();
            FontWeight weight = (FontWeight)weightConverter.ConvertFromString(User.Default.TaskListFontWeight);

            Color color = (Color)ColorConverter.ConvertFromString(User.Default.TaskListFontBrushColor);

			lbTasks.FontFamily = family;
			lbTasks.FontSize = size;
			lbTasks.FontStyle = style;
			lbTasks.FontStretch = stretch;
			lbTasks.FontWeight = weight;
			lbTasks.Foreground = new SolidColorBrush(color);
        }

        void SetSelected(MenuItem item)
        {
            var sortMenu = (MenuItem)item.Parent;
            foreach (MenuItem i in sortMenu.Items)
                i.IsChecked = false;

            item.IsChecked = true;

        }

        private void Filter(object sender, RoutedEventArgs e)
        {
			ViewModel.ShowFilterDialog();
        }

		public void ToggleUpdateMenu(string version)
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (!string.IsNullOrEmpty(version) && version != assemblyVersion)
            {
				UpdateMenu.Header = "New version: " + version;
				UpdateMenu.Visibility = Visibility.Visible;
            }
        }

		private void Window_Loaded(object sender, RoutedEventArgs e)
        {
			ViewModel = new MainWindowViewModel(this);
			DataContext = ViewModel;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Application.Current.ShutdownMode == ShutdownMode.OnExplicitShutdown)
            {
                return;
            }

            if (User.Default.MinimiseToSystemTray && User.Default.MinimiseOnClose)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
            }
        }

		#region window location handlers
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            _previousWindowLocation = new WindowLocation();

            if (Left >= 0 && Top >= 0)
            {
				User.Default.WindowLeft = Left;
				User.Default.WindowTop = Top;
                User.Default.Save();
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Maximized)
            {
                User.Default.WindowLeft = _previousWindowLocation.Left;
                User.Default.WindowTop = _previousWindowLocation.Top;
                User.Default.WindowHeight = _previousWindowLocation.Height;
                User.Default.WindowWidth = _previousWindowLocation.Width;
                User.Default.Save();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 0 && e.NewSize.Width > 0 && WindowState != System.Windows.WindowState.Maximized)
            {
                User.Default.WindowHeight = e.NewSize.Height;
                User.Default.WindowWidth = e.NewSize.Width;
                User.Default.Save();
            }
        }
        #endregion

        #region command CanExecute methods

        private void WhenTasksSelectedCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lbTasks.SelectedItems.Count > 0) && (!taskText.IsFocused);
            e.Handled = true;
        }

        private void WhenSingleTaskSelectedCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lbTasks.SelectedItems.Count == 1) && (!taskText.IsFocused);
            e.Handled = true;
        }

        private void WhenTasksLoadedCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lbTasks.Items.Count > 0);
            e.Handled = true;
        }

        private void AlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void PasteTasksIntoTaskListCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Clipboard.ContainsText());
            e.Handled = true;
        }

        #endregion

        #region file menu

        private void OpenFileExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenFile();
        }

        public void NewFileExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.NewFile();
        }

                private void PrintPreviewFileExecuted(object sender, RoutedEventArgs e)
        {
            string printContents;
			printContents = ViewModel.GetPrintContents();

            mshtml.IHTMLDocument2 doc = webBrowser1.Document as mshtml.IHTMLDocument2;
            doc.clear();
            doc.write(printContents);
            doc.close();

			ViewModel.SetPrintControlsVisibility(true);
        }

        private void PrintFileExecuted(object sender, RoutedEventArgs e)
        {
            string printContents;
			printContents = ViewModel.GetPrintContents();

            mshtml.IHTMLDocument2 doc = webBrowser1.Document as mshtml.IHTMLDocument2;
            doc.clear();
            doc.write(printContents);
            doc.execCommand("Print", true, 0);
            doc.close();
        }
		
        private void ArchiveCompletedTasksExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.ArchiveCompleted();
        }

        public void ReloadFileExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.ReloadFile();
        }

        public void OptionsExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowOptionsDialog();
        }

        public void ExitApplicationExecuted(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Edit Menu

        private void CopyTasksExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.CopySelectedTasksToClipboard();
        }

        private void CopySelectedTaskToNewTaskExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.CopySelectedTaskToTextBox();
        }

        private void PasteTasksIntoTaskListExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.PasteTasksIntoTaskList();
        }

        #endregion

        #region task menu

        private void NewTaskExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.AddNewTask();
        }

        private void NewTaskWithPriorityExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.AddNewTaskWithPriority();
        }

        private void UpdateTaskExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateTask();
        }

        private void AppendTextExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.AppendText();
        }

        private void DeleteTaskExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.DeleteTasks();
        }

        private void ToggleCompletionExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.ToggleCompletion();
        }

        private void SetPriorityExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.SetPriority();
        }

        private void IncreasePriorityExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.IncreasePriority();
        }

        private void DecreasePriorityExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.DecreasePriority();
        }

        private void RemovePriorityExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.RemovePriority();
        }

        private void SetDueDateExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.SetDueDate();
        }

        private void PostponeTaskExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.Postpone();
        }

        private void IncrementDueDateExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.IncrementDueDate();
        }

        private void DecrementDueDateExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.DecrementDueDate();
        }

        private void RemoveDueDateExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveDueDate();
        }

        private void SetThresholdDateExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.SetThresholdDate();
        }

        private void ThresholdTaskExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.Threshold();
        }

        private void IncrementThresholdDateExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.IncrementThresholdDate();
        }

        private void DecrementThresholdDateExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.DecrementThresholdDate();
        }

        private void RemoveThresholdDateExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveThresholdDate();
        }

        #endregion

        #region sort menu

        public void SetSelectedMenuItem(MenuItem menu, string selectedMenuItemTag)
        {
            foreach (var item in menu.Items)
            {
                if (item is MenuItem)
                {
                    MenuItem menuItem = (MenuItem)item;
                    menuItem.IsChecked = selectedMenuItemTag.Equals(menuItem.Tag);
                }
            }
        }

        public void SetSelectionOfMenuItem(string selectedMenuItemTag, bool isChecked)
        {
            foreach (var item in filterMenu.Items)
            {
                if (item is MenuItem)
                {
                    MenuItem menuItem = (MenuItem)item;
                    if (selectedMenuItemTag.Equals(menuItem.Tag))
                    {
                        menuItem.IsChecked = isChecked;
                        break;
                    }
                }
            }
        }

        private void SortByFileOrderExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.SortList(SortType.None);            
        }

        private void SortByContextExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.SortList(SortType.Context);
        }

        private void SortByCompletedExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.SortList(SortType.Completed);
        }

        private void SortByDueDateExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.SortList(SortType.DueDate);
        }

        private void SortByProjectExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.SortList(SortType.Project);
        }

        private void SortByAlphabeticalExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.SortList(SortType.Alphabetical);
        }

        public void SortByCreatedDateExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.SortList(SortType.Created);        
        }

        private void SortByPriorityExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.SortList(SortType.Priority);       
        }

        #endregion

        #region filter menu

        private void DefineFiltersExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowFilterDialog();
        }

        private void RemoveFilterExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyFilterPreset0();
            SetSelectedMenuItem(filterMenu, "None");
        }

        private void ApplyFilterFutureTasksExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyHideFutureTasks();
            SetSelectionOfMenuItem("FutureTasks", User.Default.FilterFutureTasks);
        }     

        private void ApplyFilterPreset1Executed(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyFilterPreset1();
            SetSelectedMenuItem(filterMenu, "Preset1");
        }

        private void ApplyFilterPreset2Executed(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyFilterPreset2();
            SetSelectedMenuItem(filterMenu, "Preset2");
        }

        private void ApplyFilterPreset3Executed(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyFilterPreset3();
            SetSelectedMenuItem(filterMenu, "Preset3");
        }

        private void ApplyFilterPreset4Executed(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyFilterPreset4();
            SetSelectedMenuItem(filterMenu, "Preset4");
        }

        private void ApplyFilterPreset5Executed(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyFilterPreset5();
            SetSelectedMenuItem(filterMenu, "Preset5");
        }
        
        private void ApplyFilterPreset6Executed(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyFilterPreset6();
            SetSelectedMenuItem(filterMenu, "Preset6");
        }
        
        private void ApplyFilterPreset7Executed(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyFilterPreset7();
            SetSelectedMenuItem(filterMenu, "Preset7");
        }
        
        private void ApplyFilterPreset8Executed(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyFilterPreset8();
            SetSelectedMenuItem(filterMenu, "Preset8");
        }
        
        private void ApplyFilterPreset9Executed(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyFilterPreset9();
            SetSelectedMenuItem(filterMenu, "Preset9");
        }

        private void ApplyShowHiddenTasksExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyShowHiddenTasks();
            SetSelectionOfMenuItem("HiddenTasks", User.Default.ShowHidenTasks);
        }

    #endregion

    #region help menu

    private void HelpAboutExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowHelpDialog();
        }

        private void ViewLogExecuted(object sender, RoutedEventArgs e)
        {
			ViewModel.ViewLog();
        }

        private void DonateExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.Donate();
        }

        private void ShowCalendarExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.AddCalendarToTitle();
        }
        
        #endregion

		#region update menu

		private void GetUpdate(object sender, RoutedEventArgs e)
        {
			try 
			{
				Process.Start(UpdateChecker.updateClientUrl);
			}
			catch (Exception ex)
			{
				ex.Handle("Error while launching " + UpdateChecker.updateClientUrl);
        }
		}

        #endregion

        #region lbTasks

        private void EmulateDownArrowExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.EmulateDownArrow();
        }

        private void EmulateUpArrowExecuted(object sender, RoutedEventArgs e)
        {
            ViewModel.EmulateUpArrow();
        }

        #endregion

		#region printing

		private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
			mshtml.IHTMLDocument2 doc = webBrowser1.Document as mshtml.IHTMLDocument2;
			doc.execCommand("Print", true, 0);
			doc.close();

			ViewModel.SetPrintControlsVisibility(false);
        }

		private void btnCancelPrint_Click(object sender, RoutedEventArgs e)
        {
			ViewModel.SetPrintControlsVisibility(false);
                lbTasks.Focus();
            }


        private void File_PrintPreview(object sender, RoutedEventArgs e)
        {
            string printContents;
			printContents = ViewModel.GetPrintContents();

            mshtml.IHTMLDocument2 doc = webBrowser1.Document as mshtml.IHTMLDocument2;
            doc.clear();
            doc.write(printContents);
            doc.close();

			ViewModel.SetPrintControlsVisibility(true);
        }

        private void File_Print(object sender, RoutedEventArgs e)
        {
            string printContents;
			printContents = ViewModel.GetPrintContents();

            mshtml.IHTMLDocument2 doc = webBrowser1.Document as mshtml.IHTMLDocument2;
            doc.clear();
            doc.write(printContents);
            doc.execCommand("Print", true, 0);
            doc.close();
        }
		#endregion  //printing

		#region taskText

        private void taskText_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ViewModel.TaskTextPreviewKeyUp(e);
        }


        #endregion

     }
}

