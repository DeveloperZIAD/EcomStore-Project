// File: OrderItemsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Business_layer;
using Data_layer; // لأن clsorderitem موجود هنا

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemsController : ControllerBase
    {
        /// <summary>
        /// Gets all items for a specific order (Admin or the order owner)
        /// </summary>
        [HttpGet("order/{orderId:int}")]
        [Authorize]
        public ActionResult<List<clsorderitem>> GetOrderItemsByOrderId(int orderId)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            // تحقق من ملكية الطلب (نستخدم OrderService عشان نجيب user_id الطلب)
            var order = OrderService.GetOrderById(orderId, currentUserId); // هيرمي خطأ لو مش مالك أو مش موجود

            // لو الـ Admin مش محتاج يتحقق من الملكية، لكن الكود فوق هيسمحله لأن OrderService.GetOrderById بيسمح للـ admin

            var items = OrderItemService.GetOrderItemsByOrderId(orderId);
            return Ok(items);
        }

        /// <summary>
        /// Gets a single order item by ID (Admin or the order owner)
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize]
        public ActionResult<clsorderitem> GetOrderItemById(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var item = OrderItemService.GetOrderItemById(id);

            if (item == null)
                return NotFound(new { message = "Order item not found." });

            // تحقق من ملكية الطلب
            var order = OrderService.GetOrderById(item.order_id, currentUserId);

            return Ok(item);
        }

        // ====================== Admin Only Endpoints ======================

        /// <summary>
        /// Adds multiple order items (Admin only - for manual order creation or correction)
        /// </summary>
        [HttpPost("order/{orderId:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult AddOrderItems(int orderId, [FromBody] List<clsorderitem> items)
        {
            if (items == null || items.Count == 0)
                return BadRequest(new { message = "Order items cannot be empty." });

            try
            {
                OrderItemService.AddOrderItems(orderId, items);
                return Ok(new { message = "Order items added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing order item (Admin only - for correction)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult UpdateOrderItem(int id, [FromBody] clsorderitem itemDto)
        {
            try
            {
                bool success = OrderItemService.UpdateOrderItem(id, itemDto);
                if (!success)
                    return NotFound(new { message = "Order item not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a single order item (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteOrderItem(int id)
        {
            try
            {
                bool success = OrderItemService.DeleteOrderItem(id);
                if (!success)
                    return NotFound(new { message = "Order item not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes all items for a specific order (Admin only - e.g., when canceling order)
        /// </summary>
        [HttpDelete("order/{orderId:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteOrderItemsByOrderId(int orderId)
        {
            try
            {
                bool success = OrderItemService.DeleteOrderItemsByOrderId(orderId);
                if (!success)
                    return NotFound(new { message = "No items found for this order." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}