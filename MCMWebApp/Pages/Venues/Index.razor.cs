using MCMWebApp.Model.DataModel;
using MCMWebApp.Shared.Common;
using MudBlazor;

namespace MCMWebApp.Pages.Venues
{
    public partial class Index
    {
        private string searchString1 = "";
        private Venue selectedItem1 = null;
        private HashSet<Venue> selectedItems = new HashSet<Venue>();
        private List<Venue> Venues = new List<Venue>();

        private List<BreadcrumbItem> _items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Dashboard", href: "/", icon: Icons.Material.Filled.Home),
            new BreadcrumbItem("Venue", href: "/venues", disabled: true,icon: @Icons.Material.Filled.MyLocation),
        };

        protected override async Task OnInitializedAsync()
        {
            //Need to call Azure function
            //Events = await httpClient.GetFromJsonAsync<List<Event>>("webapi");

            for (int i = 0; i < 50; i++)
            {
                Venue newObj = new Venue
                {
                    id = Guid.NewGuid(),
                    name = "Venue name -- " + (i + 1),
                    url = "This is url info " + (i + 1)
                };

                Venues.Add(newObj);
            }
        }

        private bool FilterFunc1(Venue venuedata) => FilterFunc(venuedata, searchString1);

        private bool FilterFunc(Venue venuedata, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;

            if (venuedata.name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }


        private void OpenDialog(Venue venuedata)
        {

            var parameters = new DialogParameters();
            //parameters.Add("InquiryModel", inquiry);
            //parameters.Add("_parameters", _parameters);
            var options = new DialogOptions()
            {
                //CloseOnEscapeKey = false,
                CloseButton = true,
                MaxWidth = MaxWidth.Large,
                Position = DialogPosition.Center,
                DisableBackdropClick = true

            };

            DialogService.Show<Edit>("Edit Venue", parameters, options);

        }
        private void OpenDialogAdd()
        {
            var options = new DialogOptions
            {
                // CloseOnEscapeKey = false,
                CloseButton = true,
                MaxWidth = MaxWidth.Large,
                Position = DialogPosition.Center,
                DisableBackdropClick = true

            };
            DialogService.Show<Add>("Add Venue", options);
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
