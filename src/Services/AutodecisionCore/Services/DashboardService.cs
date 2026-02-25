using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.DTOs;
using AutodecisionCore.Services.Interfaces;
using Elastic.Apm.Model;

namespace AutodecisionCore.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(
            IDashboardRepository dashboardRepository
            )
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<List<AverageTimeProcessingFlagDTO>> GetAverageTimeProcessingFlags(int? timePeriodId)
        {

            var list = new List<AverageTimeProcessingFlagDTO>();

            //DateTime currenteDate = DateTime.Now + new TimeSpan(0, 60, 0);
            DateTime currenteDate = DateTime.Now;
            currenteDate = currenteDate.AddSeconds(-currenteDate.Second);
            currenteDate = currenteDate.AddMilliseconds(-currenteDate.Millisecond);
            currenteDate = currenteDate.AddMicroseconds(-currenteDate.Microsecond);

            var timePeriod = await GetTimePeriods(timePeriodId);

            var countProcessingFlags = await _dashboardRepository.GetCountProcessingFlag(timePeriod.UnitTime);


            var countFlags = countProcessingFlags.GroupBy(_ => _.FlagCode).ToList();


            var datas = new List<(DateTime, DateTime)>();
            for (int i = 0; i < timePeriod.UnitTime; i += timePeriod.Interval)
            {
                DateTime upperDate = currenteDate;
                currenteDate = currenteDate - new TimeSpan(0, timePeriod.Interval, 0);
                DateTime lowerDate = currenteDate;
                datas.Add(new(lowerDate, upperDate));
            }
            foreach (var data in datas)
            {
                int totalByInterval = 0;

                foreach (var flag in countFlags)
                {
                    var flagsInterval = flag.OrderBy(_ => _.ProcessedAt);

                    foreach (var flagInterval in flagsInterval)
                    {
                        var compareDate = DateTime.ParseExact(string.Concat(flagInterval.Hour, ":", flagInterval.Minute), @"H\:m", null);
                        if (compareDate.Ticks >= data.Item1.Ticks && compareDate.Ticks < data.Item2.Ticks)
                        {
                            totalByInterval += flagInterval.TimeConsumedToProcess;
                        }

                        list.Add(new AverageTimeProcessingFlagDTO
                        {
                            Flag = new FlagAverageTimeDTO
                            {
                                Code = flagInterval.FlagCode,
                                Description = $"Flag {flagInterval.FlagCode}",
                            },
                            Range = string.Concat(data.Item1.ToString("HH:mm"), " : ", data.Item2.ToString("HH:mm")),
                            Value = totalByInterval
                        });
                    }
                }
            }

            return list;
        }

        public async Task<TimePeriodDTO> GetTimePeriods(int? id)
        {
            return await _dashboardRepository.GetTimePeriods(id);
        }

        public async Task<List<TimeProcessingApplicationResponseDTO>> GetProcessingApplicationTime(int? timePeriodId)
        {
            //DateTime currenteDate = DateTime.Now + new TimeSpan(0, 60, 0);
            DateTime currenteDate = DateTime.Now;
            currenteDate = currenteDate.AddSeconds(-currenteDate.Second);
            currenteDate = currenteDate.AddMilliseconds(-currenteDate.Millisecond);
            currenteDate = currenteDate.AddMicroseconds(-currenteDate.Microsecond);

            var response = new List<TimeProcessingApplicationResponseDTO>();

            var timePeriod = await GetTimePeriods(timePeriodId);

            var applicationByMinute = await _dashboardRepository.GetApplicationProcessedByMinute(timePeriod.UnitTime);

            var datas = new List<(DateTime, DateTime)>();

            for (int i = 0; i < timePeriod.UnitTime; i += timePeriod.Interval)
            {
                DateTime upperDate = currenteDate;
                currenteDate = currenteDate - new TimeSpan(0, timePeriod.Interval, 0);
                DateTime lowerDate = currenteDate;
                datas.Add(new(lowerDate, upperDate));
            }

            foreach (var data in datas)
            {
                int totalByInterval = 0;

                foreach (var application in applicationByMinute)
                {
                    var compareDate = DateTime.ParseExact(string.Concat(application.Hour, ":", application.Minute), @"H\:m", null);

                    if (compareDate.Ticks >= data.Item1.Ticks && compareDate.Ticks < data.Item2.Ticks)
                    {
                        totalByInterval += application.NumberProcessed;
                    }
                }

                response.Add(new TimeProcessingApplicationResponseDTO()
                {
                    Range = string.Concat(data.Item1.ToString("HH:mm"), " : ", data.Item2.ToString("HH:mm")),
                    Value = totalByInterval
                });
            }

            return response;
        }

        public async Task<AmountApplicatonByStatusResponseDTO> GetAmountApplicationByStatus(int? timePeriodId)
        {
            var timePeriod = await GetTimePeriods(timePeriodId);

            var amounts = await _dashboardRepository.GetAmmountApplicationByStatus(timePeriod.UnitTime);

            var total = 0;
            foreach (var item in amounts)
            {
                total += item.Value;
            }

            var response = new AmountApplicatonByStatusResponseDTO();

            var pendingValues = amounts.Where(x => x.StatusName.Equals("Pending")).FirstOrDefault();
            var autoDenyValues = amounts.Where(x => x.StatusName.Equals("AutoDeny")).FirstOrDefault();
            var autoApprovalValues = amounts.Where(x => x.StatusName.Equals("AutoApproval")).FirstOrDefault();
            var pendingApprovalValues = amounts.Where(x => x.StatusName.Equals("PendingApproval")).FirstOrDefault();
            var pendingDocumentsValues = amounts.Where(x => x.StatusName.Equals("PendingDocuments")).FirstOrDefault();

            response.Pending = new Details() { Value = pendingValues.Value, Percent = ConvertToPercent(pendingValues.Value, total) };
            response.AutoDeny = new Details() { Value = autoDenyValues.Value, Percent = ConvertToPercent(autoDenyValues.Value, total) };
            response.AutoApproval = new Details() { Value = autoApprovalValues.Value, Percent = ConvertToPercent(autoApprovalValues.Value, total) };
            response.PendingApproval = new Details() { Value = pendingApprovalValues.Value, Percent = ConvertToPercent(pendingApprovalValues.Value, total) };
            response.PendingDocuments = new Details() { Value = pendingDocumentsValues.Value, Percent = ConvertToPercent(pendingDocumentsValues.Value, total) };

            return response;
        }

        public string ConvertToPercent(int value, int total)
        {
            if (total == 0)
                return "0 %";

            return ((Math.Round((float)value / (float)total, 2)) * 100).ToString() + " %";
        }

        public async Task<List<AmountApplicationByStatusDetailDTO>> GetAmountApplicationByStatusDetails(int? timePeriodId, int status)
        {
            var response = new List<AmountApplicationByStatusDetailDTO>();

            var timePeriod = await GetTimePeriods(timePeriodId);

            var amountApplicationByStatusDetails = await _dashboardRepository.GetAmmountApplicationDetail(timePeriod.UnitTime, status);

            if (amountApplicationByStatusDetails is not null)
                response = amountApplicationByStatusDetails;

            return response;
        }

        public async Task<List<TimePeriodDTO>> GetTimePeriods()
        {
            var response = new List<TimePeriodDTO>();

            var timePeriods = await _dashboardRepository.GetTimePeriods();

            if (timePeriods is not null)
                response = timePeriods;

            return response;
        }

        public async Task<List<AmountFlagsStatusDTO>> GetAmountFlagsByStatus(int? timePeriodId)
        {
            var response = new List<AmountFlagsStatusDTO>();

            var timePeriod = await GetTimePeriods(timePeriodId);

            var amountFlagsStatus = await _dashboardRepository.GetAmountFlagsByStatus(timePeriod.UnitTime);

            if (amountFlagsStatus is not null)
                response = amountFlagsStatus;

            return response;
        }

        public async Task<List<FlagWithErrorDTO>> GetFlagsWithError(int? timePeriodId)
        {
            var response = new List<FlagWithErrorDTO>();

            var timePeriod = await GetTimePeriods(timePeriodId);

            var flagWithErrors = await _dashboardRepository.GetFlagsWithError(timePeriod.UnitTime);

            if (flagWithErrors is not null)
                response = flagWithErrors;

            return response;
        }
    }
}
