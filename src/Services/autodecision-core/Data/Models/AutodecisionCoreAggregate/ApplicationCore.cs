using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Data.Models.Base;
using AutodecisionCore.Extensions;
using AutodecisionCore.Utils;
using System.Text;

#pragma warning disable CS8603

namespace AutodecisionCore.Data.Models.AutodecisionCoreAggregate
{
    public class ApplicationCore : BaseModel
    {
        public string LoanNumber { get; private set; }
        public int ProcessingVersion { get; private set; }
        public InternalStatusEnum Status { get; private set; }
        public virtual ICollection<ApplicationFlag> ApplicationFlags { get; private set; } = new List<ApplicationFlag>();
        public virtual ICollection<ApplicationProcess> ApplicationProcesses { get; private set; } = new List<ApplicationProcess>();
        public virtual ICollection<AutoApprovalRule> AutoApprovalRules { get; private set; } = new List<AutoApprovalRule>();
        public DateTime? ProcessedAt { get; set; }
        public string CustomerName { get; private set; }
        public string EmployerName { get; private set; }
        public string StateAbbreviation { get; private set; }
        public string Type { get; private set; }

        public bool AllowedProcessing(DateTime requestedAt)
        {
            if (ProcessedAt == null)
                return true;
            if (requestedAt <= ProcessedAt)
                return false;
            return true;
        }

        public void RefreshProcessingInfo()
        {
            if (ProcessedAt != null || ProcessingVersion > 1)
            {
                ProcessingVersion++;
                Status = InternalStatusEnum.Pending;
            }
            ProcessedAt = DateTimeUtil.Now;
        }

        public ApplicationCore(string loanNumber)
        {
            LoanNumber = loanNumber;
            ProcessingVersion = 1;
            Status = InternalStatusEnum.Pending;
        }

        public ApplicationFlag GetApplicationFlagByFlagCode(string flagCode) =>
            ApplicationFlags.FirstOrDefault(f => f.FlagCode == flagCode);

        public void AddFlag(string flagCode, bool internalFlag, FlagResultEnum? status = null)
        {
            ApplicationFlags.Add(new ApplicationFlag()
            {
                FlagCode = flagCode,
                Status = (int)(status ?? FlagResultEnum.InProcessing),
                RequestedAt = DateTimeUtil.Now,
                InternalFlag = internalFlag
            });
        }

        public void ReprocessAllFlags()
        {
            var flags = ApplicationFlags.Where(x =>
                         x.Status != (int)FlagResultEnum.Approved
                         && x.Status != (int)FlagResultEnum.Warning
                         );

            bool isAutoApprovalFlagApproved = IsAutoApprovalFlagApproved();

            foreach (var flag in flags)
            {
                if (flag.IsFlagCodeRelatedToAutoApproval() && isAutoApprovalFlagApproved)
                    continue;

                flag.Status = (int)FlagResultEnum.InProcessing;
                flag.RequestedAt = DateTimeUtil.Now;
                flag.ProcessedAt = null;
                flag.Description = string.Empty;
            }
        }

        public void ReprocessAllFlags(List<string> flags)
        {
            if (flags.Count == 0)
            {
                ReprocessAllFlags();
                return;
            }

            bool isAutoApprovalFlagApproved = IsAutoApprovalFlagApproved();

            foreach (var flag in ApplicationFlags.Where(x =>
                         flags.Contains(x.FlagCode) && (!x.InternalFlag || x.Status != (int)FlagResultEnum.Approved)))
            {
                if (flag.IsFlagCodeRelatedToAutoApproval() && isAutoApprovalFlagApproved)
                    continue;

                flag.Status = (int)FlagResultEnum.InProcessing;
                flag.RequestedAt = DateTimeUtil.Now;
                flag.ProcessedAt = null;
                flag.Description = string.Empty;
            }
        }

        public IEnumerable<ApplicationFlag> GetNeededFlagsToProcess() =>
            ApplicationFlags.Where(x => x.Status == (int)FlagResultEnum.InProcessing && !x.InternalFlag);

        public bool HasInProcessingFlags() =>
            ApplicationFlags.Any(x => x.Status == (int)FlagResultEnum.InProcessing && !x.InternalFlag);

