using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Fouad.Services;

namespace Fouad.ViewModels
{
    /// <summary>
    /// Main ViewModel that coordinates all other ViewModels in the application.
    /// Acts as the central hub for data flow and communication between components.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private TopBarViewModel? _topBar;
        private InfoBarViewModel? _infoBar;
        private ColumnSelectorViewModel? _columnSelector;
        private SideBarViewModel? _sideBar;
        private ResultsTableViewModel? _resultsTable;
        private HistoryTableViewModel? _historyTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            _infoBar = new InfoBarViewModel();
            _columnSelector = new ColumnSelectorViewModel();
            _topBar = new TopBarViewModel(_infoBar, _columnSelector);
            _sideBar = new SideBarViewModel(_topBar.GetFileDataService(), _infoBar, _columnSelector);
            _historyTable = new HistoryTableViewModel(_columnSelector);
            // Pass the FileDataService from TopBarViewModel to ResultsTableViewModel
            _resultsTable = new ResultsTableViewModel(_topBar.GetFileDataService(), _columnSelector);
            // Pass the HistoryTableViewModel reference to ResultsTableViewModel
            _resultsTable.SetHistoryTableViewModel(_historyTable);
        }

        /// <summary>
        /// Gets or sets the top bar view model.
        /// </summary>
        public TopBarViewModel? TopBar
        {
            get { return _topBar; }
            set
            {
                _topBar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the info bar view model.
        /// </summary>
        public InfoBarViewModel? InfoBar
        {
            get { return _infoBar; }
            set
            {
                _infoBar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the column selector view model.
        /// </summary>
        public ColumnSelectorViewModel? ColumnSelector
        {
            get { return _columnSelector; }
            set
            {
                _columnSelector = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the side bar view model.
        /// </summary>
        public SideBarViewModel? SideBar
        {
            get { return _sideBar; }
            set
            {
                _sideBar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the results table view model.
        /// </summary>
        public ResultsTableViewModel? ResultsTable
        {
            get { return _resultsTable; }
            set
            {
                _resultsTable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the history table view model.
        /// </summary>
        public HistoryTableViewModel? HistoryTable
        {
            get { return _historyTable; }
            set
            {
                _historyTable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}