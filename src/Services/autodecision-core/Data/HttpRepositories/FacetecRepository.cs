using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class FacetecRepository : IFacetecRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;
        private readonly ILogger<FacetecRepository> _logger;

        public FacetecRepository(IConfiguration configuration, IHttpService httpService, ILogger<FacetecRepository> logger)
        {
            _configuration = configuration;
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<FaceRecognition> GetFaceRecognition(int customerId)
        {
            var result = new FaceRecognition();
            result.ClientIdsMatch = new List<int>();
            var resultRecognition = await GetFaceRecognitionAsync(customerId);
            var resultRecognitionFraud = await GetFaceRecognitionFraudAsync(customerId);

            if (resultRecognition != null)
            {
                if (resultRecognition.Enrollment != null)
                {
                    result.EnrollmentStatus = resultRecognition.Enrollment.Status;
                    result.Liveness = resultRecognition.Enrollment.Liveness;
                }
                if (resultRecognition.DocumentScan != null)
                {
                    result.DocumentScanSuccess = resultRecognition.DocumentScan.Success;
                    result.DocumentData = MapDocumentDataScanned(resultRecognition.DocumentScan.DocumentData);
                }
            }
            if (resultRecognitionFraud != null)
            {
                result.FraudStatus = resultRecognitionFraud.Status;
                if (resultRecognitionFraud.Enrollments != null && resultRecognitionFraud.Enrollments.Any())
                {
                    result.ClientIdsMatch.AddRange(resultRecognitionFraud.Enrollments.Select(x => x.CustomerId));
                }
            }
            return result;
        }

        public async Task<FaceTecDTO> GetFaceRecognitionAsync(int customerId)
        {
            try
            {
                var url = _configuration["Apis:FacetecApi"] + $"/api/Customer/{customerId}/details";

                var response = await _httpService.GetAsync<FacetecResponseApi>(url);
                if (!response.Success)
                {
                    _logger.LogWarning($"CustomerId: {customerId} couldn't get the facetec validation, response_code: {response.StatusCode}");
                    return new FaceTecDTO();
                }
                return response.Response.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get validation of Facetec for CustomerId: {customerId} | Error: {ex.Message}");
                throw;
            }
        }

        public async Task<FaceTecDTO> GetFaceRecognitionFraudAsync(int customerId)
        {
            try
            {
                var url = _configuration["Apis:FacetecApi"] + $"/api/Customer/{customerId}/details/fraud";

                var response = await _httpService.GetAsync<FacetecResponseApi>(url);
                if (!response.Success)
                {
                    _logger.LogWarning($"Couldn't get the Facetec validation, response_code: {response.StatusCode}");
                    return new FaceTecDTO();
                }
                return response.Response.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get validation of Facetec for CustomerId: {customerId} | Error: {ex.Message}");
                throw;
            }
        }

        private static DocumentDataScanned MapDocumentDataScanned(DocumentData documentDto)
        {
            if (documentDto?.Scanned == null)
                return new DocumentDataScanned();

            return new DocumentDataScanned()
            {
                IdNumber = documentDto.Scanned.IdNumber,
                FirstName = documentDto.Scanned.FirstName,
                LastName = documentDto.Scanned.LastName,
                DateOfBirth = documentDto.Scanned.DateOfBirth,
                Sex = documentDto.Scanned.Sex,
                Address = documentDto.Scanned.Address
            };
        }
    }
}