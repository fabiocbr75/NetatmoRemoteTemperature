using TemperatureHub.Models;

namespace TemperatureHub.Process
{
    public interface IProcessData
    {
        void Add(SensorData data);
        void Dispose();
    }
}