namespace Pekspro.RadioStorm.MAUI.Pages.Program;

public partial class ProgramControl
{
    public ProgramControl()
    {
        InitializeComponent();

        WidthStateHelper.ConfigureWidthState(this);
    }

    private ProgramModel ViewModel => (ProgramModel) BindingContext;
}
