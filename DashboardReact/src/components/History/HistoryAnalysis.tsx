import type { MinMaxData4Day } from '../../types/sensor.types';
import './HistoryAnalysis.css';

interface HistoryAnalysisProps {
  minMaxData4Day: MinMaxData4Day[];
}

const HistoryAnalysis = ({ minMaxData4Day }: HistoryAnalysisProps) => {
  if (!minMaxData4Day || minMaxData4Day.length === 0) {
    return (
      <div className="history-analysis">
        <p className="no-data">No data available for the selected period.</p>
      </div>
    );
  }

  return (
    <div className="history-analysis">
      <table className="analysis-table">
        <thead>
          <tr>
            <th>Day</th>
            <th>Min Temp</th>
            <th>Min Time</th>
            <th>Max Temp</th>
            <th>Max Time</th>
          </tr>
        </thead>
        <tbody>
          {minMaxData4Day.map((data, index) => (
            <tr key={index}>
              <td>{new Date(data.day).toLocaleDateString()}</td>
              <td className="temp-value temp-min">{data.minTemp.toFixed(1)}°</td>
              <td>{new Date(data.minTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</td>
              <td className="temp-value temp-max">{data.maxTemp.toFixed(1)}°</td>
              <td>{new Date(data.maxTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default HistoryAnalysis;
