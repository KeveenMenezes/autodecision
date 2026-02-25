using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.DTOs;

namespace AutodecisionCore.Services.Interfaces
{
    public interface IAutodecisionPublisherService
    {
        Task PublishAutodecisionProcessRequest(Application application, string reason);
        Task PublishTheFlagsRequest(ApplicationCore applicationCore, string redisKey, string reason);
        Task PublishFinalEvaluationRequest(ProcessFlagResponseEvent response);
        Task PublishFinalEvaluationRequest(ApplicationCore application, string reason);
        Task PublishApproveApplicationRequestEvent(string loanNumber);
        Task PublishDeclineApplicationRequestEvent(string loanNumber, string reason);
        Task PublishRequireDocumentsRequestEvent(string loanNumber, List<string> flagCodes);
        Task PublishNotifyDefaultDocumentsRequestEvent(string loanNumber);
        Task PublishRequireAllotmentRequestEvent(string loanNumber, string paymentType);
        Task HandleApplicationPublisher(string reason, Application application, ApplicationCore applicationCore, string redisKey);
        Task PublishOpenConnectionsProcessRequest(ApplicationCore applicationCore, string reason);
    }
}
