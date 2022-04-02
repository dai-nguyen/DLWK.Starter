 using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Documents.Commands
{
    public partial class CreateDocumentCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }

    internal class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;        
        readonly AppDbContext _dbContext;        
        
        public CreateDocumentCommandHandler(
            ILogger<CreateDocumentCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateDocumentCommandHandler> localizer,            
            AppDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;            
            _dbContext = dbContext;            
        }

        public async Task<Result<string>> Handle(
            CreateDocumentCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = new Document()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = command.Title,
                    Description = command.Description,
                    Data = command.Data,
                    Size = command.Size,
                    ContentType = command.ContentType
                };

                _dbContext.Documents.Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(entity.Id, _localizer[Constants.Messages.Saved]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Document {@0} {UserId}",
                    command, _userSession.UserId);
            }

            return Result<string>.Fail(_localizer[Constants.Messages.InternalError]);
        }
    }    

    public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
    {
        readonly IStringLocalizer _localizer;        

        public CreateDocumentCommandValidator(
            IStringLocalizer<CreateDocumentCommandValidator> localizer)
        {
            _localizer = localizer;

            RuleFor(_ => _.Title)
                .NotEmpty().WithMessage(_localizer["Title is required"])
                .MaximumLength(100).WithMessage(_localizer["Title cannot be longer than 100 characters"]);

            RuleFor(_ => _.Data)
                .NotEmpty().WithMessage(_localizer["Data is required"]);

            RuleFor(_ => _.ContentType)
                .NotEmpty().WithMessage(_localizer["Content Type is required"])
                .MaximumLength(50).WithMessage(_localizer["Content Type cannot be longer than 50 characters"]);
        }
    }
}