        public bool HasAutoDenyFlag() =>
             ApplicationFlags.Any(x => x.Status == (int)FlagResultEnum.AutoDeny);

        public bool HasPendingApprovalFlagsBesides(string[] ignoredFlagCodes) =>
            ApplicationFlags.Any(x =>
            !ignoredFlagCodes.Contains(x.FlagCode) &&
            (x.Status == (int)FlagResultEnum.PendingApproval || x.Status == (int)FlagResultEnum.Error)
            );

        public bool HasPendingApprovalFlagsBesides(string ignoredFlagCode) =>
            ApplicationFlags.Any(x =>
                x.FlagCode != ignoredFlagCode &&
                (x.Status == (int)FlagResultEnum.PendingApproval || x.Status == (int)FlagResultEnum.Error)
            );

        public bool CanAskForAllotmentOrProcessed()
        {
            var pendingAutoApprovalRules = AutoApprovalRules.Any(x => x.Status == AutoApprovalResultEnum.Pending);

            if (IsAutoApprovalFlagApproved())
                pendingAutoApprovalRules = false;

            var pendingApplicationFlags = ApplicationFlags.Any(
                x => !x.InternalFlag
                && x.FlagCode != FlagCode.AllotmentValidation
                && (
                    x.Status == (int)FlagResultEnum.PendingApproval
                    || x.Status == (int)FlagResultEnum.Error)
                    );

            if (pendingApplicationFlags || pendingAutoApprovalRules)
                return false;

            return true;
        }

        public List<string> GetPendingApprovalFlagCodes() =>
            ApplicationFlags
            .Where(x => x.Status == (int)FlagResultEnum.PendingApproval || x.Status == (int)FlagResultEnum.Error)
            .Select(x => x.FlagCode)
            .ToList();

        public void ReceiveFlagReponse(string code, string message, DateTime processedAt, int result, List<InternalMessage>? internalMessages, string approvalNote)
        {
            var flag = GetApplicationFlagByFlagCode(code);

            if (flag.Status == (int)FlagResultEnum.Approved)
                return; // não podemos mudar nada quando uma flag já está aprovada.

            if (!string.IsNullOrEmpty(message))
                message = message[..Math.Min(1000, message.Length)];

            flag.Description = message;
            flag.ProcessedAt = processedAt;
            flag.Status = result;

            if (flag.Status == (int)FlagResultEnum.Approved)
            {
                flag.ApprovedByName = AutoDecisionUser.Name;
                flag.ApprovedBy = AutoDecisionUser.Id;
                flag.ApprovedAt = DateTimeUtil.Now;
                flag.ApprovalNote = approvalNote;
            }

            if (internalMessages != null && internalMessages.Count > 0)
            {
                foreach (var internalMessage in internalMessages)
                {
                    flag.ApplicationFlagsInternalMessage.Add(new ApplicationFlagsInternalMessage(internalMessage.Message, (int)internalMessage.MessageType, internalMessage.Code));
                }
            }
        }

        public void ApproveFlag(string code, string description, int aprovedBy, string approvedByName)
        {
            var flag = GetApplicationFlagByFlagCode(code);

            if (flag.Status != (int)FlagResultEnum.PendingApproval)
                return;

            flag.Status = (int)FlagResultEnum.Approved;
            flag.ApprovalNote = description;
            flag.ApprovedBy = aprovedBy;
            flag.ApprovedAt = DateTimeUtil.Now;
            flag.ApprovedByName = approvedByName;
        }

        public void ProcessFlag(string code, string description)
        {
            var flag = GetApplicationFlagByFlagCode(code);

            if (flag.Status != (int)FlagResultEnum.PendingApproval)
                return;

            flag.Status = (int)FlagResultEnum.Processed;
            flag.Description = description;
        }

        public bool IsAutoApprovalFlagApproved() =>
            ApplicationFlags.Any(x => (x.IsFlagCodeRelatedToAutoApproval()) && x.Status == (int)FlagResultEnum.Approved);

        public bool ApplicationProcessIsFinished()
        {
            if (Status == InternalStatusEnum.AutoApproval || Status == InternalStatusEnum.AutoDeny)
                return true;

            return false;
        }

