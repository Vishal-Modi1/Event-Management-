
using MCMWebApp.Model.DataModel;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MCMWebApp.Pages.Venues
{
    public partial class Edit
    {
        [Parameter]
        public Venue EditModel { get; set; }
        
        [Parameter]
        public EventCallback<Venue> OnValidSubmit { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        private void ValidSubmit() 
        {
            try
            {
                OnValidSubmit.InvokeAsync(EditModel);
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
}
