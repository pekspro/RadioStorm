namespace Pekspro.RadioStorm.MAUI.Controls;

internal sealed class ListSearchHandler : SearchHandler
{
    protected override void OnQueryChanged(string oldValue, string newValue)
    {
        base.OnQueryChanged(oldValue, newValue);

        if (string.IsNullOrWhiteSpace(newValue))
        {
            ItemsSource = null;
        }
        else
        {
            ItemsSource = (BindingContext as ISearch)
                            ?.Search(newValue);
        }
    }

    protected override async void OnItemSelected(object item)
    {
        base.OnItemSelected(item);

        // Let the animation complete
        //await Task.Delay(1000);

        var searchItem = item as SearchItem;
        
        if(searchItem is not null)
        {
            if (searchItem.Type == SearchItemType.Channel)
            {
                string param = ChannelDetailsViewModel.CreateStartParameter(searchItem);

                await Shell.Current.GoToAsync(nameof(ChannelDetailsPage), new Dictionary<string, object>()
                {
                    { "Data", param }
                });
            }
            else if (searchItem.Type == SearchItemType.Program)
            {
                string param = ProgramDetailsViewModel.CreateStartParameter(searchItem);

                await Shell.Current.GoToAsync(nameof(ProgramDetailsPage), new Dictionary<string, object>()
                {
                    { "Data", param }
                });
            }
            else if (searchItem.Type == SearchItemType.Episode)
            {
                string param = EpisodeDetailsViewModel.CreateStartParameter(searchItem);

                await Shell.Current.GoToAsync(nameof(EpisodeDetailsPage), new Dictionary<string, object>()
                {
                    { "Data", param }
                });
            }
        }
    }
}
