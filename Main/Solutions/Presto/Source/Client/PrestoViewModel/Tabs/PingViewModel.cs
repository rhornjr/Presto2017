using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Logic;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Tabs
{
    /// <summary>
    /// Helper class/DTO/wrapper from view model to view.
    /// </summary>
    public class ServerPingDto : NotifyPropertyChangedBase
    {
        private ApplicationServer _applicationServer;
        private DateTime? _responseTime;
        private string _comment;

        /// <summary>
        /// Gets or sets the application server.
        /// </summary>
        /// <value>
        /// The application server.
        /// </value>
        public ApplicationServer ApplicationServer
        {
            get { return this._applicationServer; }

            set
            {
                this._applicationServer = value;
                NotifyPropertyChanged(() => this.ApplicationServer);
            }
        }

        /// <summary>
        /// Gets or sets the response time.
        /// </summary>
        /// <value>
        /// The response time.
        /// </value>
        public DateTime? ResponseTime
        {
            get { return this._responseTime; }

            set
            {
                this._responseTime = value;
                NotifyPropertyChanged(() => this.ResponseTime);
            }
        }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string Comment
        {
            get { return this._comment; }

            set
            {
                this._comment = value;
                NotifyPropertyChanged(() => this.Comment);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PingViewModel : ViewModelBase, IDisposable
    {
        private const int TotalTimerRunTimeInMinutes = 5;

        private Timer _timer;
        private DateTime _timerStartTime;        
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private static readonly object _locker = new object();
        private PingRequest _pingRequest;
        private ObservableCollection<ServerPingDto> _serverPingDtoList;

        /// <summary>
        /// Gets the ping command.
        /// </summary>
        public ICommand PingCommand { get; private set; }

        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        public ICommand RefreshCommand { get; private set; }

        /// <summary>
        /// Gets the ping request.
        /// </summary>
        public PingRequest PingRequest
        {
            get
            {
                if (this._pingRequest == null)
                {
                    this._pingRequest = PingRequestLogic.GetMostRecent();
                }
                
                return this._pingRequest;
            }

            private set
            {
                this._pingRequest = value;
                NotifyPropertyChanged(() => this.PingRequest);
            }
        }

        /// <summary>
        /// Gets the server ping DTO list.
        /// </summary>
        public ObservableCollection<ServerPingDto> ServerPingDtoList
        {
            get
            {
                if (this._serverPingDtoList == null)
                {
                    this.ServerPingDtoList = new ObservableCollection<ServerPingDto>();

                    IEnumerable allServers = ApplicationServerLogic.GetAll();

                    foreach (ApplicationServer server in allServers)
                    {
                        this.ServerPingDtoList.Add(new ServerPingDto() { ApplicationServer = server });
                    }
                }

                return this._serverPingDtoList;
            }

            private set
            {
                this._serverPingDtoList = value;
                this.NotifyPropertyChanged(() => this.ServerPingDtoList);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PingViewModel"/> class.
        /// </summary>
        public PingViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();
        }

        private void Initialize()
        {
            this.RefreshCommand = new RelayCommand(_ => Refresh(null));
            this.PingCommand    = new RelayCommand(_ => Ping());

            Refresh(null);
        }

        private void Ping()
        {
            PingRequest pingRequest = new PingRequest(DateTime.Now, WindowsIdentity.GetCurrent().Name);            

            PingRequestLogic.Save(pingRequest);

            this.PingRequest = pingRequest;

            ClearResponseTimes();

            this._timer = new Timer(this.Refresh, this._autoResetEvent, 0, 5000);
            this._timerStartTime = DateTime.Now;
        }

        private void ClearResponseTimes()
        {
            foreach (ServerPingDto serverPingDto in this.ServerPingDtoList)
            {
                serverPingDto.ResponseTime = null;
                serverPingDto.Comment      = null;
            }
        }

        private void Refresh(object stateInfo)
        {
            if (!Monitor.TryEnter(_locker)) { return; }

            // Don't run forever
            if (DateTime.Now.Subtract(this._timerStartTime).Minutes >= TotalTimerRunTimeInMinutes) { this._timer = null; }

            if (this.PingRequest == null) { return; }

            try
            {
                foreach (PingResponse response in PingResponseLogic.GetAllForPingRequest(this.PingRequest))
                {
                    ServerPingDto serverPingDto = this.ServerPingDtoList.Where(x => x.ApplicationServer.Id == response.ApplicationServerId).FirstOrDefault();

                    if (serverPingDto == null || serverPingDto.ResponseTime == response.ResponseTime) { continue; }

                    serverPingDto.ResponseTime = response.ResponseTime;
                    serverPingDto.Comment      = response.Comment;
                }

                // If every server has a response, no need to continue polling.
                if (this.ServerPingDtoList.Where(dto => dto.ResponseTime == null).FirstOrDefault() == null)
                {
                    // Couldn't find any response times of null.
                    this._timer = null;
                }

                ViewModelUtility.MainWindowViewModel.UserMessage = ViewModelResources.PingItemsRefreshed;
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing == false) { return; }

            if (this._timer != null) { this._timer.Dispose(); }

            if (this._autoResetEvent != null) { this._autoResetEvent.Dispose(); }
        }
    }
}
