// File: CategoryService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Data_layer;

namespace Business_layer
{
    public static class CategoryService
    {
        public static int AddCategory(clscategory category)
        {
            ValidateCategory(category);

            int newId = category_dal.AddCategory(category);

            if (newId <= 0)
                throw new Exception("Failed to add the category.");

            AuditLogService.LogAction("Category Created", $"Category ID: {newId}, Name: {category.name}");

            return newId;
        }

        public static List<clscategory> GetAllCategories()
        {
            return category_dal.GetAllCategories();
        }

        public static clscategory GetCategoryById(int categoryId)
        {
            if (categoryId <= 0)
                throw new ArgumentException("Invalid category ID.");

            var category = category_dal.GetCategoryById(categoryId);

            if (category == null)
                throw new KeyNotFoundException("Category not found.");

            return category;
        }

        public static bool UpdateCategory(int categoryId, clscategory category)
        {
            if (categoryId <= 0)
                throw new ArgumentException("Invalid category ID.");

            ValidateCategory(category);

            var existing = category_dal.GetCategoryById(categoryId);

            if (existing == null)
                throw new KeyNotFoundException("Category not found.");

            existing.name = category.name.Trim();
            existing.description = string.IsNullOrWhiteSpace(category.description) ? null : category.description.Trim();

            bool success = category_dal.UpdateCategory(existing);

            if (success)
            {
                AuditLogService.LogAction("Category Updated", $"Category ID: {categoryId}, New Name: {category.name}");
            }

            return success;
        }

        public static bool DeleteCategory(int categoryId)
        {
            if (categoryId <= 0)
                throw new ArgumentException("Invalid category ID.");

            var category = category_dal.GetCategoryById(categoryId);

            if (category == null)
                throw new KeyNotFoundException("Category not found.");

            if (category_dal.HasProducts(categoryId))
                throw new InvalidOperationException("Cannot delete category because it has associated products.");

            bool success = category_dal.DeleteCategory(categoryId);

            if (success)
            {
                AuditLogService.LogAction("Category Deleted", $"Category ID: {categoryId}, Name: {category.name}");
            }

            return success;
        }

        private static void ValidateCategory(clscategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));
            if (string.IsNullOrWhiteSpace(category.name))
                throw new ArgumentException("Category name is required.");
        }
    }
}