        public void UpdateApplicationStatus(InternalStatusEnum status)
        {
            Status = status;

            ApplicationProcesses.Add(new ApplicationProcess()
            {
                CreatedAt = DateTimeUtil.Now,
                ProcessingVersion = ProcessingVersion,
                Status = status,
                ProcessedAt = DateTimeUtil.Now
            });
        }

        public void ClearAutoApprovalRules()
        {
            ClearAutoApprovalFlags();

            foreach (var item in AutoApprovalRules)
            {
                item.Status = AutoApprovalResultEnum.Ignored;
                item.Description = null;
            }
        }

        public void AddAutoApprovalRule(AutoApprovalRule autoApprovalRule)
        {
            var rule = AutoApprovalRules.FirstOrDefault(x => x.RuleName == autoApprovalRule.RuleName);

            if (rule == null)
            {
                AutoApprovalRules.Add(autoApprovalRule);
                return;
            }

            rule.Status = autoApprovalRule.Status;
            rule.Description = autoApprovalRule.Description;
        }

        public void Validate()
        {
            if (AutoApprovalRules.Any(x => x.Status == AutoApprovalResultEnum.Error))
            {
                if (Type == null)
                {
                    RaiseAutoApprovalFlag(FlagResultEnum.Error, FlagCode.LoanVerification);
                }
                else if (Type.Equals(ApplicationType.Refi))
                {
                    RaiseAutoApprovalFlag(FlagResultEnum.Error, FlagCode.Flag209);
                    RaiseAutoApprovalFlag(FlagResultEnum.Ignored, FlagCode.LoanVerification);
                }
                else
                {
                    RaiseAutoApprovalFlag(FlagResultEnum.Error, FlagCode.LoanVerification);
                    RaiseAutoApprovalFlag(FlagResultEnum.Ignored, FlagCode.Flag209);
                }
            }
            else if (AutoApprovalRules.Any(x => x.Status == AutoApprovalResultEnum.Pending
                                            || x.Status == AutoApprovalResultEnum.PendingAllotment))
            {
                if (Type == null)
                {
                    RaiseAutoApprovalFlag(FlagResultEnum.PendingApproval, FlagCode.LoanVerification);
                }
                else if (Type.Equals(ApplicationType.Refi))
                {
                    RaiseAutoApprovalFlag(FlagResultEnum.PendingApproval, FlagCode.Flag209);
                    RaiseAutoApprovalFlag(FlagResultEnum.Ignored, FlagCode.LoanVerification);
                }
                else
                {
                    RaiseAutoApprovalFlag(FlagResultEnum.PendingApproval, FlagCode.LoanVerification);
                    RaiseAutoApprovalFlag(FlagResultEnum.Ignored, FlagCode.Flag209);
                }
            }
            else
            {
                if (Type.Equals(ApplicationType.Refi))
                {
                    RaiseAutoApprovalFlag(FlagResultEnum.Processed, FlagCode.Flag209);
                    RaiseAutoApprovalFlag(FlagResultEnum.Ignored, FlagCode.LoanVerification);
                }
                else
                {
                    RaiseAutoApprovalFlag(FlagResultEnum.Processed, FlagCode.LoanVerification);
                    RaiseAutoApprovalFlag(FlagResultEnum.Ignored, FlagCode.Flag209);
                }
            }
        }

        private void ClearAutoApprovalFlags()
        {
            var flag180 = ApplicationFlags.Where(x => x.FlagCode == FlagCode.LoanVerification).FirstOrDefault();
            var flag209 = ApplicationFlags.Where(x => x.FlagCode == FlagCode.Flag209).FirstOrDefault();

            if (flag180 != null)
            {
                flag180.Status = (int)FlagResultEnum.Ignored;
                flag180.Description = string.Empty;
                flag180.ProcessedAt = null;
            }

            if (flag209 != null)
            {
                flag209.Status = (int)FlagResultEnum.Ignored;
                flag209.Description = string.Empty;
                flag209.ProcessedAt = null;
            }
        }

