using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.View.UserControl;
using System.Windows.Controls;

namespace GPCAEventsCheckIn.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private AttendeeViewModel _attendeeViewModel;
        private UserControl _currentView;
        private string _previousView;
        private AttendeeModel _currentAttendee;
        private string _backDropStatus = "Collapsed";
        private List<AttendeeModel> _attendeeSuggesstionList;
        private string? _selectedCompanyName;
        private string _loadingProgressStatus = "Collapsed";
        private string _loadingProgressMessage = "Loading...";

        public string EventBanner 
        {
            get
            {
                return EventModel.EventBanner;
            }
        }

        public string BadgeBanner
        {
            get
            {
                return EventModel.BadgeBanner;
            }
        }

        public string? SelectedCompanyName
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

        public string LoadingProgressMessage
        {
            get { return _loadingProgressMessage; }
            set
            {
                _loadingProgressMessage = value;
                OnPropertyChanged(nameof(LoadingProgressMessage));
            }
        }

        public string LoadingProgressStatus
        {
            get { return _loadingProgressStatus; }
            set
            {
                _loadingProgressStatus = value;
                OnPropertyChanged(nameof(LoadingProgressStatus));
            }
        }

        public string BackDropStatus
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

        public string PreviousView
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
                NavigateToAttendeeListView("SearchCategoriesView");
            }
        }

        public void NavigateToHomeView()
        {
            CurrentView = new HomeView(this);
        }
        public void NavigateToSearchCategoryView(string previousView)
        {
            PreviousView = previousView;
            CurrentView = new SearchCategoriesView(this);
        }
        public void NavigateToAttendeeListView(string previousView)
        {
            PreviousView = previousView;
            CurrentView = new AttendeeListView(this);
        }
        public void NavigateToAttendeeDetailsView(String previousView)
        {
            PreviousView = previousView;
            CurrentView = new AttendeeDetailsView(this);
        }

        public MainViewModel()
        {
            AttendeeViewModel = new AttendeeViewModel(this);
            NavigateToHomeView();
        }
    }
}
