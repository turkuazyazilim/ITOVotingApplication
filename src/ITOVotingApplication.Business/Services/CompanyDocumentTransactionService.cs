using AutoMapper;
using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.CompanyDocument;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Enums;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Business.Services
{
    public class CompanyDocumentTransactionService : ICompanyDocumentTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string _webRootPath;

        public CompanyDocumentTransactionService(IUnitOfWork unitOfWork, IMapper mapper, Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _webRootPath = webHostEnvironment.WebRootPath ?? webHostEnvironment.ContentRootPath;
        }

        public async Task<ApiResponse<CompanyDocumentTransactionDto>> CreateAsync(CreateDocumentTransactionDto dto, int userId)
        {
            try
            {
                var transaction = new CompanyDocumentTransaction
                {
                    CompanyId = dto.CompanyId,
                    DocumentType = dto.DocumentType,
                    DocumentUrl = dto.DocumentUrl,
                    UploadDate = DateTime.Now,
                    UploadedByUserId = userId,
                    WillParticipateInElection = dto.WillParticipateInElection,
                    AssignedContactId = dto.AssignedContactId,
                    CreatedDate = DateTime.Now
                };

                await _unitOfWork.CompanyDocumentTransactions.AddAsync(transaction);
                await _unitOfWork.CompleteAsync();

                var resultDto = await GetByIdAsync(transaction.Id);
                return resultDto;
            }
            catch (Exception ex)
            {
                return ApiResponse<CompanyDocumentTransactionDto>.ErrorResult($"Belge kaydı oluşturulamadı: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CompanyDocumentTransactionDto>> UpdateDeliveryStatusAsync(UpdateDeliveryStatusDto dto)
        {
            try
            {
                var transaction = await _unitOfWork.CompanyDocumentTransactions.GetByIdAsync(dto.TransactionId);
                if (transaction == null)
                {
                    return ApiResponse<CompanyDocumentTransactionDto>.ErrorResult("Belge kaydı bulunamadı");
                }

                transaction.DeliveryStatus = dto.DeliveryStatus;
                transaction.DeliveryStatusDate = DateTime.Now;
                transaction.RejectionReason = dto.RejectionReason;
                transaction.RejectionNote = dto.RejectionNote;
                transaction.UpdatedDate = DateTime.Now;

                _unitOfWork.CompanyDocumentTransactions.Update(transaction);
                await _unitOfWork.CompleteAsync();

                var resultDto = await GetByIdAsync(transaction.Id);
                return resultDto;
            }
            catch (Exception ex)
            {
                return ApiResponse<CompanyDocumentTransactionDto>.ErrorResult($"Teslim durumu güncellenemedi: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CompanyDocumentTransactionDto>> AssignToContactAsync(int transactionId, int contactId)
        {
            try
            {
                var transaction = await _unitOfWork.CompanyDocumentTransactions.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    return ApiResponse<CompanyDocumentTransactionDto>.ErrorResult("Belge kaydı bulunamadı");
                }

                // Verify contact exists and is eligible to vote
                var contact = await _unitOfWork.Contacts.GetByIdAsync(contactId);
                if (contact == null)
                {
                    return ApiResponse<CompanyDocumentTransactionDto>.ErrorResult("Yetkili kişi bulunamadı");
                }

                transaction.AssignedContactId = contactId;
                transaction.UpdatedDate = DateTime.Now;

                _unitOfWork.CompanyDocumentTransactions.Update(transaction);
                await _unitOfWork.CompleteAsync();

                var resultDto = await GetByIdAsync(transaction.Id);
                return resultDto;
            }
            catch (Exception ex)
            {
                return ApiResponse<CompanyDocumentTransactionDto>.ErrorResult($"Belge ataması yapılamadı: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int transactionId)
        {
            try
            {
                var transaction = await _unitOfWork.CompanyDocumentTransactions.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    return ApiResponse<bool>.ErrorResult("Belge kaydı bulunamadı");
                }

                // Delete physical file (if exists)
                if (!string.IsNullOrEmpty(transaction.DocumentUrl))
                {
                    string physicalPath;

                    // Web URL'yi fiziksel path'e çevir
                    if (transaction.DocumentUrl.StartsWith("/Documents/"))
                    {
                        // Web URL: /Documents/... -> Physical Path: wwwroot/Documents/...
                        physicalPath = Path.Combine(_webRootPath, transaction.DocumentUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    }
                    else
                    {
                        // TC firmaları için dummy path veya eski kayıtlar için direkt path
                        physicalPath = transaction.DocumentUrl;
                    }

                    if (File.Exists(physicalPath))
                    {
                        try
                        {
                            File.Delete(physicalPath);
                        }
                        catch
                        {
                            // Dosya silinemese bile devam et
                        }
                    }
                }

                _unitOfWork.CompanyDocumentTransactions.Remove(transaction);
                await _unitOfWork.CompleteAsync();

                return ApiResponse<bool>.SuccessResult(true, "Belge kaydı başarıyla silindi");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Belge kaydı silinemedi: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CompanyDocumentTransactionDto>> GetByIdAsync(int id)
        {
            try
            {
                var transaction = await _unitOfWork.CompanyDocumentTransactions.Query()
                    .Include(t => t.Company)
                    .Include(t => t.UploadedByUser)
                    .Include(t => t.AssignedContact)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (transaction == null)
                {
                    return ApiResponse<CompanyDocumentTransactionDto>.ErrorResult("Belge kaydı bulunamadı");
                }

                var dto = MapToDto(transaction);
                return ApiResponse<CompanyDocumentTransactionDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<CompanyDocumentTransactionDto>.ErrorResult($"Belge kaydı getirilemedi: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<CompanyDocumentTransactionDto>>> GetByCompanyIdAsync(int companyId)
        {
            try
            {
                var transactions = await _unitOfWork.CompanyDocumentTransactions.Query()
                    .Include(t => t.Company)
                    .Include(t => t.UploadedByUser)
                    .Include(t => t.AssignedContact)
                    .Where(t => t.CompanyId == companyId)
                    .OrderByDescending(t => t.CreatedDate)
                    .ToListAsync();

                var dtos = transactions.Select(MapToDto).ToList();
                return ApiResponse<List<CompanyDocumentTransactionDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CompanyDocumentTransactionDto>>.ErrorResult($"Belge kayıtları getirilemedi: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CompanyDocumentTransactionDto>> GetLatestByCompanyAndTypeAsync(int companyId, CompanyDocumentType documentType)
        {
            try
            {
                var transaction = await _unitOfWork.CompanyDocumentTransactions.Query()
                    .Include(t => t.Company)
                    .Include(t => t.UploadedByUser)
                    .Include(t => t.AssignedContact)
                    .Where(t => t.CompanyId == companyId && t.DocumentType == documentType)
                    .OrderByDescending(t => t.CreatedDate)
                    .FirstOrDefaultAsync();

                if (transaction == null)
                {
                    return ApiResponse<CompanyDocumentTransactionDto>.ErrorResult("Belge kaydı bulunamadı");
                }

                var dto = MapToDto(transaction);
                return ApiResponse<CompanyDocumentTransactionDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<CompanyDocumentTransactionDto>.ErrorResult($"Belge kaydı getirilemedi: {ex.Message}");
            }
        }

        private CompanyDocumentTransactionDto MapToDto(CompanyDocumentTransaction transaction)
        {
            return new CompanyDocumentTransactionDto
            {
                Id = transaction.Id,
                CompanyId = transaction.CompanyId,
                DocumentType = transaction.DocumentType,
                DocumentTypeName = GetDocumentTypeName(transaction.DocumentType),
                DocumentUrl = transaction.DocumentUrl,
                UploadDate = transaction.UploadDate,
                UploadedByUserId = transaction.UploadedByUserId,
                UploadedByUserName = transaction.UploadedByUser != null
                    ? $"{transaction.UploadedByUser.FirstName} {transaction.UploadedByUser.LastName}"
                    : "",
                DeliveryStatus = transaction.DeliveryStatus,
                DeliveryStatusName = transaction.DeliveryStatus.HasValue
                    ? GetDeliveryStatusName(transaction.DeliveryStatus.Value)
                    : null,
                DeliveryStatusDate = transaction.DeliveryStatusDate,
                RejectionReason = transaction.RejectionReason,
                RejectionReasonName = transaction.RejectionReason.HasValue
                    ? GetRejectionReasonName(transaction.RejectionReason.Value)
                    : null,
                RejectionNote = transaction.RejectionNote,
                WillParticipateInElection = transaction.WillParticipateInElection,
                AssignedContactId = transaction.AssignedContactId,
                AssignedContactName = transaction.AssignedContact != null
                    ? $"{transaction.AssignedContact.FirstName} {transaction.AssignedContact.LastName}"
                    : null,
                CreatedDate = transaction.CreatedDate,
                UpdatedDate = transaction.UpdatedDate
            };
        }

        private string GetDocumentTypeName(CompanyDocumentType type)
        {
            return type switch
            {
                CompanyDocumentType.YetkiBelgesiTalepDilekçesi => "Yetki Belgesi Talep Dilekçesi",
                CompanyDocumentType.OnaylanmisYetkiBelgesi => "Onaylanmış Yetki Belgesi",
                _ => type.ToString()
            };
        }

        private string GetDeliveryStatusName(DocumentDeliveryStatus status)
        {
            return status switch
            {
                DocumentDeliveryStatus.TeslimEdildi => "Teslim Edildi",
                DocumentDeliveryStatus.TeslimAlindi => "Teslim Alındı (Onaylı)",
                DocumentDeliveryStatus.RedEdildi => "Red Edildi",
                _ => status.ToString()
            };
        }

        private string GetRejectionReasonName(DocumentRejectionReason reason)
        {
            return reason switch
            {
                DocumentRejectionReason.ImzaUyusmadi => "İmza Uyuşmadı",
                DocumentRejectionReason.BorcuVar => "Borcu Var",
                DocumentRejectionReason.MusterekImzaGerekli => "Müşterek İmza Gerekli",
                _ => reason.ToString()
            };
        }
    }
}
