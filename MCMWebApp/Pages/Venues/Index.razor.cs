using MCMWebApp.Model;
using MCMWebApp.Model.DataModel;
using MCMWebApp.Shared.Common;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MCMWebApp.Pages.Venues
{
    public partial class Index
    {
        private string AzureFunctionBaseURL = "http://localhost:7151/";
        private string searchString1 = "";
        private bool _loading = false;
        private Venue selectedItem1 = null;
        private HashSet<Venue> selectedItems = new HashSet<Venue>();
        private List<Venue> Venues = new List<Venue>();
        IDialogReference dialogresult;
        [Inject] HttpClient HttpClient { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        [Inject] IDialogService DialogService { get; set; }


        private List<BreadcrumbItem> _items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Dashboard", href: "/", icon: Icons.Material.Filled.Home),
            new BreadcrumbItem("Venue", href: "/venues", disabled: true,icon: @Icons.Material.Filled.MyLocation),
        };

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HttpClient.BaseAddress = new Uri(AzureFunctionBaseURL);
                Venues = new();
                //Call to Azure function URL
                await RefreshGrid();
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message);
            }
        }

        private bool FilterFunc1(Venue venuedata) => FilterFunc(venuedata, searchString1);

        private bool FilterFunc(Venue venuedata, string searchString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchString))
                    return true;

                if (venuedata.name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
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
                parameters.Add("OnValidSubmit", EventCallback.Factory.Create<Venue>(this, OnCreateValidSubmit));
                var options = new DialogOptions
                {
                    // CloseOnEscapeKey = false,
                    CloseButton = true,
                    MaxWidth = MaxWidth.Large,
                    Position = DialogPosition.Center,
                    DisableBackdropClick = true
                };

                dialogresult = DialogService.Show<Add>("Add Venue", parameters, options);
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
        
        private void OpenEditDialog(Venue venuedata)
        {
            try
            {
                if (Venues.Any(x => x.id == venuedata.id))
                {
                    var parameters = new DialogParameters();
                    parameters.Add("EditModel", venuedata);
                    parameters.Add("OnValidSubmit", EventCallback.Factory.Create<Venue>(this, OnUpdateValidSubmit));
                    var options = new DialogOptions()
                    {
                        CloseOnEscapeKey = false,
                        CloseButton = true,
                        MaxWidth = MaxWidth.Large,
                        Position = DialogPosition.Center,
                        DisableBackdropClick = true,
                    };

                    dialogresult = DialogService.Show<Edit>("Edit Venue", parameters, options);
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
        
        private void Delete(Venue venuedata)
        {
            try
            {
                var parameters = new DialogParameters();
                parameters.Add("ContentText", "Do you want to confirm?");
                parameters.Add("ButtonText", "Yes");
                parameters.Add("DeleteModelId", venuedata.id.ToString());
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
        
        private async Task OnCreateValidSubmit(Venue createModel) 
        {
            try
            {
                var venueResponse = await HttpClient.PostAsJsonAsync("api/venue", createModel);
                if (venueResponse != null && venueResponse.IsSuccessStatusCode)
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

        private async Task OnUpdateValidSubmit(Venue editModel) 
        {
            try
            {
                var venueResponse = await HttpClient.PutAsJsonAsync("api/venue", editModel);
                if (venueResponse != null && venueResponse.IsSuccessStatusCode)
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
                var httpResponse = await HttpClient.DeleteAsync($"api/venue/{editModelId}");
                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    Snackbar.Add("Deleted successfully.", Severity.Success);
                    await RefreshGrid();
                    dialogresult.Close();
                }
                else {
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
                var venueResponse = await HttpClient.GetFromJsonAsync<IEnumerable<Venue>>("api/venue");
                if (venueResponse is not null && venueResponse.Any())
                {
                    Venues = venueResponse.ToList();
                    _loading = false;
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
}
