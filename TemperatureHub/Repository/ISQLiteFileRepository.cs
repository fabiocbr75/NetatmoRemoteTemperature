namespace TemperatureHub.Repository
{
    public interface ISQLiteFileRepository
    {
        bool ContainsItem(string id);
    }
}