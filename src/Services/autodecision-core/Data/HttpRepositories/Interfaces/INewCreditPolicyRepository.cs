using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces;

public interface INewCreditPolicyRepository
{
    Task<CreditPolicy> GetNewCreditPolicyAndRules(Application application);
}