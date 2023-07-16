using MCMWebApp.Model.DataModel;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MCMWebApp.Pages.EventDetails
{
    public partial class Edit
    {
        [Parameter]
        public Event EditModel { get; set; }

        [Parameter]
        public EventCallback<Event> OnValidSubmit { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        public bool isDisabled { get; set; } = false;

        protected override void OnInitialized()
        {
            isDisabled = false;
            base.OnInitialized();
        }
        private void ValidSubmit()
        {
            try
            {
                isDisabled = true;
                OnValidSubmit.InvokeAsync(EditModel);
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
}
