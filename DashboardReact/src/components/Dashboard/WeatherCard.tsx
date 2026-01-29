import { useEffect, useState, useCallback } from 'react';
import type { Weather, WeatherValue } from '../../types/sensor.types';
import { weatherService } from '../../services/api.service';
import './WeatherCard.css';

interface WeatherCardProps {
  senderName: string;
  senderMAC: string;
}

const WeatherCard = ({ senderName, senderMAC }: WeatherCardProps) => {
  const [weatherData, setWeatherData] = useState<Weather>({
    temperature: 0,
    humidity: 0,
    date: '',
    weatherValue: Array(5).fill({ dateOfWeek: '', min: 0, max: 0 }),
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadWeatherData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await weatherService.getWeatherData(senderMAC);
      setWeatherData(data);
    } catch (err) {
      setError('Failed to load weather data');
      console.error('Error loading weather:', err);
    } finally {
      setLoading(false);
    }
  }, [senderMAC]);

  useEffect(() => {
    loadWeatherData();
  }, [loadWeatherData]);

  if (loading) {
    return (
      <div className="weather-card">
        <div className="loading">Loading...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="weather-card">
        <div className="error">{error}</div>
      </div>
    );
  }

  return (
    <div className="weather-card">
      <div className="location">{senderName}</div>
      <div className="date">{weatherData.date}</div>
      <div className="daily-forecast">
        <div className="info">
          <div className="temperature">{weatherData.temperature}°</div>
          <div className="icon">
            <i className="pulse-icon">⚡</i>
          </div>
        </div>
        <div className="details">
          <div className="parameter">
            <span className="parameter-name">humidity</span>
            <span className="parameter-value">{weatherData.humidity}%</span>
          </div>
          <div className="parameter">
            <span className="parameter-name">max</span>
            <span className="parameter-value">{weatherData.weatherValue[0]?.max || 0}</span>
          </div>
          <div className="parameter">
            <span className="parameter-name">min</span>
            <span className="parameter-value">{weatherData.weatherValue[0]?.min || 0}</span>
          </div>
        </div>
      </div>
      <div className="weekly-forecast">
        {weatherData.weatherValue.slice(1, 5).reverse().map((day: WeatherValue, index: number) => (
          <div key={index} className="day">
            <span className="caption">{day.dateOfWeek}</span>
            <span className="temp-max">{day.max}</span>
            <span className="temp-min">{day.min}</span>
          </div>
        ))}
      </div>
    </div>
  );
};

export default WeatherCard;
