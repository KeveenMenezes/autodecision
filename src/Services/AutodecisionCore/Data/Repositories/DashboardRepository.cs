using AutodecisionCore.Data.Infra;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.DTOs;
using Dapper;
using MySqlConnector;
using System.Text;

namespace AutodecisionCore.Data.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DatabaseConnectionHelper _databaseConnectionHelper;

        public DashboardRepository(DatabaseConnectionHelper databaseConnectionHelper)
        {
            _databaseConnectionHelper = databaseConnectionHelper;
        }

        public async Task<List<CountProcessingFlagDTO>> GetCountProcessingFlag(int timeUnit)
        {

            await using (var connection = new MySqlConnection(_databaseConnectionHelper.BmgMoneyConectionString))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine($@"                                                                
                                SELECT t1.hour, t1.minute, sum(TimeConsumedToProcess) AS TimeConsumedToProcess, t1.processed_at AS ProcessedAt, COUNT(1) AS CountFlag, t1.flag_code AS FlagCode
                                FROM 
                                (
                                SELECT app.processed_at, 
                                    HOUR(app.processed_at) AS hour,
                                    MINUTE(app.processed_at) AS minute,
                                    MINUTE(app.processed_at) AS TimeConsumedToProcess,
                                    app.flag_code
                                FROM autodecision_core.application_flags app
                                WHERE app.processed_at > SYSDATE() - INTERVAL {timeUnit} minute 
                                ) t1
                                GROUP BY t1.hour, t1.minute, t1.flag_code;
                                ");                

                return  connection.Query<CountProcessingFlagDTO>(sql.ToString()).ToList();
            }
        }

        public async Task<List<TimePeriodDTO>> GetTimePeriods()
        {
            await using (var connection = new MySqlConnection(_databaseConnectionHelper.BmgMoneyConectionString))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine($@"SELECT Id,description AS Description,unit_time AS UnitTime ,`interval` AS 'Interval',is_default as IsDefault FROM autodecision_core.time_periods");

                return connection.Query<TimePeriodDTO>(sql.ToString()).ToList();
            }
        }

        public async Task<List<AmountFlagsStatusDTO>> GetAmountFlagsByStatus(int timeUnit)
        {
            await using (var connection = new MySqlConnection(_databaseConnectionHelper.BmgMoneyConectionString))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine($@"                                                                
                                SELECT t1.status as Status, t1.flag_code as Code, COUNT(1) AS Value, CONCAT('Flag', ' ' ,t1.flag_code) as Description
                                FROM 
                                (
                                SELECT app.status,
                                    app.flag_code
                                FROM autodecision_core.application_flags app
                                WHERE app.processed_at > SYSDATE() - INTERVAL {timeUnit} minute 
                                ) t1
                                 group by t1.flag_code, t1.status;
                                ");

                return connection.Query<AmountFlagsStatusDTO>(sql.ToString()).ToList();
            }
        }

        public async Task<List<FlagWithErrorDTO>> GetFlagsWithError(int timeUnit)
        {
            await using (var connection = new MySqlConnection(_databaseConnectionHelper.BmgMoneyConectionString))
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine($@"                                                                
                                        select b.loan_number AS LoanNumber, a.flag_code AS Flag, a.description from autodecision_core.application_flags a
                                        inner join autodecision_core.application_cores b on a.application_core_id = b.id
                                        WHERE a.status = 6 AND a.processed_at > SYSDATE() - INTERVAL {timeUnit} minute;
                                ");

                return connection.Query<FlagWithErrorDTO>(sql.ToString()).ToList();
            }
        }

        public async Task<TimePeriodDTO> GetTimePeriods(int? id)
        {
            await using (var connection = new MySqlConnection(_databaseConnectionHelper.BmgMoneyConectionString))
            {
                string filter = (id == null) ? "WHERE is_default = 1" : $"WHERE ID = {id}";
                var timePeriod = new TimePeriodDTO();
                StringBuilder sql = new StringBuilder();
                sql.AppendLine($"SELECT description AS 'Description' , unit_time AS 'UnitTime' , `interval` AS 'Interval', is_default AS 'IsDefault' FROM autodecision_core.time_periods");
                sql.AppendLine(filter);
                timePeriod = connection.Query<TimePeriodDTO>(sql.ToString()).FirstOrDefault();
                return timePeriod;
            }
        }

        public async Task<List<ApplicationsProcessedByMinuteDTO>> GetApplicationProcessedByMinute(int timeUnit)
        {
            await using (var connection = new MySqlConnection(_databaseConnectionHelper.BmgMoneyConectionString))
            {
                var list = new List<ApplicationsProcessedByMinuteDTO>();
                StringBuilder sql = new StringBuilder();
                sql.AppendLine($"SELECT t1.hour AS 'Hour', t1.minute as 'Minute', COUNT(1) AS 'NumberProcessed'");
                sql.AppendLine($"FROM (SELECT app.processed_at,");
                sql.AppendLine($"      HOUR(app.processed_at) AS hour,");
                sql.AppendLine($"      MINUTE(app.processed_at) AS minute");
                sql.AppendLine($"      FROM autodecision_core.application_cores app");
                sql.AppendLine($"WHERE app.processed_at > SYSDATE() - INTERVAL {timeUnit} minute ) t1");
                sql.AppendLine($"GROUP BY t1.hour, t1.minute");
                sql.AppendLine($"ORDER BY t1.hour desc,t1.minute desc");
                list = connection.Query<ApplicationsProcessedByMinuteDTO>(sql.ToString()).ToList();
                return list;
            }
        }

        public async Task<List<AmountApplicationByStatusDTO>> GetAmmountApplicationByStatus(int timeUnit)
        {
            await using (var connection = new MySqlConnection(_databaseConnectionHelper.BmgMoneyConectionString))
            {
                var list = new List<AmountApplicationByStatusDTO>();
                StringBuilder sql = new StringBuilder();
                sql.AppendLine($"(SELECT COUNT(*) AS Value , 'Pending' AS StatusName FROM autodecision_core.application_cores");
                sql.AppendLine($" WHERE status = 0 AND processed_at > SYSDATE() - INTERVAL {timeUnit} MINUTE) UNION ALL");
                sql.AppendLine($"(SELECT COUNT(*) AS Value , 'AutoDeny' AS StatusName FROM autodecision_core.application_cores ");
                sql.AppendLine($" WHERE status = 1 AND processed_at > SYSDATE() - INTERVAL {timeUnit} MINUTE) UNION ALL");
                sql.AppendLine($"(SELECT COUNT(*) AS Value , 'AutoApproval' AS StatusName FROM autodecision_core.application_cores ");
                sql.AppendLine($" WHERE status = 2 AND processed_at > SYSDATE() - INTERVAL {timeUnit} MINUTE) UNION ALL");
                sql.AppendLine($"(SELECT COUNT(*) AS Value , 'PendingApproval' AS StatusName FROM autodecision_core.application_cores ");
                sql.AppendLine($" WHERE status = 3 AND processed_at > SYSDATE() - INTERVAL {timeUnit} MINUTE) UNION ALL");
                sql.AppendLine($"(SELECT COUNT(*) AS Value , 'PendingDocuments' AS StatusName FROM autodecision_core.application_cores");
                sql.AppendLine($" WHERE status = 4 AND processed_at > SYSDATE() - INTERVAL {timeUnit} MINUTE)");

                list = connection.Query<AmountApplicationByStatusDTO>(sql.ToString()).ToList();
                return list;
            }
        }

        public async Task<List<AmountApplicationByStatusDetailDTO>> GetAmmountApplicationDetail(int timeUnit, int status)
        {
            await using (var connection = new MySqlConnection(_databaseConnectionHelper.BmgMoneyConectionString))
            {
                var list = new List<AmountApplicationByStatusDetailDTO>();
                StringBuilder sql = new StringBuilder();
                sql.AppendLine($"SELECT loan_number AS LoanNumber, status AS Status ,employer_name AS EmployerName , customer_name AS CustomerName , state_abbreviation AS State FROM autodecision_core.application_cores");
                sql.AppendLine($"WHERE status = {status} AND processed_at  > SYSDATE() - INTERVAL {timeUnit} MINUTE");
                sql.AppendLine($"ORDER BY processed_at DESC");
                list = connection.Query<AmountApplicationByStatusDetailDTO>(sql.ToString()).ToList();
                return list;
            }
        }

    }
}
