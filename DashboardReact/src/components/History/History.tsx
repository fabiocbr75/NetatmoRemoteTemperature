import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import type { SensorDataEx, MinMaxData4Day } from '../../types/sensor.types';
import { temperatureService } from '../../services/api.service';
import HistoryGraph from './HistoryGraph';
import HistoryAnalysis from './HistoryAnalysis';
import './History.css';

const History = () => {
  const { mac } = useParams<{ mac: string }>();
  const [sensorDataEx, setSensorDataEx] = useState<SensorDataEx[]>([]);
  const [minMaxData4Day, setMinMaxData4Day] = useState<MinMaxData4Day[]>([]);
  const [showMinMaxAnalysis, setShowMinMaxAnalysis] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Initialize date range
  const now = new Date();
  const startDay = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate(), 0, 0, 0, 0));
  
  const [fromDate, setFromDate] = useState(startDay.toISOString().split('T')[0]);
  const [toDate, setToDate] = useState(now.toISOString().split('T')[0]);

  useEffect(() => {
    if (mac) {
      loadSensorData();
    }
  }, [mac]);

  const loadSensorData = async () => {
    if (!mac) return;

    try {
      setLoading(true);
      setError(null);

      const from = new Date(`${fromDate}T00:00:00Z`).toISOString();
      const to = new Date(`${toDate}T23:59:59Z`).toISOString();

      if (showMinMaxAnalysis) {
        const data = await temperatureService.getMinMaxData4Day(mac, from, to);
        setMinMaxData4Day(data);
      } else {
        const data = await temperatureService.getSensorDataEx(mac, from, to);
        setSensorDataEx(data);
      }
    } catch (err) {
      setError('Failed to load historical data');
      console.error('Error loading history:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDateChange = () => {
    loadSensorData();
  };

  const handleToggleAnalysis = async (checked: boolean) => {
    setShowMinMaxAnalysis(checked);
    
    if (checked && mac) {
      try {
        setLoading(true);
        setError(null);

        // Set date range to last 7 days for min/max analysis
        const tmpDate = new Date();
        const tmpLastWeek = new Date();
        tmpLastWeek.setDate(tmpDate.getDate() - 7);

        const from = tmpLastWeek.toISOString();
        const to = tmpDate.toISOString();

        const data = await temperatureService.getMinMaxData4Day(mac, from, to);
        setMinMaxData4Day(data);
      } catch (err) {
        setError('Failed to load min/max data');
        console.error('Error loading min/max data:', err);
      } finally {
        setLoading(false);
      }
    }
  };

  return (
    <div className="history-container">
      <div className="history-header">
        <h2 className="history-title">History Graph</h2>
        
        <div className="controls">
          <label className="checkbox-label">
            <input
              type="checkbox"
              checked={showMinMaxAnalysis}
              onChange={(e) => handleToggleAnalysis(e.target.checked)}
            />
            <span>Min/Max Analysis</span>
          </label>

          <div className="date-picker">
            <input
              type="date"
              value={fromDate}
              onChange={(e) => setFromDate(e.target.value)}
              className="date-input"
            />
            <span className="date-separator">to</span>
            <input
              type="date"
              value={toDate}
              onChange={(e) => setToDate(e.target.value)}
              className="date-input"
            />
            <button onClick={handleDateChange} className="load-button">
              Load Data
            </button>
          </div>
        </div>
      </div>

      <div className="history-content">
        {loading && (
          <div className="loading-message">Loading data...</div>
        )}

        {error && (
          <div className="error-message">
            {error}
            <button onClick={loadSensorData} className="retry-button">
              Retry
            </button>
          </div>
        )}

        {!loading && !error && (
          <>
            {showMinMaxAnalysis ? (
              <HistoryAnalysis minMaxData4Day={minMaxData4Day} />
            ) : (
              <HistoryGraph sensorDataEx={sensorDataEx} />
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default History;
