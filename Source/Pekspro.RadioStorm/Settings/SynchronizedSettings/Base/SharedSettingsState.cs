namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Base
{
    public class SharedSettingsState //: DependencyObject
    {
        public bool IsSynchronizing { get; set; }

        public DateTime? LatestSynchronizingTime { get; set; }

        /*
        public bool IsSynchronizing
        {
            get { return (bool)GetValue(IsSynchronizingProperty); }
            set { SetValue(IsSynchronizingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSynchronizing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSynchronizingProperty =
            DependencyProperty.Register(nameof(IsSynchronizing), typeof(bool), typeof(SharedSettingsManager), new PropertyMetadata(0));

        public DateTime? LatestSynchronizingTime
        {
            get { return (DateTime?)GetValue(LatestSynchronizingTimeProperty); }
            set { SetValue(LatestSynchronizingTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LatestSynchronizingTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatestSynchronizingTimeProperty =
            DependencyProperty.Register(nameof(LatestSynchronizingTime), typeof(DateTime?), typeof(SharedSettingsManager), new PropertyMetadata(null));


        */
    }
}
