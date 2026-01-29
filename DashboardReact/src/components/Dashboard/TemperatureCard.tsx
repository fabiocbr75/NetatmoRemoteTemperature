import { useEffect, useState, useCallback } from 'react';
import type { Temperature } from '../../types/sensor.types';
import { temperatureService, sensorService } from '../../services/api.service';
import './TemperatureCard.css';

interface TemperatureCardProps {
  senderName: string;
  senderMAC: string;
  temperatureOff: boolean;
}

const TemperatureCard = ({ senderName, senderMAC, temperatureOff }: TemperatureCardProps) => {
  const [temperatureData, setTemperatureData] = useState<Temperature>({
    min: 0,
    max: 100,
    value: 0,
    batteryLevel: '',
    ingestionTimestamp: '',
    scheduledTemperature: 0,
  });
  const [isOff, setIsOff] = useState(temperatureOff);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadTemperatureData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await temperatureService.getLastTemperature(senderMAC);
      setTemperatureData(data);
    } catch (err) {
      setError('Failed to load temperature data');
      console.error('Error loading temperature:', err);
    } finally {
      setLoading(false);
    }
  }, [senderMAC]);

  useEffect(() => {
    loadTemperatureData();
  }, [loadTemperatureData]);

  useEffect(() => {
    setIsOff(temperatureOff);
  }, [temperatureOff]);

  const handlePowerSwitch = async () => {
    try {
      const newPowerValue = !isOff;
      const sensor = await sensorService.switchPower(senderMAC, !newPowerValue);
      setIsOff(!sensor.netatmoSetTemp);
    } catch (err) {
      console.error('Error switching power:', err);
    }
  };

  const formatTime = (timestamp: string) => {
    const date = new Date(timestamp);
    return `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;
  };

  const getInfo = () => {
    if (!temperatureData.ingestionTimestamp) return '';
    const time = formatTime(temperatureData.ingestionTimestamp);
    return `${time} - ${temperatureData.scheduledTemperature}° - ${temperatureData.batteryLevel}v`;
  };

  if (loading) {
    return (
      <div className="temperature-card">
        <div className="loading">Loading...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="temperature-card">
        <div className="error">{error}</div>
      </div>
    );
  }

  return (
    <div className="temperature-card">
      <div className="card-header">
        <h3 className="sensor-name">{senderName}</h3>
        <button
          className={`power-button ${isOff ? 'off' : 'on'}`}
          onClick={handlePowerSwitch}
          aria-label={isOff ? 'Turn on' : 'Turn off'}
        >
          <span className="power-icon">{isOff ? '⭘' : '⚡'}</span>
        </button>
      </div>
      <div className="temperature-display">
        <div className="temperature-value">{temperatureData.value.toFixed(1)}°</div>
        <div className="temperature-info">{getInfo()}</div>
      </div>
    </div>
  );
};

export default TemperatureCard;
