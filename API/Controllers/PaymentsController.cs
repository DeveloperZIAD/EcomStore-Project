using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Business_layer;
using Data_layer; // لأن clspayment موجود هنا

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        /// <summary>
        /// Gets all payments (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult<List<clspayment>> GetAllPayments()
        {
            var payments = PaymentService.GetAllPayments();
            return Ok(payments);
        }

        /// <summary>
        /// Gets a specific payment by ID (Admin only)
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "admin")]
        public ActionResult<clspayment> GetPaymentById(int id)
        {
            try
            {
                var payment = PaymentService.GetPaymentById(id);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets payment by order ID (Admin only)
        /// </summary>
        [HttpGet("order/{orderId:int}")]
        [Authorize(Roles = "admin")]
        public ActionResult<clspayment> GetPaymentByOrderId(int orderId)
        {
            try
            {
                var payment = PaymentService.GetPaymentByOrderId(orderId);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates payment status by payment ID (Admin only)
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "admin")]
        public IActionResult UpdatePaymentStatus(int id, [FromBody] string newStatus)
        {
            try
            {
                bool success = PaymentService.UpdatePaymentStatus(id, newStatus);
                if (!success)
                    return NotFound(new { message = "Payment not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates payment status by order ID (Admin only)
        /// </summary>
        [HttpPatch("order/{orderId:int}/status")]
        [Authorize(Roles = "admin")]
        public IActionResult UpdatePaymentStatusByOrderId(int orderId, [FromBody] string newStatus)
        {
            try
            {
                bool success = PaymentService.UpdatePaymentStatusByOrderId(orderId, newStatus);
                if (!success)
                    return NotFound(new { message = "Payment not found for this order." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Webhook endpoint for payment gateway (no auth - use signature or IP whitelist in production)
        /// </summary>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public IActionResult PaymentWebhook([FromBody] dynamic payload)
        {
            try
            {
                // في الـ Production هتتحقق من signature البوابة
                int orderId = payload.order_id;
                string newStatus = payload.status;
                string transactionId = payload.transaction_id ?? null;

                bool success = PaymentService.UpdatePaymentStatusByOrderId(orderId, newStatus, transactionId);

                if (!success)
                    return NotFound("Payment not found.");

                AuditLogService.LogAction("Payment Webhook", $"Order ID: {orderId}, Status: {newStatus}, Transaction: {transactionId}");

                return Ok(new { message = "Webhook received." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}