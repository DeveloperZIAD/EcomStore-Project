// File: ProductsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Business_layer;
using Data_layer; // لأن clsproduct موجود هنا

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        /// <summary>
        /// Gets all products with category name (Public - no login required)
        /// </summary>
        [HttpGet]
        public ActionResult<List<clsproduct>> GetAllProducts()
        {
            var products = ProductService.GetAllProducts();
            return Ok(products);
        }

        /// <summary>
        /// Gets a specific product by ID (Public)
        /// </summary>
        [HttpGet("{id:int}")]
        public ActionResult<clsproduct> GetProductById(int id)
        {
            try
            {
                var product = ProductService.GetProductById(id);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Searches products by name or description (Public)
        /// </summary>
        [HttpGet("search")]
        public ActionResult<List<clsproduct>> SearchProducts([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Search query is required." });

            var products = ProductService.SearchProducts(q);
            return Ok(products);
        }

        /// <summary>
        /// Gets products by category ID (Public)
        /// </summary>
        [HttpGet("category/{categoryId:int}")]
        public ActionResult<List<clsproduct>> GetProductsByCategoryId(int categoryId)
        {
            try
            {
                var products = ProductService.GetProductsByCategoryId(categoryId);
                return Ok(products);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ====================== Admin Only Endpoints ======================

        /// <summary>
        /// Adds a new product (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult<int> AddProduct([FromBody] clsproduct product)
        {
            try
            {
                int newProductId = ProductService.AddProduct(product);
                return CreatedAtAction(nameof(GetProductById), new { id = newProductId }, new { id = newProductId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing product (Admin only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult UpdateProduct(int id, [FromBody] clsproduct product)
        {
            try
            {
                bool success = ProductService.UpdateProduct(id, product);
                if (!success)
                    return NotFound(new { message = "Product not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates product stock (Admin only)
        /// </summary>
        [HttpPatch("{id:int}/stock")]
        [Authorize(Roles = "admin")]
        public IActionResult UpdateProductStock(int id, [FromBody] int newStock)
        {
            try
            {
                bool success = ProductService.UpdateProductStock(id, newStock);
                if (!success)
                    return NotFound(new { message = "Product not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a product (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                bool success = ProductService.DeleteProduct(id);
                if (!success)
                    return NotFound(new { message = "Product not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}