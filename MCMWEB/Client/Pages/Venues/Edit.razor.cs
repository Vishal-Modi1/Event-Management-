﻿using MCMWEB.Shared.DataModel;

namespace MCMWEB.Client.Pages.Venues
{
    public partial class Edit
    {
        private Venue venueModel = new();
        private bool visible;
        private void Close() => visible = false;
    }
}
