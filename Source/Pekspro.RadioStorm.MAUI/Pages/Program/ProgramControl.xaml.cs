namespace Pekspro.RadioStorm.MAUI.Pages.Program;

public partial class ProgramControl
{
    public ProgramControl()
    {
        InitializeComponent();

        WidthStateHelper.ConfigureWidthState(this);
    }

    protected ProgramModel ViewModel => BindingContext as ProgramModel;
}
