using ApplicationCore.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Web.Pages.Pages.Users
{
    public partial class UserCreate
    {
        [Inject]
        ISnackbar _snackbar { get; set; }
        [Inject]
        IMediator _mediator { get; set; }
        

        
    }
}
