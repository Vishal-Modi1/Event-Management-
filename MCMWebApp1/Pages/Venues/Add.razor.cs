using MCMWebApp.Model;
using MCMWebApp.Model.DataModel;
using MCMWebApp1.Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace MCMWebApp1.Pages.Venues
{
    public partial class Add
    {
        private Venue createModel = new();
        private bool visible;
        private void Close() => visible = false;

        [Parameter]
        public EventCallback<(Venue, List<AttachmentModel>)> OnValidSubmit { get; set; }
        [Inject] ISnackbar Snackbar { get; set; }
        public bool isDisabled { get; set; } = false;

        #region File Attachment
        private IReadOnlyList<IBrowserFile> newlyAttachedFiles { get; set; }
        private List<IBrowserFile> newlyAttachedFileList { get; set; } = new List<IBrowserFile>();
        private List<AttachmentModel> uploadAttachmentList = new List<AttachmentModel>();
        #endregion

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
                OnValidSubmit.InvokeAsync((createModel,uploadAttachmentList));
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private async Task AttachFiles(InputFileChangeEventArgs e)
        {
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
    }
}
