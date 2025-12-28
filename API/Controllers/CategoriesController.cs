using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Business_layer;
using Data_layer; // عشان clscategory موجود هنا

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        /// <summary>
        /// Gets all categories (Public - no login required)
        /// </summary>
        [HttpGet]
        public ActionResult<List<clscategory>> GetAllCategories()
        {
            var categories = CategoryService.GetAllCategories();
            return Ok(categories);
        }

        /// <summary>
        /// Gets a specific category by ID (Public)
        /// </summary>
        [HttpGet("{id:int}")]
        public ActionResult<clscategory> GetCategoryById(int id)
        {
            try
            {
                var category = CategoryService.GetCategoryById(id);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ====================== Admin Only Endpoints ======================

        /// <summary>
        /// Adds a new category (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult<int> AddCategory([FromBody] clscategory category)
        {
            try
            {
                int newCategoryId = CategoryService.AddCategory(category);
                return CreatedAtAction(nameof(GetCategoryById), new { id = newCategoryId }, new { id = newCategoryId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing category (Admin only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult UpdateCategory(int id, [FromBody] clscategory category)
        {
            try
            {
                bool success = CategoryService.UpdateCategory(id, category);
                if (!success)
                    return NotFound(new { message = "Category not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a category (Admin only - only if no products)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteCategory(int id)
        {
            try
            {
                bool success = CategoryService.DeleteCategory(id);
                if (!success)
                    return NotFound(new { message = "Category not found or has associated products." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}