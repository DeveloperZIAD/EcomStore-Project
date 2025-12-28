using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Business_layer;
using Data_layer; // لأن clsaddressesdb هنا

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressesController : ControllerBase
    {
        [HttpGet("user/{userId:int}")]
        [Authorize]
        public ActionResult<List<clsaddressesdb>> GetAddressesByUserId(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (currentUserId != userId && currentUserRole != "admin")
                return Forbid("You can only view your own addresses.");

            var addresses = AddressService.GetAddressesByUserId(userId);
            return Ok(addresses);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public ActionResult<clsaddressesdb> GetAddressById(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var address = AddressService.GetAddressById(id, currentUserId);
                return Ok(address);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("user/{userId:int}")]
        [Authorize]
        public ActionResult<int> AddAddress(int userId, [FromBody] clsaddressesdb addressDto)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (currentUserId != userId && currentUserRole != "admin")
                return Forbid("You can only add addresses to your own account.");

            try
            {
                int newAddressId = AddressService.AddAddress(userId, addressDto);
                return CreatedAtAction(nameof(GetAddressById), new { id = newAddressId }, new { id = newAddressId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public IActionResult UpdateAddress(int id, [FromBody] clsaddressesdb addressDto)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                bool success = AddressService.UpdateAddress(id, currentUserId, addressDto);
                if (!success)
                    return NotFound(new { message = "Address not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id:int}/default")]
        [Authorize]
        public IActionResult SetDefaultAddress(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                bool success = AddressService.SetDefaultAddress(id, currentUserId);
                if (!success)
                    return NotFound(new { message = "Address not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public IActionResult DeleteAddress(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                bool success = AddressService.DeleteAddress(id, currentUserId);
                if (!success)
                    return NotFound(new { message = "Address not found or cannot delete the only address." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}