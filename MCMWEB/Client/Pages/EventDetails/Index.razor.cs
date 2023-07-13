using MCMWEB.Client.Shared.Common;
using MCMWEB.Shared.DataModel;
using MudBlazor;


namespace MCMWEB.Client.Pages.EventDetails
{
    public partial class Index
    {
        private string searchString1 = "";
        private Event selectedItem1 = null;
        private HashSet<Event> selectedItems = new HashSet<Event>();
        private List<Event> Events = new List<Event>();

        private List<BreadcrumbItem> _items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Dashboard", href: "/", icon: Icons.Material.Filled.Home),
            new BreadcrumbItem("Events", href: "/events", disabled: true,icon: @Icons.Material.Filled.Event),
        }; 
       
        protected override async Task OnInitializedAsync()
        {
            //Need to call Azure function
            //Events = await httpClient.GetFromJsonAsync<List<Event>>("webapi");

            for (int i = 0; i < 50; i++)
            {
                Event newObj = new Event
                {
                    id = Guid.NewGuid(),
                    name = "Event name -- " + (i + 1),
                    description = "This is Description info " + (i + 1)
                };

                Events.Add(newObj);
            }
        }

        private bool FilterFunc1(Event eventdata) => FilterFunc(eventdata, searchString1);

        private bool FilterFunc(Event eventdata, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;

            if (eventdata.name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }


        private void OpenDialog(Event eventdata)
        {

            var parameters = new DialogParameters();
            parameters.Add("eventdata", eventdata);
            //parameters.Add("_parameters", _parameters);
            var options = new DialogOptions()
            {
               // CloseOnEscapeKey = false,
                CloseButton = true,
                MaxWidth = MaxWidth.Large,
                Position = DialogPosition.Center,
                DisableBackdropClick = true

            };

            DialogService.Show<Edit>("Edit Event", parameters, options);

        }
        private void OpenDialogAdd()
        {
            var options = new DialogOptions {
               // CloseOnEscapeKey = false,
                CloseButton = true,
                MaxWidth = MaxWidth.Large,
                Position = DialogPosition.Center,
                DisableBackdropClick = true

            };
            DialogService.Show<Add>("Add Event", options);
        }
        private void DeleteUser()
        {
            var parameters = new DialogParameters();
            parameters.Add("ContentText", "Do you want to confirm?");
            parameters.Add("ButtonText", "Yes");
            var options = new DialogOptions
            {
                // CloseOnEscapeKey = false,
                CloseButton = true,
                MaxWidth = MaxWidth.ExtraLarge,
                Position = DialogPosition.Center,
                DisableBackdropClick = true

            };
            var dialogresult = DialogService.Show<Confirm>("Confirm", parameters, options);
        }
    }
}
