
using MCMWebApp.Model.DataModel;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MCMWebApp.Pages.Venues
{
    public partial class Add
    {
        private Venue createModel = new();
        private bool visible;
        private void Close() => visible = false;

        [Parameter]
        public EventCallback<Venue> OnValidSubmit { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        private void ValidSubmit()
        {
            try
            {
                OnValidSubmit.InvokeAsync(createModel);
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
}
