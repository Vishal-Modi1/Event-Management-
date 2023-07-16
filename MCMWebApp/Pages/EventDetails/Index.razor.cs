using MCMWebApp.Model.DataModel;
using MCMWebApp.Shared.Common;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace MCMWebApp.Pages.EventDetails
{
    public partial class Index
    {
        //private string searchString1 = "";
        //private Event selectedItem1 = null;
        //private HashSet<Event> selectedItems = new HashSet<Event>();
        //private List<Event> Events = new List<Event>();

        //private List<BreadcrumbItem> _items = new List<BreadcrumbItem>
        //{
        //    new BreadcrumbItem("Dashboard", href: "/", icon: Icons.Material.Filled.Home),
        //    new BreadcrumbItem("Events", href: "/events", disabled: true,icon: @Icons.Material.Filled.Event),
        //};

        //protected override async Task OnInitializedAsync()
        //{
        //    //Need to call Azure function
        //    //Events = await httpClient.GetFromJsonAsync<List<Event>>("webapi");

        //    for (int i = 0; i < 50; i++)
        //    {
        //        Event newObj = new Event
        //        {
        //            id = Guid.NewGuid(),
        //            name = "Event name -- " + (i + 1),
        //            description = "This is Description info " + (i + 1)
        //        };

        //        Events.Add(newObj);
        //    }
        //}

        //private bool FilterFunc1(Event eventdata) => FilterFunc(eventdata, searchString1);

        //private bool FilterFunc(Event eventdata, string searchString)
        //{
        //    if (string.IsNullOrWhiteSpace(searchString))
        //        return true;

        //    if (eventdata.name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        //        return true;
        //    return false;
        //}


        //private void OpenDialog(Event eventdata)
        //{

        //    var parameters = new DialogParameters();
        //    parameters.Add("eventdata", eventdata);
        //    //parameters.Add("_parameters", _parameters);
        //    var options = new DialogOptions()
        //    {
        //        // CloseOnEscapeKey = false,
        //        CloseButton = true,
        //        MaxWidth = MaxWidth.Large,
        //        Position = DialogPosition.Center,
        //        DisableBackdropClick = true

        //    };

        //    DialogService.Show<Edit>("Edit Event", parameters, options);

        //}
        //private void OpenDialogAdd()
        //{
        //    var options = new DialogOptions
        //    {
        //        // CloseOnEscapeKey = false,
        //        CloseButton = true,
        //        MaxWidth = MaxWidth.Large,
        //        Position = DialogPosition.Center,
        //        DisableBackdropClick = true

        //    };
        //    DialogService.Show<Add>("Add Event", options);
        //}
        //private void DeleteUser()
        //{
        //    var parameters = new DialogParameters();
        //    parameters.Add("ContentText", "Do you want to confirm?");
        //    parameters.Add("ButtonText", "Yes");
        //    var options = new DialogOptions
        //    {
        //        // CloseOnEscapeKey = false,
        //        CloseButton = true,
        //        MaxWidth = MaxWidth.ExtraLarge,
        //        Position = DialogPosition.Center,
        //        DisableBackdropClick = true

        //    };
        //    var dialogresult = DialogService.Show<Confirm>("Confirm", parameters, options);
        //}


        //
        private string AzureFunctionBaseURL = "http://localhost:7265/";
        private string searchString1 = "";
        private Event selectedItem1 = null;
        private HashSet<Event> selectedItems = new HashSet<Event>();
        private List<Event> Events = new List<Event>();
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
                HttpClient.BaseAddress = new Uri(AzureFunctionBaseURL);
                Events = new();
                //Call to Azure function URL
                await RefreshGrid();
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

        private void OpenDialogAdd()
        {
            try
            {
                var parameters = new DialogParameters();
                parameters.Add("OnValidSubmit", EventCallback.Factory.Create<Event>(this, OnCreateValidSubmit));
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

        private void OpenEditDialog(Event eventdata)
        {
            try
            {
                if (Events.Any(x => x.id == eventdata.id))
                {
                    var parameters = new DialogParameters();
                    parameters.Add("EditModel", eventdata);
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
                var eventResponse = await HttpClient.PostAsJsonAsync("api/event", createModel);
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
                var eventResponse = await HttpClient.PutAsJsonAsync("api/event", editModel);
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
                var httpResponse = await HttpClient.DeleteAsync($"api/event/{editModelId}");
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
                var eventResponse = await HttpClient.GetFromJsonAsync<IEnumerable<Event>>("api/event");
                if (eventResponse is not null && eventResponse.Any())
                {
                    Events = eventResponse.ToList();
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
}
