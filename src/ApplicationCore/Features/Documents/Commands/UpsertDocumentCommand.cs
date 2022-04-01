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
    public partial class UpsertDocumentCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();        
        public string Title { get; set; } = "";        
        public string Description { get; set; } = "";                
        public string Data { get; set; } = "";        
        public string DocumentTypeId { get; set; } = "";        
    }

    internal class UpsertDocumentCommandHandler : IRequestHandler<UpsertDocumentCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;        
        readonly AppDbContext _dbContext;        
        
        public UpsertDocumentCommandHandler(
            ILogger<UpsertDocumentCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpsertDocumentCommandHandler> localizer,            
            AppDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;            
            _dbContext = dbContext;            
        }

        public async Task<Result<string>> Handle(
            UpsertDocumentCommand command,
            CancellationToken cancellationToken)
        {            
            if (string.IsNullOrEmpty(command.Id))
            {
                return await AddAsync(command, cancellationToken);
            }
            else
            {
                return await UpdateAsync(command, cancellationToken);
            }
        }

        private async Task<Result<string>> AddAsync(            
            UpsertDocumentCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = new Document()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = command.Title,
                    Description = command.Description,
                    Data = command.Data
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

            return Result<string>.Fail(_localizer["Internal Error"]);
        }

        private async Task<Result<string>> UpdateAsync(            
            UpsertDocumentCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.Documents.FindAsync(command.Id);

                if (entity == null)
                    return Result<string>.Fail(_localizer[Constants.Messages.NotFound]);

                entity.Title = command.Title;
                entity.Description = command.Description;
                entity.DocumentTypeId = command.DocumentTypeId;
                entity.Data = command.Data;

                _dbContext.Update(entity);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(entity.Id, _localizer[Constants.Messages.Saved]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Document {@0) {UserId}", 
                    command, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer[Constants.Messages.InternalError]);
        }
    }    

    public class UpsertDocumentCommandValidator : AbstractValidator<UpsertDocumentCommand>
    {
        readonly IStringLocalizer _localizer;        

        public UpsertDocumentCommandValidator(
            IStringLocalizer<UpsertDocumentCommandValidator> localizer)
        {
            _localizer = localizer;

            RuleFor(_ => _.Title)
                .NotEmpty().WithMessage(_localizer["Title is required"])
                .MaximumLength(100).WithMessage(_localizer["Title cannot be longer than 100 characters"]);

            RuleFor(_ => _.Data)
                .NotEmpty().WithMessage(_localizer["Data is required"]);

            RuleFor(_ => _.DocumentTypeId)
                .NotEmpty().WithMessage(_localizer["Document Type ID is required"]);
        }
    }
}
