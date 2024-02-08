﻿using GPCAEventsCheckIn.Model;
using System.Windows.Controls;

namespace GPCAEventsCheckIn.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private AttendeeViewModel _attendeeViewModel;
        private UserControl _currentView;
        private String _previousView;
        private int _currentIndex;
        private AttendeeModel _currentAttendee;
        private String _backDropStatus = "Collapsed";
        private List<AttendeeModel> _attendeeSuggesstionList;
        private String? _selectedCompanyName;



        public String? SelectedCompanyName
        {
            get { return _selectedCompanyName; }
            set
            {
                _selectedCompanyName = value;
                OnPropertyChanged(nameof(SelectedCompanyName));
            }
        }

        public List<AttendeeModel> AttendeeSuggesstionList
        {
            get { return _attendeeSuggesstionList; }
            set
            {
                _attendeeSuggesstionList = value;
                OnPropertyChanged(nameof(AttendeeSuggesstionList));
            }
        }

        public String BackDropStatus
        {
            get { return _backDropStatus; }
            set
            {
                _backDropStatus = value;
                OnPropertyChanged(nameof(BackDropStatus));
            }
        }

        public AttendeeViewModel AttendeeViewModel
        {
            get { return _attendeeViewModel; }
            set
            {
                if (_attendeeViewModel != value)
                {
                    _attendeeViewModel = value;
                    OnPropertyChanged(nameof(AttendeeViewModel));
                }
            }
        }

        public AttendeeModel CurrentAttendee
        {
            get { return _currentAttendee; }
            set
            {
                _currentAttendee = value;
                OnPropertyChanged(nameof(CurrentAttendee));
            }
        }

        public int CurrentIndex
        {
            get { return _currentIndex; }
            set
            {
                _currentIndex = value;
                OnPropertyChanged(nameof(CurrentIndex));
            }
        }

        public String PreviousView
        {
            get { return _previousView; }
            set
            {
                _previousView = value;
                OnPropertyChanged(nameof(PreviousView));
            }
        }

        public UserControl CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public void ReturnBack()
        {
            if (PreviousView == "HomeView")
            {
                PreviousView = null;
                NavigateToHomeView();
            }
            else if (PreviousView == "SearchCategoriesView")
            {
                PreviousView = null;
                NavigateToSearchCategoryView("HomeView");
            }
            else if (PreviousView == "AttendeeListView")
            {
                NavigateToAttendeeListView("HomeView");
            }
        }

        public void NavigateToAttendeeListView(string previousView)
        {
            PreviousView = previousView;
            CurrentView = new AttendeeListView(this, DelegateViewModel);
        }

        public void NavigateToSearchCategoryView(string previousView)
        {
            PreviousView = previousView;
            CurrentView = new SearchCategoriesView(this);
        }

        public void NavigateToAttendeeDetailsView(String previousView)
        {
            PreviousView = previousView;
            CurrentView = new AttendeeDetailsView(this);
        }
        public void NavigateToHomeView()
        {
            CurrentView = new HomeView(this, DelegateViewModel);
        }


        public MainViewModel()
        {
            AttendeeViewModel = new AttendeeViewModel();
            NavigateToHomeView();
        }
    }
}
