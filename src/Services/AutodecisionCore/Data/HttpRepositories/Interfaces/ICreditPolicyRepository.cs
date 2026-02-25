using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface ICreditPolicyRepository
    {   
        Task<CreditPolicy> GetCreditPolicyInfo(string employerKey, string productKey, string stateAbbreviation, string applicationType);
    }
}
