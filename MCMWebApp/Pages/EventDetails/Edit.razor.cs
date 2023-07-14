using MCMWebApp.Model.DataModel;
using Microsoft.AspNetCore.Components;

namespace MCMWebApp.Pages.EventDetails
{
    public partial class Edit
    {
        [Parameter] 
        public Event eventdata { get; set; } = new Event();
        private Event eventModel = new();
        private bool visible;
        private void Close() => visible = false;
    }
}
