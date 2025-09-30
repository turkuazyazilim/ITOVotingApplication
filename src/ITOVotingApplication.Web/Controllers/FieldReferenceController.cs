using AutoMapper;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.FieldReference;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FieldReferenceController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FieldReferenceController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<ApiResponse<List<FieldReferenceCategoryDto>>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _unitOfWork.FieldReferenceCategories.Query()
                    .Where(c => c.IsActive)
                    .Include(c => c.SubCategories.Where(sc => sc.IsActive))
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();

                var result = categories.Select(c => new FieldReferenceCategoryDto
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    SubCategoryCount = c.SubCategories.Count(sc => sc.IsActive),
                    SubCategories = c.SubCategories.Where(sc => sc.IsActive).Select(sc => new FieldReferenceSubCategoryDto
                    {
                        Id = sc.Id,
                        CategoryId = sc.CategoryId,
                        CategoryName = c.CategoryName,
                        SubCategoryName = sc.SubCategoryName,
                        Description = sc.Description,
                        IsActive = sc.IsActive
                    }).ToList()
                }).ToList();

                return ApiResponse<List<FieldReferenceCategoryDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<FieldReferenceCategoryDto>>.ErrorResult($"Kategoriler getirme hatası: {ex.Message}");
            }
        }

        [HttpGet("categories/{id}/subcategories")]
        [AllowAnonymous]
        public async Task<ApiResponse<List<FieldReferenceSubCategoryDto>>> GetSubCategoriesAsync(int id)
        {
            try
            {
                var subCategories = await _unitOfWork.FieldReferenceSubCategories.Query()
                    .Include(sc => sc.Category)
                    .Where(sc => sc.CategoryId == id && sc.IsActive)
                    .OrderBy(sc => sc.SubCategoryName)
                    .ToListAsync();

                var result = subCategories.Select(sc => new FieldReferenceSubCategoryDto
                {
                    Id = sc.Id,
                    CategoryId = sc.CategoryId,
                    CategoryName = sc.Category.CategoryName,
                    SubCategoryName = sc.SubCategoryName,
                    Description = sc.Description,
                    IsActive = sc.IsActive
                }).ToList();

                return ApiResponse<List<FieldReferenceSubCategoryDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<FieldReferenceSubCategoryDto>>.ErrorResult($"Alt kategoriler getirme hatası: {ex.Message}");
            }
        }

        [HttpPost("categories")]
        public async Task<ApiResponse<FieldReferenceCategoryDto>> CreateCategoryAsync(CreateFieldReferenceCategoryDto dto)
        {
            try
            {
                // Check for duplicates
                var existingCategory = await _unitOfWork.FieldReferenceCategories.Query()
                    .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == dto.CategoryName.ToLower());

                if (existingCategory != null)
                {
                    return ApiResponse<FieldReferenceCategoryDto>.ErrorResult("Bu kategori adı zaten mevcut!");
                }

                var category = new FieldReferenceCategory
                {
                    CategoryName = dto.CategoryName,
                    Description = dto.Description ?? "",
                    IsActive = dto.IsActive
                };

                await _unitOfWork.FieldReferenceCategories.AddAsync(category);
                await _unitOfWork.CompleteAsync();

                var result = new FieldReferenceCategoryDto
                {
                    Id = category.Id,
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                    IsActive = category.IsActive,
                    SubCategoryCount = 0,
                    SubCategories = new List<FieldReferenceSubCategoryDto>()
                };

                return ApiResponse<FieldReferenceCategoryDto>.SuccessResult(result, "Kategori başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                return ApiResponse<FieldReferenceCategoryDto>.ErrorResult($"Kategori ekleme hatası: {ex.Message}");
            }
        }

        [HttpPost("subcategories")]
        public async Task<ApiResponse<FieldReferenceSubCategoryDto>> CreateSubCategoryAsync(CreateFieldReferenceSubCategoryDto dto)
        {
            try
            {
                // Check if category exists
                var category = await _unitOfWork.FieldReferenceCategories.GetByIdAsync(dto.CategoryId);
                if (category == null)
                {
                    return ApiResponse<FieldReferenceSubCategoryDto>.ErrorResult("Üst kategori bulunamadı!");
                }

                // Check for duplicates within the same category
                var existingSubCategory = await _unitOfWork.FieldReferenceSubCategories.Query()
                    .FirstOrDefaultAsync(sc => sc.CategoryId == dto.CategoryId &&
                                             sc.SubCategoryName.ToLower() == dto.SubCategoryName.ToLower());

                if (existingSubCategory != null)
                {
                    return ApiResponse<FieldReferenceSubCategoryDto>.ErrorResult("Bu üst kategoride aynı alt kategori adı mevcut!");
                }

                var subCategory = new FieldReferenceSubCategory
                {
                    CategoryId = dto.CategoryId,
                    SubCategoryName = dto.SubCategoryName,
                    Description = dto.Description ?? "",
                    IsActive = dto.IsActive
                };

                await _unitOfWork.FieldReferenceSubCategories.AddAsync(subCategory);
                await _unitOfWork.CompleteAsync();

                var result = new FieldReferenceSubCategoryDto
                {
                    Id = subCategory.Id,
                    CategoryId = subCategory.CategoryId,
                    CategoryName = category.CategoryName,
                    SubCategoryName = subCategory.SubCategoryName,
                    Description = subCategory.Description,
                    IsActive = subCategory.IsActive
                };

                return ApiResponse<FieldReferenceSubCategoryDto>.SuccessResult(result, "Alt kategori başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                return ApiResponse<FieldReferenceSubCategoryDto>.ErrorResult($"Alt kategori ekleme hatası: {ex.Message}");
            }
        }

        [HttpDelete("categories/{id}")]
        public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _unitOfWork.FieldReferenceCategories.Query()
                    .Include(c => c.SubCategories)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return ApiResponse<bool>.ErrorResult("Kategori bulunamadı!");
                }

                // Check if any subcategories are linked to user roles
                var linkedUserRoles = await _unitOfWork.UserRoles.Query()
                    .Where(ur => ur.FieldReferenceCategoryId == id ||
                                category.SubCategories.Any(sc => sc.Id == ur.FieldReferenceSubCategoryId))
                    .ToListAsync();

                if (linkedUserRoles.Any())
                {
                    return ApiResponse<bool>.ErrorResult("Bu kategori kullanıcılar tarafından kullanılıyor, silinemez!");
                }

                // Manually remove subcategories first (in case cascade delete doesn't work)
                if (category.SubCategories != null && category.SubCategories.Any())
                {
                    foreach (var subCategory in category.SubCategories.ToList())
                    {
                        _unitOfWork.FieldReferenceSubCategories.Remove(subCategory);
                    }
                }

                // Remove the main category
                _unitOfWork.FieldReferenceCategories.Remove(category);
                await _unitOfWork.CompleteAsync();

                return ApiResponse<bool>.SuccessResult(true, "Kategori başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Kategori silme hatası: {ex.Message}");
            }
        }

        [HttpDelete("subcategories/{id}")]
        public async Task<ApiResponse<bool>> DeleteSubCategoryAsync(int id)
        {
            try
            {
                var subCategory = await _unitOfWork.FieldReferenceSubCategories.GetByIdAsync(id);
                if (subCategory == null)
                {
                    return ApiResponse<bool>.ErrorResult("Alt kategori bulunamadı!");
                }

                // Check if linked to user roles
                var linkedUserRoles = await _unitOfWork.UserRoles.Query()
                    .Where(ur => ur.FieldReferenceSubCategoryId == id)
                    .ToListAsync();

                if (linkedUserRoles.Any())
                {
                    return ApiResponse<bool>.ErrorResult("Bu alt kategori kullanıcılar tarafından kullanılıyor, silinemez!");
                }

                _unitOfWork.FieldReferenceSubCategories.Remove(subCategory);
                await _unitOfWork.CompleteAsync();

                return ApiResponse<bool>.SuccessResult(true, "Alt kategori başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Alt kategori silme hatası: {ex.Message}");
            }
        }
    }
}