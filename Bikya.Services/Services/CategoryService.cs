
using Bikya.Data.Repositories.Interfaces;
using Bikya.Data.Models;
using Bikya.Data.Response;
using Bikya.DTOs.CategoryDTOs;
using Bikya.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Services.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<ApiResponse<object>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            var (categories, totalCount) = await _categoryRepository.GetPaginatedAsync(page, pageSize, search);

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var result = categories.Select(ToCategoryDTO).ToList();

            var responseData = new
            {
                items = result,
                totalCount = totalCount,
                totalPages = totalPages,
                currentPage = page,
                pageSize = pageSize
            };

            return ApiResponse<object>.SuccessResponse(responseData, "Categories retrieved with pagination");
        }

        public async Task<ApiResponse<CategoryDTO>> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdWithProductsAsync(id);

            if (category == null)
                return ApiResponse<CategoryDTO>.ErrorResponse("Category not found", 404);

            return ApiResponse<CategoryDTO>.SuccessResponse(ToCategoryDTO(category), "Category retrieved successfully");
        }

        public async Task<ApiResponse<CategoryDTO>> GetByNameAsync(string name)
        {
            var category = await _categoryRepository.GetByNameWithProductsAsync(name);

            if (category == null)
                return ApiResponse<CategoryDTO>.ErrorResponse("Category not found", 404);

            return ApiResponse<CategoryDTO>.SuccessResponse(ToCategoryDTO(category), "Category retrieved successfully");
        }

        public async Task<ApiResponse<CategoryDTO>> AddAsync(CreateCategoryDTO dto)
        {
            // Check if a category with the same name already exists
            var nameExists = await _categoryRepository.ExistsByNameAsync(dto.Name);

            if (nameExists)
            {
                return ApiResponse<CategoryDTO>.ErrorResponse("Category name already exists", 400);
            }

            var category = ToCategoryFromCreateDTO(dto);
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return ApiResponse<CategoryDTO>.SuccessResponse(ToCategoryDTO(category), "Category created successfully", 201);
        }

        public async Task<ApiResponse<CategoryDTO>> UpdateAsync(int id, UpdateCategoryDTO dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return ApiResponse<CategoryDTO>.ErrorResponse("Category not found", 404);

            // Check for duplicate name in other categories
            var existsWithSameName = await _categoryRepository.ExistsByNameExcludingIdAsync(dto.Name, id);

            if (existsWithSameName)
                return ApiResponse<CategoryDTO>.ErrorResponse("Category name already exists", 400);

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.IconUrl = dto.IconUrl;

            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();

            return ApiResponse<CategoryDTO>.SuccessResponse(ToCategoryDTO(category), "Category updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return ApiResponse<bool>.ErrorResponse("Category not found", 404);

            _categoryRepository.Remove(category);
            await _categoryRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully");
        }

        private CategoryDTO ToCategoryDTO(Category category)
        {
            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                IconUrl = category.IconUrl,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
            };
        }

        private Category ToCategoryFromCreateDTO(CreateCategoryDTO dto)
        {
            return new Category
            {
                Name = dto.Name,
                IconUrl = dto.IconUrl,
                Description = dto.Description,
            };
        }
    }
}