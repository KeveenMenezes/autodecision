using AutodecisionCore.Commands;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.DTOs;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using RedLockNet;
using Application = AutodecisionCore.Contracts.ViewModels.Application.Application;

namespace AutodecisionCore.Services
{
    public class ApplicationCoreService : IApplicationCoreService
    {
        private readonly IApplicationCoreRepository _applicationCoreRepository;
        private readonly IFlagRepository _flagRepository;
        private readonly ILogger<ApplicationCoreService> _logger;
        private readonly IDistributedLockFactory _redLockFactory;

        public ApplicationCoreService(
            IApplicationCoreRepository applicationCoreRepository,
            IFlagRepository flagRepository,
            ILogger<ApplicationCoreService> logger,
            IDistributedLockFactory redLockFactory
            )
        {
            _applicationCoreRepository = applicationCoreRepository;
            _flagRepository = flagRepository;
            _logger = logger;
            _redLockFactory = redLockFactory;
        }

        public async Task<ApplicationCore> ApplicationCoreRegister(string loanNumber)
        {
            var applicationCore = await _applicationCoreRepository.FindByLoanNumberAsync(loanNumber);
            if (applicationCore == null)
            {
                applicationCore = new ApplicationCore(loanNumber);
                await _applicationCoreRepository.CreateAsync(applicationCore);
                _logger.LogInformation($"Registered Application Core for Loan Number: {loanNumber}");
            }
            return applicationCore;
        }

        public async Task<List<ApplicationDto>> GetFlagsByLoanNumbers(List<string> loanNumbers)
        {
            List<ApplicationDto> applicationList = new List<ApplicationDto>();
            var flags = await _flagRepository.GetAllFlagsAsync();
            var flagsByLoan = await _applicationCoreRepository.FindByLoanNumbersIncludeApplicationFlagsAsync(loanNumbers);
            var response = new List<ApplicationDto>();
            if (flagsByLoan != null)
            {
                foreach (var flag in flagsByLoan)
                {
                    var newApp = new ApplicationDto() { LoanNumber = flag.Key };
                    var flagList = new List<ApplicationFlagDto>();
                    flag.Value.ForEach(value => flagList.Add(new ApplicationFlagDto(value, flags)));
                    newApp.ApplicationFlags = flagList.OrderBy(x => Convert.ToInt16(x.FlagCode)).ToList();
                    response.Add(newApp);
                }
            }
            return response;
        }

        public async Task<List<ApplicationFlagDto>> GetFlagsByLoanNumber(string loanNumber)
        {
            List<ApplicationFlagDto> response = new List<ApplicationFlagDto>();

            var flagsByLoan = await _applicationCoreRepository.FindByLoanNumberIncludeApplicationFlagsAsync(loanNumber);

            if (flagsByLoan != null)
            {
                var flags = await _flagRepository.GetAllFlagsAsync();

                foreach (var flag in flagsByLoan.ApplicationFlags)
                {

                    var flagProperties = flags.Where(x => x.Code == flag.FlagCode).FirstOrDefault();

                    var newitem = new ApplicationFlagDto()
                    {
                        FlagCode = flag.FlagCode,
                        ApprovedAt = flag.ApprovedAt,
                        ApprovedBy = flag.ApprovedBy,
                        ApprovedByName = flag.ApprovedByName,
                        ApprovalNote = flag.ApprovalNote,
                        Description = flag.Description,
                        RequestedAt = flag.RequestedAt,
                        Status = flag.Status,
                        FlagName = flagProperties != null ? flagProperties.Name : "",
                        FlagDescription = flagProperties != null ? flagProperties.Description : "",
                    };
                    response.Add(newitem);

                }
            }
            var ordenatedResponse = response.OrderBy(x => Convert.ToInt16(x.FlagCode)).ToList();

            return ordenatedResponse;
        }

        public async Task<ApplicationFlagDto> GetFlagsByLoanNumberAndFlagCode(string loanNumber, string flagCode)
        {
            ApplicationFlagDto response = new ApplicationFlagDto();

            var flagsByLoan = await _applicationCoreRepository.FindByLoanNumberIncludeApplicationFlagsAsync(loanNumber);
            if (flagsByLoan != null)
            {
                var flag = flagsByLoan.ApplicationFlags.FirstOrDefault(x => x.FlagCode == flagCode);

                var flagProperties = await _flagRepository.GetFlagByCodeAsync(flagCode);

                if (flag != null)
                {

                    response.FlagCode = flag.FlagCode;
                    response.ApprovedAt = flag.ApprovedAt;
                    response.ApprovedBy = flag.ApprovedBy;
                    response.ApprovedByName = flag.ApprovedByName;
                    response.ApprovalNote = flag.ApprovalNote;
                    response.Description = flag.Description;
                    response.RequestedAt = flag.RequestedAt;
                    response.Status = flag.Status;
                    response.FlagName = flagProperties != null ? flagProperties.Name : "";
                    response.FlagDescription = flagProperties != null ? flagProperties.Description : "";

                }
            }
            return response;
        }

        public async Task<List<ApplicationFlagDto>> ApproveFlag(string loanNumber, string flagCode, ApproveFlagCommand command)
        {
            List<ApplicationFlagDto> response = new List<ApplicationFlagDto>();

            var resource = $"{RedLockKeys.RequestProcess}{loanNumber}";
            var timeResouce = new TimeSpan(0, 0, 2, 0);
            var retryTime = new TimeSpan(0, 0, 0, 3);

            using (var redLock = await _redLockFactory.CreateLockAsync(resource, timeResouce, timeResouce, retryTime))
            {
                if (!redLock.IsAcquired)
                {
                    _logger.LogWarning($"Autodecision Process: Resource {resource} is locked. LoanNumber: {loanNumber}");
                    throw new Exception($"Red Lock don't acquired for LoanNumber: {loanNumber}");
                }

                var flagsByLoan = await _applicationCoreRepository.FindByLoanNumberIncludeApplicationFlagsAsync(loanNumber);

                if (flagsByLoan != null)
                {

                    flagsByLoan.ApproveFlag(flagCode, command.Description, command.ApprovedBy, command.ApprovedByName);
                    await _applicationCoreRepository.SaveChanges();

                    foreach (var flag in flagsByLoan.ApplicationFlags)
                    {
                        if (flag.Status != FlagResultEnum.Ignored.GetHashCode() && flag.Status != FlagResultEnum.Processed.GetHashCode() && flag.Status != FlagResultEnum.Approved.GetHashCode())
                        {
                            var newitem = new ApplicationFlagDto()
                            {
                                FlagCode = flag.FlagCode,
                                ApprovedAt = flag.ApprovedAt,
                                ApprovedBy = flag.ApprovedBy,
                                ApprovedByName = flag.ApprovedByName,
                                ApprovalNote = flag.ApprovalNote,
                                Description = flag.Description,
                                RequestedAt = flag.RequestedAt,
                                Status = flag.Status,
                            };
                            response.Add(newitem);

                        }
                    }
                }
            }
            return response;
        }

        public async Task<ResultDTO> OpenForChanges(string loanNumber, string user)
        {
            ResultDTO response = new ResultDTO();

            var flagsByLoan = await _applicationCoreRepository.FindByLoanNumberIncludeApplicationFlagsAsync(loanNumber);
            if (flagsByLoan != null)
            {
                flagsByLoan.RaiseFlag183(user);
                await _applicationCoreRepository.SaveChanges();

                response.Success = true;
                return response;
            }

            response.Success = false;
            return response;
        }
    }
}
