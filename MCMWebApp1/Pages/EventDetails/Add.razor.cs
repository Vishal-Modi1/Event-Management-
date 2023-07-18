using MCMWebApp.Model;
using MCMWebApp.Model.AzureConfig;
using MCMWebApp.Model.DataModel;
using MCMWebApp1.Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Tewr.Blazor.FileReader;

namespace MCMWebApp1.Pages.EventDetails
{
    public partial class Add
    {
        private Event createModel = new();
        private bool visible;

        private string AzureFunctionBaseURL = "http://localhost:7265/";
        private string VenueAzureFunctionBaseURL = "http://localhost:7151/";
        private void Close() => visible = false;

        [Parameter]
        public EventCallback<(Event, List<AttachmentModel>)> OnValidSubmit { get; set; }

        [Parameter]
        public List<Venue> VenueList { get; set; }

        [Inject] ISnackbar Snackbar { get; set; }
        
        
        #region File Attachment
        //private TicketAttachmentModel ticketAttachmentModel = new TicketAttachmentModel();
        private IReadOnlyList<IBrowserFile> newlyAttachedFiles { get; set; }
        private List<IBrowserFile> newlyAttachedFileList { get; set; } = new List<IBrowserFile>();
        private List<AttachmentModel> uploadAttachmentList = new List<AttachmentModel>();
        #endregion

        public bool isDisabled { get; set; } = false;
        protected override void OnInitialized()
        {
            isDisabled = false;
            base.OnInitialized();
        }

        private async Task ValidSubmit()
        {
            try
            {
                isDisabled = true;
                //await SendAttachments();
                await OnValidSubmit.InvokeAsync((createModel, uploadAttachmentList));
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }

        #region FileAttachment

        private async Task<string> UploadFile(UploadBlobRequestDto uploadBlobRequestDto)
        {
            //Logger.LogDebug("Upload Files In");
            try
            {
                //BlobDto result = await AzureBlobService.SaveBlobAsync(uploadBlobRequestDto, ContainerEnum.EVENT);
                //Logger.LogDebug("Upload Files out");
                return null;
                //return result.Uri;
            }
            catch (Exception ex)
            {
                Snackbar.Add("Error while uploading file", Severity.Error);
                //Logger.LogDebug(ex.Message, ex);
            }

            return string.Empty;
        }

        private async Task AttachFiles(InputFileChangeEventArgs e)
        {
            //Logger.LogDebug("Attach Files in");
            try
            {
                newlyAttachedFiles = e.GetMultipleFiles();
                newlyAttachedFileList.AddRange(newlyAttachedFiles);

                if (newlyAttachedFiles != null && newlyAttachedFiles.Any())
                {
                    bool showMaxFileSizeError = false;
                    for (int i = 0; i < newlyAttachedFiles.Count(); i++)
                    {
                        IBrowserFile file = newlyAttachedFiles[i];
                        if (!FileUploadHelper.ValidFileSize(file))
                        {
                            showMaxFileSizeError = true;
                        }
                        else
                        {
                            byte[] fileBytes = await FileUploadHelper.GetFileByteArray(file);
                            //var extension = MimeTypeMap.GetExtension(file.ContentType);
                            uploadAttachmentList.Add(new AttachmentModel()
                            {
                                Content = fileBytes,
                                FileName = $"{new DateTime().Ticks}_EVENT_{file.Name}",
                                ContentType = file.ContentType
                            });
                        }
                    }

                    if (showMaxFileSizeError)
                    {
                        Snackbar.Add("The maximum file size for the attachment is 8 MB.", Severity.Error);
                    }
                }

                //Logger.LogDebug("Attach Files out");
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
                //Logger.LogDebug(ex.Message, ex);
            }
        }

        private async Task<List<AttachmentModel>> SendAttachments()
        {
            if (uploadAttachmentList != null && uploadAttachmentList.Any())
            {
                bool showMaxFileSizeError = false;
                for (int i = 0; i < uploadAttachmentList.Count(); i++)
                {
                    var file = uploadAttachmentList[i];

                    string fileURL = await UploadFile(new UploadBlobRequestDto
                    {
                        Content = file.Content,
                        Name = file.FileName,
                        ContentType = file.ContentType
                    });

                    if (!string.IsNullOrEmpty(fileURL))
                    {
                        createModel.photos.Add(fileURL);
                    }
                }

                if (showMaxFileSizeError)
                {
                    Snackbar.Add("The maximum file size for the attachment is 8 MB.", Severity.Error);
                }
            }

            return uploadAttachmentList;
        }
        #endregion
    }
}
