﻿
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

        public async Task<ApiResponse<PaginatedCategoryResponse>> GetPaginatedAsync(int page = 1, int pageSize = 9, string? search = null)
        {
            var (categories, totalCount) = await _categoryRepository.GetPaginatedAsync(page, pageSize, search);

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var result = categories.Select(ToCategoryDTO).ToList();

            var response = new PaginatedCategoryResponse
            {
                Items = result,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
            return ApiResponse<PaginatedCategoryResponse>.SuccessResponse(response, "Categories retrieved with pagination");
        }

        public async Task<ApiResponse<List<CategoryDTO>>> GetAllAsync(string? search = null)
        {
            var categories = await _categoryRepository.GetAllAsync(search);
            var result = categories.Select(ToCategoryDTO).ToList();

            return ApiResponse<List<CategoryDTO>>.SuccessResponse(result, "Categories retrieved successfully");
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

        public async Task<ApiResponse<int>> CreateBulkAsync(List<CreateCategoryDTO> dtos)
        {
            if (dtos == null || !dtos.Any())
            {
                return ApiResponse<int>.ErrorResponse("No categories provided.", 404);
            }

            var categories = dtos.Select(dto => new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                IconUrl = dto.IconUrl,
                ParentCategoryId = dto.ParentCategoryId,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _categoryRepository.AddRangeAsync(categories);

            return ApiResponse<int>.SuccessResponse(categories.Count, "Categories created successfully.");
        }

        public async Task<ApiResponse<CategoryDTO>> AddAsync(CreateCategoryDTO dto)
        {
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

            var existsWithSameName = await _categoryRepository.ExistsByNameExcludingIdAsync(dto.Name, id);

            if (existsWithSameName)
                return ApiResponse<CategoryDTO>.ErrorResponse("Category name already exists", 400);

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.IconUrl = dto.IconUrl;
            category.ParentCategoryId = dto.ParentCategoryId;

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
                ParentName = category.ParentCategory?.Name // ✅ نعرض اسم الكاتيجوري الأب
            };
        }

        private Category ToCategoryFromCreateDTO(CreateCategoryDTO dto)
        {
            return new Category
            {
                Name = dto.Name,
                IconUrl = dto.IconUrl,
                Description = dto.Description,
                ParentCategoryId = dto.ParentCategoryId // ✅ مهم جدًا
            };
        }
    }
}