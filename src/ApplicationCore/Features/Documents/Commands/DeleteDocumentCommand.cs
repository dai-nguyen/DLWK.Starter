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
        readonly IUserSession _userSession;
        readonly IStringLocalizer<DeleteDocumentCommandHandler> _localizer;
        readonly AppDbContext _dbContext;        
        readonly IFileService _fileService;
        

        public DeleteDocumentCommandHandler(
            ILogger<DeleteDocumentCommandHandler> logger,
            IUserSession userSession,
            IStringLocalizer<DeleteDocumentCommandHandler> localizer,            
            AppDbContext dbContext,            
            IFileService fileService)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;            
            _fileService = fileService;            
        }

        public async Task<Result<string>> Handle(
            DeleteDocumentCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var doc = await _dbContext.Documents.FindAsync(command.Id);

                if (doc == null)
                    return Result<string>.Fail(_localizer["Document Not Found!"]);

                var dbPath = doc.URL;

                _dbContext.Documents.Remove(doc);
                await _dbContext.SaveChangesAsync();

                await _fileService.DeleteAsync(dbPath);

                return Result<string>.Success(_localizer["Document Deleted"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {0} {UserId}", 
                    command.Id, _userSession.UserId);
            }

            return Result<string>.Fail(_localizer["Internal Error"]);
        }
    }
}
