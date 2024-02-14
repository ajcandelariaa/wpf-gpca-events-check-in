﻿using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPCAEventsCheckIn.View.UserControl
{
    public partial class AttendeeListView
    {
        private AttendeeViewModel _attendeeViewModel;
        private MainViewModel _mainViewModel;

        public AttendeeListView(MainViewModel mainViewModel, AttendeeViewModel existingAttendeeViewModel)
        {
            InitializeComponent();
            _attendeeViewModel = existingAttendeeViewModel;
            _mainViewModel = mainViewModel;

            if (_mainViewModel.SelectedCompanyName != null)
            {
                _mainViewModel.AttendeeSuggesstionList = _attendeeViewModel.ConfirmedAttendees
                    .Where(delegateItem => string.Equals(delegateItem.CompanyName, _mainViewModel.SelectedCompanyName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        private void Btn_Return(object sender, MouseButtonEventArgs e)
        {
            _mainViewModel.ReturnBack();
            _mainViewModel.SelectedCompanyName = null;

            if(_mainViewModel.AttendeeSuggesstionList != null)
            {
                _mainViewModel.AttendeeSuggesstionList.Clear();
            }
        }

        private void Tb_Keystroke(object sender, TextChangedEventArgs e)
        {
            string searchText = textBoxName.Text.ToLower();

            if (_mainViewModel.SelectedCompanyName == null)
            {
                _mainViewModel.AttendeeSuggesstionList = _attendeeViewModel.ConfirmedAttendees
                    .Where(delegateItem => delegateItem.FullName.ToLower().Contains(searchText))
                    .ToList();
            }
            else
            {
                _mainViewModel.AttendeeSuggesstionList = _attendeeViewModel.ConfirmedAttendees
                    .Where(delegateItem => string.Equals(delegateItem.CompanyName, _mainViewModel.SelectedCompanyName, StringComparison.OrdinalIgnoreCase))
                    .Where(delegateItem => delegateItem.FullName.ToLower().Contains(searchText))
                    .ToList();
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyDataGrid.SelectedItem != null)
            {
                AttendeeModel selectedDelegate = (AttendeeModel)MyDataGrid.SelectedItem;
                _mainViewModel.CurrentAttendee = selectedDelegate;
                _mainViewModel.NavigateToAttendeeDetailsView("AttendeeListView");
            }
        }
    }
}