namespace AutodecisionCore.DTOs
{
    public class RedisInformationTiming
    {
        public TimeSpan ElapsedSearchRedisData { get; set; } = TimeSpan.Zero;
        public TimeSpan ElapsedSetRedisData { get; set; } = TimeSpan.Zero;

        public RedisInformationTiming()
        {

        }

        public RedisInformationTiming(TimeSpan elapsedSearchRedisData, TimeSpan elapsedSetRedisData)
        {
            ElapsedSearchRedisData = elapsedSearchRedisData;
            ElapsedSetRedisData = elapsedSetRedisData;
        }

    }
}
