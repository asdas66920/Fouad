using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Fouad.Models;
using Fouad.Commands;
using Fouad.Services;

namespace Fouad.ViewModels
{
    /// <summary>
    /// ViewModel for the manual review interface.
    /// Coordinates the display and processing of new, match, and disagreement records.
    /// </summary>
    public class ReviewViewModel : INotifyPropertyChanged
    {
        private readonly ReviewService _reviewService;
        private readonly int _archiveId;
        private string? _fileName;
        private bool _isProcessing;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewViewModel"/> class.
        /// </summary>
        /// <param name="reviewService">The review service to use.</param>
        /// <param name="archiveId">The archive ID being reviewed.</param>
        /// <param name="fileName">The name of the file being reviewed.</param>
        public ReviewViewModel(ReviewService reviewService, int archiveId, string fileName)
        {
            _reviewService = reviewService;
            _archiveId = archiveId;
            _fileName = fileName;
            _isProcessing = false;
            
            NewRecords = new ObservableCollection<NewRecordViewModel>();
            MatchRecords = new ObservableCollection<MatchRecordViewModel>();
            DisagreementRecords = new ObservableCollection<DisagreementRecordViewModel>();
            
            ProcessDecisionsCommand = new RelayCommand(async o => await ProcessDecisionsAsync(), o => !_isProcessing);
            CancelCommand = new RelayCommand(o => OnCloseRequested());
        }

        /// <summary>
        /// Gets the collection of new records.
        /// </summary>
        public ObservableCollection<NewRecordViewModel> NewRecords { get; }

        /// <summary>
        /// Gets the collection of match records.
        /// </summary>
        public ObservableCollection<MatchRecordViewModel> MatchRecords { get; }

        /// <summary>
        /// Gets the collection of disagreement records.
        /// </summary>
        public ObservableCollection<DisagreementRecordViewModel> DisagreementRecords { get; }

        /// <summary>
        /// Gets the name of the file being reviewed.
        /// </summary>
        public string? FileName => _fileName;

        /// <summary>
        /// Gets the archive ID being reviewed.
        /// </summary>
        public int ArchiveId => _archiveId;

        /// <summary>
        /// Gets a value indicating whether the view model is currently processing.
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            private set
            {
                _isProcessing = value;
                OnPropertyChanged();
                ProcessDecisionsCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets the process decisions command.
        /// </summary>
        public RelayCommand ProcessDecisionsCommand { get; }

        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        public RelayCommand CancelCommand { get; }

        /// <summary>
        /// Initializes the view model with data.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            try
            {
                IsProcessing = true;
                
                // Identify matching records
                var (newRecords, matchRecords, disagreementRecords) = 
                    await _reviewService.IdentifyMatchingRecordsAsync(_archiveId);
                
                // Populate collections
                NewRecords.Clear();
                foreach (var record in newRecords)
                {
                    var viewModel = new NewRecordViewModel(record);
                    viewModel.Initialize();
                    NewRecords.Add(viewModel);
                }
                
                MatchRecords.Clear();
                foreach (var record in matchRecords)
                {
                    var viewModel = new MatchRecordViewModel(record);
                    viewModel.Initialize();
                    MatchRecords.Add(viewModel);
                }
                
                DisagreementRecords.Clear();
                foreach (var record in disagreementRecords)
                {
                    var viewModel = new DisagreementRecordViewModel(record);
                    viewModel.Initialize();
                    DisagreementRecords.Add(viewModel);
                }
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Processes all user decisions and cleans up.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task ProcessDecisionsAsync()
        {
            try
            {
                IsProcessing = true;
                
                // Collect decisions
                var newRecordDecisions = NewRecords.Select(vm => (vm.Record, vm.SelectedDecision)).ToList();
                var matchRecordDecisions = MatchRecords.Select(vm => (vm.Record, vm.SelectedDecision)).ToList();
                var disagreementRecordDecisions = DisagreementRecords.Select(vm => (vm.Record, vm.SelectedDecision)).ToList();
                
                // Process decisions
                await _reviewService.ProcessUserDecisionsAsync(
                    newRecordDecisions,
                    matchRecordDecisions,
                    disagreementRecordDecisions);
                
                // Clean up indexed content
                await _reviewService.CleanupIndexedContentAsync(_archiveId);
                
                // Notify that processing is complete
                OnCloseRequested(true);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Event raised when the review window should be closed.
        /// </summary>
        public event EventHandler<bool>? CloseRequested;

        /// <summary>
        /// Raises the CloseRequested event.
        /// </summary>
        /// <param name="success">Whether the processing was successful.</param>
        protected virtual void OnCloseRequested(bool success = false)
        {
            CloseRequested?.Invoke(this, success);
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