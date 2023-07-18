using MCMWebApp.Model.DataModel;
using MCMWebApp.Shared.Common;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace MCMWebApp.Pages.EventDetails
{
    public partial class Index
    {
        private string AzureFunctionBaseURL = "http://localhost:7265/";
        private string VenueAzureFunctionBaseURL = "http://localhost:7151/";
        private string searchString1 = "";
        private bool _loading = false;
        private Event selectedItem1 = null;
        private HashSet<Event> selectedItems = new HashSet<Event>();
        private List<Event> Events = new List<Event>();
        private List<Venue> VenueList  = new List<Venue>();
        IDialogReference dialogresult;
        [Inject] HttpClient HttpClient { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        [Inject] IDialogService DialogService { get; set; }

        private List<BreadcrumbItem> _items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Dashboard", href: "/", icon: Icons.Material.Filled.Home),
            new BreadcrumbItem("Event", href: "/events", disabled: true,icon: @Icons.Material.Filled.MyLocation),
        };

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Events = new();
                //Call to Azure function URL
                await RefreshGrid();
                await FetchVenueList();
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message);
            }
        }

        private bool FilterFunc1(Event eventdata) => FilterFunc(eventdata, searchString1);

        private bool FilterFunc(Event eventdata, string searchString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchString))
                    return true;

                if (eventdata.name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
                return false;
            }
        }

        private async Task OpenDialogAdd()
        {
            try
            {
                if (VenueList is null || VenueList is not null && !VenueList.Any())
                {
                    await FetchVenueList();
                }

                var parameters = new DialogParameters();
                parameters.Add("OnValidSubmit", EventCallback.Factory.Create<Event>(this, OnCreateValidSubmit));
                parameters.Add("VenueList", VenueList);
                var options = new DialogOptions
                {
                    // CloseOnEscapeKey = false,
                    CloseButton = true,
                    MaxWidth = MaxWidth.Large,
                    Position = DialogPosition.Center,
                    DisableBackdropClick = true
                };

                dialogresult = DialogService.Show<Add>("Add Event", parameters, options);
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private async Task OpenEditDialog(Event eventdata)
        {
            try
            {
                if (Events.Any(x => x.id == eventdata.id))
                {
                    if (VenueList is null || VenueList is not null && !VenueList.Any())
                    {
                        await FetchVenueList();
                    }

                    var parameters = new DialogParameters();
                    parameters.Add("EditModel", eventdata);
                    parameters.Add("VenueList", VenueList);
                    parameters.Add("OnValidSubmit", EventCallback.Factory.Create<Event>(this, OnUpdateValidSubmit));
                    var options = new DialogOptions()
                    {
                        CloseOnEscapeKey = false,
                        CloseButton = true,
                        MaxWidth = MaxWidth.Large,
                        Position = DialogPosition.Center,
                        DisableBackdropClick = true,
                    };

                    dialogresult = DialogService.Show<Edit>("Edit Event", parameters, options);
                }
                else
                {
                    Snackbar.Add("Invalid record.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private void Delete(Event eventdata)
        {
            try
            {
                var parameters = new DialogParameters();
                parameters.Add("ContentText", "Do you want to confirm?");
                parameters.Add("ButtonText", "Yes");
                parameters.Add("DeleteModelId", eventdata.id.ToString());
                parameters.Add("OnSubmit", EventCallback.Factory.Create<string>(this, OnDelete));
                var options = new DialogOptions
                {
                    CloseOnEscapeKey = false,
                    CloseButton = true,
                    MaxWidth = MaxWidth.ExtraLarge,
                    Position = DialogPosition.Center,
                    DisableBackdropClick = true
                };

                dialogresult = DialogService.Show<Confirm>("Confirm", parameters, options);
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private async Task OnCreateValidSubmit(Event createModel)
        {
            try
            {
                var eventResponse = await HttpClient.PostAsJsonAsync(string.Concat(AzureFunctionBaseURL, "api/event"), createModel);
                if (eventResponse != null && eventResponse.IsSuccessStatusCode)
                {
                    Snackbar.Add("Created successfully.", Severity.Success);
                    await RefreshGrid();
                    dialogresult.Close();
                }
                else
                {
                    Snackbar.Add("Create failed.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.StackTrace, Severity.Error);
                throw;
            }
        }

        private async Task OnUpdateValidSubmit(Event editModel)
        {
            try
            {
                var eventResponse = await HttpClient.PutAsJsonAsync(string.Concat(AzureFunctionBaseURL, "api/event"), editModel);
                if (eventResponse != null && eventResponse.IsSuccessStatusCode)
                {
                    Snackbar.Add("Update successfully.", Severity.Success);
                    dialogresult.Close();
                    await RefreshGrid();
                }
                else
                {
                    Snackbar.Add("Update failed.", Severity.Success);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.StackTrace, Severity.Error);
                throw;
            }
        }

        private async Task OnDelete(string editModelId)
        {
            try
            {
                var httpResponse = await HttpClient.DeleteAsync(string.Concat(AzureFunctionBaseURL, $"api/event/{editModelId}"));
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    Snackbar.Add("Deleted successfully.", Severity.Success);
                    await RefreshGrid();
                    dialogresult.Close();
                }
                else
                {
                    Snackbar.Add("Deletion failed.", Severity.Success);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.StackTrace, Severity.Error);
                throw;
            }
        }

        private async Task RefreshGrid()
        {
            try
            {
                _loading = true;
                var eventResponse = await HttpClient.GetFromJsonAsync<IEnumerable<Event>>(string.Concat(AzureFunctionBaseURL, "api/event"));
                if (eventResponse is not null && eventResponse.Any())
                {
                    Events = eventResponse.ToList();
                    _loading = false;
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private async Task FetchVenueList()
        {
            try
            {
                VenueList = new();
                var venueResponse = await HttpClient.GetFromJsonAsync<IEnumerable<Venue>>(string.Concat(VenueAzureFunctionBaseURL, "api/venue"));
                if (venueResponse is not null && venueResponse.Any())
                {
                    VenueList = venueResponse.OrderBy(x => x.name).ToList();
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
}
