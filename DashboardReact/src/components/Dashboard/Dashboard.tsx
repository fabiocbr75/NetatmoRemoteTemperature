import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Sensor } from '../../types/sensor.types';
import { sensorService } from '../../services/api.service';
import TemperatureCard from './TemperatureCard';
import WeatherCard from './WeatherCard';
import './Dashboard.css';

const Dashboard = () => {
  const [sensors, setSensors] = useState<Sensor[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    loadSensors();
  }, []);

  const loadSensors = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await sensorService.getSensorMasterData();
      setSensors(data);
    } catch (err) {
      setError('Failed to load sensor data');
      console.error('Error loading sensors:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCardClick = (mac: string) => {
    navigate(`/history/${mac}`);
  };

  if (loading) {
    return (
      <div className="dashboard-container">
        <div className="loading-message">Loading sensors...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="dashboard-container">
        <div className="error-message">{error}</div>
        <button onClick={loadSensors} className="retry-button">
          Retry
        </button>
      </div>
    );
  }

  const externalSensors = sensors.filter((s) => s.externalSensor);
  const internalSensors = sensors.filter((s) => !s.externalSensor);

  return (
    <div className="dashboard-container">
      <h2 className="dashboard-title">IoT Dashboard</h2>
      <div className="sensors-grid">
        {externalSensors.map((sensor) => (
          <div
            key={sensor.senderMAC}
            className="sensor-card-wrapper"
            onClick={() => handleCardClick(sensor.senderMAC)}
          >
            <WeatherCard senderName={sensor.senderName} senderMAC={sensor.senderMAC} />
          </div>
        ))}
        {internalSensors.map((sensor) => (
          <div
            key={sensor.senderMAC}
            className="sensor-card-wrapper"
            onClick={() => handleCardClick(sensor.senderMAC)}
          >
            <TemperatureCard
              senderName={sensor.senderName}
              senderMAC={sensor.senderMAC}
              temperatureOff={!sensor.netatmoSetTemp}
            />
          </div>
        ))}
      </div>
    </div>
  );
};

export default Dashboard;
