// File: OrdersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Business_layer;
using Data_layer; // لأن clsorder موجود هنا
using System.Collections.Generic;
using Business_layer.Dtos;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        /// <summary>
        /// Gets all orders for the authenticated user (customer view)
        /// </summary>
        [HttpGet("my")]
        [Authorize]
        public ActionResult<List<clsorder>> GetMyOrders()
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var orders = OrderService.GetOrdersByUserId(currentUserId);
            return Ok(orders);
        }

        /// <summary>
        /// Gets a specific order by ID for the authenticated user
        /// </summary>
        [HttpGet("my/{id:int}")]
        [Authorize]
        public ActionResult<clsorder> GetMyOrderById(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var order = OrderService.GetOrderById(id, currentUserId);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Gets all orders in the system (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult<List<dynamic>> GetAllOrdersAdmin()
        {
            var orders = OrderService.GetAllOrdersAdmin();
            return Ok(orders);
        }

        /// <summary>
        /// Gets full details of an order including items, payment, address, and user (Admin only)
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "admin")]
        public ActionResult<FullOrderDetailsDto> GetOrderDetails(int id)
        {
            try
            {
                var details = OrderService.GetOrderDetails(id);

                if (details == null)
                    return NotFound(new { message = "Order not found." });

                return Ok(details);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates order status (Admin only)
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "admin")]
        public IActionResult UpdateOrderStatus(int id, [FromBody] string newStatus)
        {
            try
            {
                bool success = OrderService.UpdateOrderStatus(id, newStatus);
                if (!success)
                    return NotFound(new { message = "Order not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cancels an order (Customer can cancel his own pending order)
        /// </summary>
        [HttpPatch("{id:int}/cancel")]
        [Authorize]
        public IActionResult CancelOrder(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                bool success = OrderService.CancelOrder(id, currentUserId);
                if (!success)
                    return NotFound(new { message = "Order not found or cannot be cancelled." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}