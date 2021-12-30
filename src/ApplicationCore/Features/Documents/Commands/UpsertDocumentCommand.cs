using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Requests;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Features.Documents.Commands
{
    public partial class UpsertDocumentCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string Title { get; set; } = "";
        [Required]
        public string Description { get; set; } = "";
        public bool IsPublic { get; set; } = false;
        [Required]
        public string URL { get; set; } = "";
        [Required]
        public string DocumentTypeId { get; set; } = "";
        public UploadRequest? UploadRequest { get; set; }
    }

    internal class UpsertDocumentCommandHandler : IRequestHandler<UpsertDocumentCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSession _userSession;
        readonly IStringLocalizer<UpsertDocumentCommandHandler> _localizer;
        readonly IMapper _mapper;
        readonly AppDbContext _dbContext;
        readonly IFileService _fileService;
        
        public UpsertDocumentCommandHandler(
            ILogger<UpsertDocumentCommandHandler> logger,
            IUserSession userSession,
            IStringLocalizer<UpsertDocumentCommandHandler> localizer,
            IMapper mapper,
            AppDbContext dbContext,
            IFileService fileService)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _mapper = mapper;
            _dbContext = dbContext;
            _fileService = fileService;            
        }

        public async Task<Result<string>> Handle(
            UpsertDocumentCommand command,
            CancellationToken cancellationToken)
        {
            var uploadRequest = command.UploadRequest;

            if (uploadRequest != null)
            {
                uploadRequest.FileName = $"D-{Guid.NewGuid()}{uploadRequest.Extension}";
            }

            if (string.IsNullOrEmpty(command.Id))
            {
                return await AddAsync(uploadRequest, command, cancellationToken);
            }
            else
            {
                return await UpdateAsync(uploadRequest, command, cancellationToken);
            }
        }

        private async Task<Result<string>> AddAsync(
            UploadRequest? uploadRequest,
            UpsertDocumentCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var doc = _mapper.Map<Document>(command);

                if (uploadRequest != null)
                    doc.URL = await _fileService.UploadAsync(uploadRequest);

                doc.Id = Guid.NewGuid().ToString();

                _dbContext.Documents.Add(doc);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(doc.Id, _localizer["Document Saved"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Document {@0} {UserId}", 
                    command, _userSession.UserId);
            }

            return Result<string>.Fail(_localizer["Internal Error"]);
        }

        private async Task<Result<string>> UpdateAsync(
            UploadRequest? uploadRequest,
            UpsertDocumentCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var doc = await _dbContext.Documents.FindAsync(command.Id);

                if (doc == null)
                    return Result<string>.Fail(_localizer["Document Not Found!"]);

                doc.Title = command.Title ?? doc.Title;
                doc.Description = command.Description ?? doc.Description;
                doc.IsPublic = command.IsPublic;

                if (uploadRequest != null)
                {
                    doc.URL = await _fileService.UploadAsync(uploadRequest);
                }

                doc.DocumentTypeId = string.IsNullOrEmpty(command.DocumentTypeId) ? doc.DocumentTypeId : command.DocumentTypeId;

                _dbContext.Update(doc);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(doc.Id, _localizer["Document Updated"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Document {@0) {UserId}", 
                    command, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer["Internal Error"]);
        }
    }    
}
