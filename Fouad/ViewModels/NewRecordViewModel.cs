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
    /// ViewModel for a new record in the manual review interface.
    /// </summary>
    public class NewRecordViewModel : INotifyPropertyChanged
    {
        private NewRecord _record;
        private UserDecision _selectedDecision;
        private bool _isReviewed;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewRecordViewModel"/> class.
        /// </summary>
        /// <param name="record">The new record to wrap.</param>
        public NewRecordViewModel(NewRecord record)
        {
            _record = record;
            _selectedDecision = UserDecision.Ignore;
            _isReviewed = false;
            
            IgnoreCommand = new RelayCommand(o => SelectedDecision = UserDecision.Ignore);
            AddAsNewCommand = new RelayCommand(o => SelectedDecision = UserDecision.AddAsNew);
        }

        /// <summary>
        /// Gets the underlying new record.
        /// </summary>
        public NewRecord Record => _record;

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
        /// Gets the dynamic column values for display.
        /// </summary>
        public ObservableCollection<string> DynamicColumnValues { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets the ignore command.
        /// </summary>
        public ICommand IgnoreCommand { get; }

        /// <summary>
        /// Gets the add as new command.
        /// </summary>
        public ICommand AddAsNewCommand { get; }

        /// <summary>
        /// Initializes the view model with the record data.
        /// </summary>
        public void Initialize()
        {
            DynamicColumnValues.Clear();
            foreach (var value in _record.DynamicColumnValues)
            {
                DynamicColumnValues.Add(value);
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