        private void RaiseAutoApprovalFlag(FlagResultEnum status, string flagCode)
        {
            var flag = ApplicationFlags.FirstOrDefault(x => x.FlagCode == flagCode);

            if (flag == null)
            {
                flag = new ApplicationFlag()
                {
                    CreatedAt = DateTimeUtil.Now,
                    FlagCode = flagCode,
                    RequestedAt = DateTimeUtil.Now
                };

                ApplicationFlags.Add(flag);
            }
            if (status != FlagResultEnum.Ignored)
            {
                var finalText = new StringBuilder();

                foreach (var item in AutoApprovalRules.Where(x => x.Status != AutoApprovalResultEnum.Ignored))
                {
                    if (!string.IsNullOrEmpty(item.Description))
                        finalText.Append($"{item.Description}; ");
                }

                flag.Description = finalText.ToString();
            }

            flag.Status = (int)status;
            flag.ProcessedAt = DateTimeUtil.Now;
        }

        public bool HasDenyAutoApprovalRules() =>
            AutoApprovalRules.Any(x => x.Status == AutoApprovalResultEnum.Denied);

        public bool HasAllotmentNeeded()
        {
            var hasAutoApprovalAllotment = AutoApprovalRules.Any(
                x => x.RuleName == "AllotmentRule"
                && x.Status == AutoApprovalResultEnum.PendingAllotment);

            var hasCommitmentLevel = AutoApprovalRules.Any(
                x => x.RuleName == "CommitmentLevelRule"
                && x.Status == AutoApprovalResultEnum.Approved);

            var hasEndBalanceCommitmentLevel = AutoApprovalRules.Any(
                x => x.RuleName == "EndBalanceCommitmentLevelRule"
                && x.Status == AutoApprovalResultEnum.Approved);

            return hasAutoApprovalAllotment && HasPendingAllotmentFlag() && hasCommitmentLevel && !hasEndBalanceCommitmentLevel;
        }

        public bool HasPendingAllotmentFlag() =>
            ApplicationFlags.Any(x => x.FlagCode == FlagCode.AllotmentValidation && x.Status == (int)FlagResultEnum.PendingApproval);

        public bool HasRequestOldPayStub() =>
            ApplicationFlags.Any(x => x.FlagCode == FlagCode.OpenPayrollInconsistency && x.Status == (int)FlagResultEnum.PendingApproval
                && x.Description.Contains("Asking for customer the oldest paystub to manual approval"));

        public ApplicationFlag HasPaystubApproved() =>
            ApplicationFlags?.FirstOrDefault(x => x.FlagCode == FlagCode.OpenPayrollNotConnected && x.Description?.Contains("There is paystub document uploaded and approved") == true);

        public ApplicationFlag HasBankStatementApproved() => ApplicationFlags?.FirstOrDefault(x => x.FlagCode == FlagCode.OpenBankingNotConnected
                                                                && x.Description?.Contains("There is bank statement uploaded and approved") == true);
        public List<string> InProcessingFlags() =>
            ApplicationFlags.Where(x => x.Status == (int)FlagResultEnum.InProcessing).Select(x => x.FlagCode).ToList();

        public void SetCustomerInfo(string customerName, string employerName, string stateAbbreviation)
        {
            CustomerName = customerName;
            EmployerName = employerName;
            StateAbbreviation = stateAbbreviation;
        }

        public void SetApplicationInfo(string type)
        {
            Type = type;
        }

        public void RaiseFlag183(string user)
        {

            if (!ApplicationFlags.Any(x => x.FlagCode == FlagCode.OpenForChanges))
            {
                ApplicationFlags.Add(new ApplicationFlag()
                {
                    CreatedAt = DateTimeUtil.Now,
                    FlagCode = FlagCode.OpenForChanges,
                    RequestedAt = DateTimeUtil.Now,
                });
            }

            var flag183 = ApplicationFlags.FirstOrDefault(x => x.FlagCode == FlagCode.OpenForChanges);

            flag183.Description = $"The application was opened for changes by {user} at {DateTimeUtil.Now}";
            flag183.Status = (int)FlagResultEnum.Warning;
            flag183.ProcessedAt = DateTimeUtil.Now;
            flag183.InternalFlag = true;
        }
    }
}