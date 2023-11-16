using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Web.Components.Shared
{
    public partial class Dialog
    {
        [CascadingParameter] 
        MudDialogInstance MudDialog { get; set; }

        [Parameter] 
        public string ContentText { get; set; }

        [Parameter] 
        public string ButtonText { get; set; }

        [Parameter] 
        public MudBlazor.Color Color { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private void Submit()
        {
            MudDialog.Close(DialogResult.Ok(true));
        }
    }
}
