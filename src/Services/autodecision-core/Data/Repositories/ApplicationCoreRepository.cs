using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Data.Context;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Repositories.Base;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq;

//disable nullable reference warnings for database methods
#pragma warning disable CS8603

namespace AutodecisionCore.Data.Repositories
{
    public class ApplicationCoreRepository : BaseRepository<ApplicationCore>, IApplicationCoreRepository
    {
        public ApplicationCoreRepository(DatabaseContext dbContext) : base(dbContext) { }

        public async Task<ApplicationCore> FindByLoanNumberAsync(string loanNumber)
        {
            return await _dbSet.Include(a => a.ApplicationFlags)
                 .Include(a => a.ApplicationProcesses)
                 .Include(a => a.AutoApprovalRules)
                 .FirstOrDefaultAsync(a => a.LoanNumber == loanNumber);
        }

        public async Task<ApplicationCore> FindByLoanNumberIncludeApplicationFlagsAsync(string loanNumber)
        {
            return await _dbSet
                .Include(a => a.ApplicationFlags)
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.LoanNumber == loanNumber);
        }

        public async Task<ApplicationCore> FindByLoanNumberIncludeApplicationProcessAsync(string loanNumber)
        {
            return await _dbSet
                .Include(a => a.ApplicationProcesses)
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.LoanNumber == loanNumber);
        }

        public async Task<bool> HasCustomerIdentityFlagNotRaised(string loanNumber)
        {
            List<int> notRaisedFlagsList = new() {

                (int)FlagResultEnum.Ignored,
                (int)FlagResultEnum.Processed,
                (int)FlagResultEnum.Approved
            };
            return await _dbSet
                .Include(a => a.ApplicationProcesses)
                .AsSplitQuery()
                .AnyAsync(x => x.LoanNumber == loanNumber && 
                               x.ApplicationFlags.Any(a => a.FlagCode == FlagCode.CustomerIdentityFlag && notRaisedFlagsList.Contains(a.Status))
                        );
        }

        public async Task<ApplicationCore> FindByLoanNumberIncludeAutoApprovalRulesAsync(string loanNumber)
        {
            return await _dbSet
                .Include(a => a.AutoApprovalRules)
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.LoanNumber == loanNumber);
        }

        public async Task<Dictionary<string, List<ApplicationFlag>>> FindByLoanNumbersIncludeApplicationFlagsAsync(List<string> loanNumbers)
        {
            var applicationCores = await _dbSet
                .Where(ac => loanNumbers.Contains(ac.LoanNumber))
                .Include(ac => ac.ApplicationFlags)
                .ToListAsync();

            return applicationCores
                .GroupBy(ac => ac.LoanNumber)
                .ToDictionary(g => g.Key ?? string.Empty, g => g.SelectMany(ac => ac.ApplicationFlags).ToList());
        }

    }
}
