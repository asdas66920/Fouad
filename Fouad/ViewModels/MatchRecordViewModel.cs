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

namespace Fouad.ViewModels
{
    /// <summary>
    /// ViewModel for a match record in the manual review interface.
    /// </summary>
    public class MatchRecordViewModel : INotifyPropertyChanged
    {
        private MatchRecord _record;
        private UserDecision _selectedDecision;
        private bool _isReviewed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchRecordViewModel"/> class.
        /// </summary>
        /// <param name="record">The match record to wrap.</param>
        public MatchRecordViewModel(MatchRecord record)
        {
            _record = record;
            _selectedDecision = UserDecision.Ignore;
            _isReviewed = false;
            
            IgnoreCommand = new RelayCommand(o => SelectedDecision = UserDecision.Ignore);
            UpdateCommand = new RelayCommand(o => SelectedDecision = UserDecision.Update);
        }

        /// <summary>
        /// Gets the underlying match record.
        /// </summary>
        public MatchRecord Record => _record;

        /// <summary>
        /// Gets or sets the selected decision for this record.
        /// </summary>
        public UserDecision SelectedDecision
        {
            get => _selectedDecision;
            set
            {
                _selectedDecision = value;
                _isReviewed = true;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsReviewed));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this record has been reviewed.
        /// </summary>
        public bool IsReviewed => _isReviewed;

        /// <summary>
        /// Gets the existing data for display.
        /// </summary>
        public ObservableCollection<string> ExistingData { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets the new data for display.
        /// </summary>
        public ObservableCollection<string> NewData { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets the ignore command.
        /// </summary>
        public ICommand IgnoreCommand { get; }

        /// <summary>
        /// Gets the update command.
        /// </summary>
        public ICommand UpdateCommand { get; }

        /// <summary>
        /// Initializes the view model with the record data.
        /// </summary>
        public void Initialize()
        {
            ExistingData.Clear();
            foreach (var value in _record.ExistingData)
            {
                ExistingData.Add(value);
            }

            NewData.Clear();
            foreach (var value in _record.NewData)
            {
                NewData.Add(value);
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