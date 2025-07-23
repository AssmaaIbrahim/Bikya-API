using Bikya.Data.Enums;
using Bikya.Data.Models;
using Bikya.Data.Repositories.Interfaces;
using Bikya.Data.Response;
using Bikya.DTOs.ExchangeRequestDTOs;
using Bikya.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bikya.Services.Services
{
    public class ExchangeRequestService : IExchangeRequestService
    {
        private readonly IExchangeRequestRepository _exchangeRequestRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ExchangeRequestService> _logger;

        public ExchangeRequestService(
            IExchangeRequestRepository exchangeRequestRepository,
            IProductRepository productRepository,
            ILogger<ExchangeRequestService> logger)
        {
            _exchangeRequestRepository = exchangeRequestRepository ?? throw new ArgumentNullException(nameof(exchangeRequestRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<ExchangeRequestDTO>> CreateAsync(CreateExchangeRequestDTO dto, int senderUserId)
        {
            try
            {
                var offeredProduct = await _productRepository.GetProductWithImagesByIdAsync(dto.OfferedProductId);
                var requestedProduct = await _productRepository.GetProductWithImagesByIdAsync(dto.RequestedProductId);

                if (offeredProduct == null || requestedProduct == null)
                    return ApiResponse<ExchangeRequestDTO>.ErrorResponse("One or both products not found", 404);

                if (offeredProduct.UserId != senderUserId)
                    return ApiResponse<ExchangeRequestDTO>.ErrorResponse("You can only offer your own product", 403);

                // Check for existing pending request between these products
                var hasPendingRequest = await _exchangeRequestRepository.HasPendingRequestBetweenProductsAsync(
                    dto.OfferedProductId, dto.RequestedProductId);

                if (hasPendingRequest)
                    return ApiResponse<ExchangeRequestDTO>.ErrorResponse("A pending request already exists between these products", 409);

                var request = new ExchangeRequest
                {
                    OfferedProductId = dto.OfferedProductId,
                    RequestedProductId = dto.RequestedProductId,
                    Message = dto.Message
                };

                await _exchangeRequestRepository.AddAsync(request);
                await _exchangeRequestRepository.SaveChangesAsync();

                _logger.LogInformation("Exchange request created successfully by user {UserId} for products {OfferedProductId} and {RequestedProductId}", 
                    senderUserId, dto.OfferedProductId, dto.RequestedProductId);

                return ApiResponse<ExchangeRequestDTO>.SuccessResponse(ToDTO(request), "Exchange request created successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exchange request by user {UserId}", senderUserId);
                return ApiResponse<ExchangeRequestDTO>.ErrorResponse("An error occurred while creating the exchange request", 500);
            }
        }

        public async Task<ApiResponse<ExchangeRequestDTO>> GetByIdAsync(int id)
        {
            try
            {
                var request = await _exchangeRequestRepository.GetByIdWithProductsAsync(id);

                if (request == null)
                    return ApiResponse<ExchangeRequestDTO>.ErrorResponse("Request not found", 404);

                return ApiResponse<ExchangeRequestDTO>.SuccessResponse(ToDTO(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exchange request with ID {RequestId}", id);
                return ApiResponse<ExchangeRequestDTO>.ErrorResponse("An error occurred while retrieving the exchange request", 500);
            }
        }

        public async Task<ApiResponse<List<ExchangeRequestDTO>>> GetAllAsync()
        {
            try
            {
                var requests = await _exchangeRequestRepository.GetAllWithProductsAsync();
                var requestDTOs = requests.Select(ToDTO).ToList();

                return ApiResponse<List<ExchangeRequestDTO>>.SuccessResponse(requestDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all exchange requests");
                return ApiResponse<List<ExchangeRequestDTO>>.ErrorResponse("An error occurred while retrieving exchange requests", 500);
            }
        }

        public async Task<ApiResponse<List<ExchangeRequestDTO>>> GetSentRequestsAsync(int senderUserId)
        {
            try
            {
                var requests = await _exchangeRequestRepository.GetSentRequestsAsync(senderUserId);
                var requestDTOs = requests.Select(ToDTO).ToList();

                return ApiResponse<List<ExchangeRequestDTO>>.SuccessResponse(requestDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sent requests for user {UserId}", senderUserId);
                return ApiResponse<List<ExchangeRequestDTO>>.ErrorResponse("An error occurred while retrieving sent requests", 500);
            }
        }

        public async Task<ApiResponse<List<ExchangeRequestDTO>>> GetReceivedRequestsAsync(int receiverUserId)
        {
            try
            {
                var requests = await _exchangeRequestRepository.GetReceivedRequestsAsync(receiverUserId);
                var requestDTOs = requests.Select(ToDTO).ToList();

                return ApiResponse<List<ExchangeRequestDTO>>.SuccessResponse(requestDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving received requests for user {UserId}", receiverUserId);
                return ApiResponse<List<ExchangeRequestDTO>>.ErrorResponse("An error occurred while retrieving received requests", 500);
            }
        }

        public async Task<ApiResponse<ExchangeRequestDTO>> ApproveRequestAsync(int requestId, int currentUserId)
        {
            try
            {
                var request = await _exchangeRequestRepository.GetRequestForApprovalAsync(requestId, currentUserId);

                if (request == null)
                    return ApiResponse<ExchangeRequestDTO>.ErrorResponse("Request not found or you are not authorized to approve this request", 404);

                var success = await _exchangeRequestRepository.UpdateStatusAsync(requestId, ExchangeStatus.Accepted);
                if (!success)
                    return ApiResponse<ExchangeRequestDTO>.ErrorResponse("Failed to update request status", 500);

                await _exchangeRequestRepository.SaveChangesAsync();

                // Refresh the request to get updated data
                var updatedRequest = await _exchangeRequestRepository.GetByIdWithProductsAsync(requestId);
                
                _logger.LogInformation("Exchange request {RequestId} approved by user {UserId}", requestId, currentUserId);
                
                return ApiResponse<ExchangeRequestDTO>.SuccessResponse(ToDTO(updatedRequest!), "Request approved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving exchange request {RequestId} by user {UserId}", requestId, currentUserId);
                return ApiResponse<ExchangeRequestDTO>.ErrorResponse("An error occurred while approving the request", 500);
            }
        }

        public async Task<ApiResponse<ExchangeRequestDTO>> RejectRequestAsync(int requestId, int currentUserId)
        {
            try
            {
                var request = await _exchangeRequestRepository.GetRequestForApprovalAsync(requestId, currentUserId);

                if (request == null)
                    return ApiResponse<ExchangeRequestDTO>.ErrorResponse("Request not found or you are not authorized to reject this request", 404);

                var success = await _exchangeRequestRepository.UpdateStatusAsync(requestId, ExchangeStatus.Rejected);
                if (!success)
                    return ApiResponse<ExchangeRequestDTO>.ErrorResponse("Failed to update request status", 500);

                await _exchangeRequestRepository.SaveChangesAsync();

                // Refresh the request to get updated data
                var updatedRequest = await _exchangeRequestRepository.GetByIdWithProductsAsync(requestId);
                
                _logger.LogInformation("Exchange request {RequestId} rejected by user {UserId}", requestId, currentUserId);
                
                return ApiResponse<ExchangeRequestDTO>.SuccessResponse(ToDTO(updatedRequest!), "Request rejected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting exchange request {RequestId} by user {UserId}", requestId, currentUserId);
                return ApiResponse<ExchangeRequestDTO>.ErrorResponse("An error occurred while rejecting the request", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int requestId, int currentUserId)
        {
            try
            {
                var request = await _exchangeRequestRepository.GetRequestForDeletionAsync(requestId, currentUserId);

                if (request == null)
                    return ApiResponse<bool>.ErrorResponse("Request not found or you are not authorized to delete this request", 404);

                _exchangeRequestRepository.Remove(request);
                await _exchangeRequestRepository.SaveChangesAsync();

                _logger.LogInformation("Exchange request {RequestId} deleted by user {UserId}", requestId, currentUserId);

                return ApiResponse<bool>.SuccessResponse(true, "Request deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting exchange request {RequestId} by user {UserId}", requestId, currentUserId);
                return ApiResponse<bool>.ErrorResponse("An error occurred while deleting the request", 500);
            }
        }

        private ExchangeRequestDTO ToDTO(ExchangeRequest request)
        {
            return new ExchangeRequestDTO
            {
                Id = request.Id,
                OfferedProductId = request.OfferedProductId,
                RequestedProductId = request.RequestedProductId,
                Status = request.Status.ToString(),
                Message = request.Message,
                RequestedAt = request.RequestedAt
            };
        }
    }
}