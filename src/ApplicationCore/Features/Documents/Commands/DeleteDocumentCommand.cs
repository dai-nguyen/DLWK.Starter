using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Documents.Commands
{
    public class DeleteDocumentCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = "";
    }

    internal class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;                
        

        public DeleteDocumentCommandHandler(
            ILogger<DeleteDocumentCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<DeleteDocumentCommandHandler> localizer,            
            AppDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;                        
        }

        public async Task<Result<string>> Handle(
            DeleteDocumentCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.Documents.FindAsync(command.Id);

                if (entity == null)
                    return Result<string>.Fail(_localizer[Constants.Messages.NotFound]);
                
                _dbContext.Documents.Remove(entity);
                await _dbContext.SaveChangesAsync();
               
                return Result<string>.Success(_localizer[Constants.Messages.Deleted]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {0} {UserId}", 
                    command.Id, _userSession.UserId);
            }

            return Result<string>.Fail(_localizer[Constants.Messages.InternalError]);
        }
    }
}
