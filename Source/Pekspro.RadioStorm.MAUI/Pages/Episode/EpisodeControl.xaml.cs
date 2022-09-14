using Pekspro.RadioStorm.UI.Model.Episode;
using Pekspro.RadioStorm.UI.ViewModel.Episode;

namespace Pekspro.RadioStorm.MAUI.Pages.Episode;

public partial class EpisodeControl
{
    public EpisodeControl()
    {
        InitializeComponent();
    }

    private EpisodeModel ViewModel => (EpisodeModel) BindingContext;
}
