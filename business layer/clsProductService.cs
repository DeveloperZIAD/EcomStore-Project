// File: ProductService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Data_layer;
using Business_layer.Dtos;

namespace Business_layer
{
    public static class ProductService
    {
        /// <summary>
        /// Adds a new product (admin only)
        /// </summary>
        public static int AddProduct(clsproduct product)
        {
            ValidateProduct(product);

            int newId = productDal.AddProduct(product);

            if (newId <= 0)
                throw new Exception("Failed to add the product.");

            AuditLogService.LogAction("Product Created", $"Product ID: {newId}, Name: {product.name}, Price: {product.price}");

            return newId;
        }

        public static List<ProductResponseDto> GetAllProducts()
        {
            var dbProducts = productDal.GetAllProducts();

            return dbProducts
                .Select(p => MapToResponseDto(p))
                .ToList();
        }

        public static List<ProductResponseDto> GetProductsByCategoryId(int categoryId)
        {
            if (categoryId <= 0)
                throw new ArgumentException("Invalid category ID.");

            var dbProducts = productDal.GetProductsByCategoryId(categoryId);

            return dbProducts
                .Select(p => MapToResponseDto(p))
                .ToList();
        }

        public static List<ProductResponseDto> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllProducts();

            var dbProducts = productDal.SearchProducts(searchTerm.Trim());

            return dbProducts
                .Select(p => MapToResponseDto(p))
                .ToList();
        }

        public static ProductResponseDto GetProductById(int productId)
        {
            if (productId <= 0)
                throw new ArgumentException("Invalid product ID.");

            var product = productDal.GetProductById(productId);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            return MapToResponseDto(product); // نفس الميثود!
        }

        /// <summary>
        /// Updates an existing product (admin only)
        /// </summary>
        public static bool UpdateProduct(int productId, clsproduct product)
        {
            if (productId <= 0)
                throw new ArgumentException("Invalid product ID.");

            ValidateProduct(product);

            var existing = productDal.GetProductById(productId);

            if (existing == null)
                throw new KeyNotFoundException("Product not found.");

            existing.name = product.name.Trim();
            existing.description = string.IsNullOrWhiteSpace(product.description) ? null : product.description.Trim();
            existing.price = product.price;
            existing.stock = product.stock;
            existing.category_id = product.category_id;
            existing.image_url = string.IsNullOrWhiteSpace(product.image_url) ? null : product.image_url.Trim();

            bool success = productDal.UpdateProduct(existing);

            if (success)
            {
                AuditLogService.LogAction("Product Updated", $"Product ID: {productId}, New Name: {product.name}, New Price: {product.price}");
            }

            return success;
        }
        /// <summary>
        /// Updates product stock directly (admin or inventory management)
        /// </summary>
        public static bool UpdateProductStock(int productId, int newStock)
        {
            if (productId <= 0)
                throw new ArgumentException("Invalid product ID.");

            if (newStock < 0)
                throw new ArgumentException("Stock cannot be negative.");

            var product = productDal.GetProductById(productId);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            bool success = productDal.UpdateStock(productId, newStock);

            if (success)
            {
                AuditLogService.LogAction("Product Stock Updated", $"Product ID: {productId}, New Stock: {newStock}");
            }

            return success;
        }

        /// <summary>
        /// Deletes a product (admin only)
        /// </summary>
        public static bool DeleteProduct(int productId)
        {
            if (productId <= 0)
                throw new ArgumentException("Invalid product ID.");

            var product = productDal.GetProductById(productId);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            bool success = productDal.DeleteProduct(productId);

            if (success)
            {
                AuditLogService.LogAction("Product Deleted", $"Product ID: {productId}, Name: {product.name}");
            }

            return success;
        }

        // ----------------- Private Helper Methods -----------------

        private static void ValidateAddProductDto(AddProductRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            ValidateProductFields(dto.Name, dto.Price, dto.Stock);
        }

        private static void ValidateUpdateProductDto(UpdateProductRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            ValidateProductFields(dto.Name, dto.Price, dto.Stock);
        }

        private static void ValidateProductFields(string name, decimal price, int stock)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required.");

            if (price < 0)
                throw new ArgumentException("Price cannot be negative.");

            if (stock < 0)
                throw new ArgumentException("Stock cannot be negative.");
        }

        // Mapping from ProductSummaryDto (used by GetAllProducts, Search, ByCategory)
        // ----------------- Private Helper Method -----------------
        private static void ValidateProduct(clsproduct product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrWhiteSpace(product.name))
                throw new ArgumentException("Product name is required.", nameof(product.name));

            if (product.price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(product.price));

            if (product.stock < 0)
                throw new ArgumentException("Stock cannot be negative.", nameof(product.stock));
        }
        private static ProductResponseDto MapToResponseDto(object source)
        {
            ProductResponseDto dto = new ProductResponseDto();

            if (source is ProductSummaryDto summary)
            {
                dto.Id = summary.Id;
                dto.Name = summary.Name;
                dto.Description = summary.Description;
                dto.Price = summary.Price;
                dto.Stock = summary.Stock;
                dto.CategoryName = summary.CategoryName;
                dto.ImageUrl = summary.ImageUrl;
                dto.CreatedAt = summary.CreatedAt;
            }
            else if (source is clsproduct product)
            {
                dto.Id = product.id;
                dto.Name = product.name;
                dto.Description = product.description;
                dto.Price = product.price;
                dto.Stock = product.stock;
                dto.ImageUrl = product.image_url;
                dto.CreatedAt = product.created_at;

                // جلب اسم الفئة إذا كان موجود category_id
                dto.CategoryName = product.category_id.HasValue
                    ? category_dal.GetCategoryById(product.category_id.Value)?.name
                    : null;
            }
            else
            {
                throw new ArgumentException("Unsupported source type for product mapping.");
            }

            return dto;
        }
    }
}