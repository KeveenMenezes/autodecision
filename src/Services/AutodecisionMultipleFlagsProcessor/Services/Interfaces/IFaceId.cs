using AutodecisionMultipleFlagsProcessor.DTOs;

namespace AutodecisionMultipleFlagsProcessor.Services.Interfaces
{
    public interface IFaceId
    {
        Task<CustomerFaceIdDTO> GetCustomerFaceId(int customer_id);
    }
}
