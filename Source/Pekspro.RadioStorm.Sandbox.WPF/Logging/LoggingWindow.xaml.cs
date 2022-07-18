using Pekspro.RadioStorm.UI.ViewModel.Logging;

namespace Pekspro.RadioStorm.Sandbox.WPF.Logging
{
    /// <summary>
    /// Interaction logic for LoggingWindow.xaml
    /// </summary>
    public partial class LoggingWindow : Window
    {
        public LoggingWindow(LoggingViewModel loggingViewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();

            DataContext = loggingViewModel;
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        protected LoggingViewModel ViewModel => (LoggingViewModel)DataContext;

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            ViewModel.OnNavigatedTo();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            ViewModel.OnNavigatedFrom();
        }
    }
}
