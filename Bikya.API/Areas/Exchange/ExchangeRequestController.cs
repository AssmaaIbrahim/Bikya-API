using Bikya.DTOs.ExchangeRequestDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bikya.API.Areas.Exchange
{
    /// <summary>
    /// Controller for managing exchange request operations.
    /// </summary>
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Exchange")]
    [Authorize]
    public class ExchangeRequestController : ControllerBase
    {
        private readonly IExchangeRequestService _service;

        public ExchangeRequestController(IExchangeRequestService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Gets the current user ID from claims.
        /// </summary>
        /// <returns>User ID</returns>
        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                throw new UnauthorizedAccessException("Invalid user token");
            return userId;
        }

        /// <summary>
        /// Creates a new exchange request.
        /// </summary>
        /// <param name="dto">Exchange request creation data</param>
        /// <returns>Creation result</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExchangeRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _service.CreateAsync(dto, GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Gets an exchange request by ID.
        /// </summary>
        /// <param name="id">Exchange request ID</param>
        /// <returns>Exchange request details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid exchange request ID" });

            var response = await _service.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Gets all exchange requests.
        /// </summary>
        /// <returns>List of all exchange requests</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Gets exchange requests sent by the current user.
        /// </summary>
        /// <returns>List of sent exchange requests</returns>
        [HttpGet("sent")]
        public async Task<IActionResult> GetSent()
        {
            var response = await _service.GetSentRequestsAsync(GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Gets exchange requests received by the current user.
        /// </summary>
        /// <returns>List of received exchange requests</returns>
        [HttpGet("received")]
        public async Task<IActionResult> GetReceived()
        {
            var response = await _service.GetReceivedRequestsAsync(GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Approves an exchange request.
        /// </summary>
        /// <param name="id">Exchange request ID</param>
        /// <returns>Approval result</returns>
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid exchange request ID" });

            var response = await _service.ApproveRequestAsync(id, GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Rejects an exchange request.
        /// </summary>
        /// <param name="id">Exchange request ID</param>
        /// <returns>Rejection result</returns>
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid exchange request ID" });

            var response = await _service.RejectRequestAsync(id, GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Deletes an exchange request.
        /// </summary>
        /// <param name="id">Exchange request ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid exchange request ID" });

            var response = await _service.DeleteAsync(id, GetUserId());
            return StatusCode(response.StatusCode, response);
        }
    }
}
