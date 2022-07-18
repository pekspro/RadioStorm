namespace Pekspro.RadioStorm.Sandbox.WPF.Program
{
    /// <summary>
    /// Interaction logic for UserControlProgramItem.xaml
    /// </summary>
    public partial class ProgramControl : UserControl
    {
        public ProgramControl()
        {
            InitializeComponent();
        }

        private ProgramModel ViewModel
        {
            get
            {
                var vm = DataContext as ProgramModel;

                return vm;
            }
        }
    }
}
