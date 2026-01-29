import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  TimeScale,
} from 'chart.js';
import { Line } from 'react-chartjs-2';
import 'chartjs-adapter-date-fns';
import type { SensorDataEx } from '../../types/sensor.types';
import './HistoryGraph.css';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  TimeScale
);

interface HistoryGraphProps {
  sensorDataEx: SensorDataEx[];
}

const HistoryGraph = ({ sensorDataEx }: HistoryGraphProps) => {
  const processData = () => {
    let maxValue = 0;
    let minValue = 100;
    const tempArray: Array<{ y: number; t: string }> = [];
    const tValveArray: Array<{ y: number; t: string }> = [];
    const tSchedArray: Array<{ y: number; t: string }> = [];
    const tCurrentArray: Array<{ y: number; t: string }> = [];
    const humidityArray: Array<{ y: number; t: string }> = [];
    const pointRadius0: number[] = [];
    const pointRadius1: number[] = [];
    const pointRadius2: number[] = [];
    const pointRadius3: number[] = [];
    const pointRadius4: number[] = [];

    for (let i = 0; i < sensorDataEx.length; i++) {
      if (i > 0) {
        const dateNewValue = new Date(sensorDataEx[i].ingestionTimestamp);
        const dateOldValue = new Date(sensorDataEx[i - 1].ingestionTimestamp);
        const diff = Math.abs(dateNewValue.getTime() - dateOldValue.getTime());
        const minutes = Math.floor(diff / 60000);

        if (minutes > 15) {
          const dateIntermediate = new Date(dateNewValue.getTime() - 10 * 60 * 1000);
          const dateIso = dateIntermediate.toISOString();

          tempArray.push({ y: sensorDataEx[i - 1].temp, t: dateIso });
          tValveArray.push({ y: sensorDataEx[i - 1].tValve, t: dateIso });
          tSchedArray.push({ y: sensorDataEx[i - 1].tScheduledTarget, t: dateIso });
          tCurrentArray.push({ y: sensorDataEx[i - 1].tCurrentTarget, t: dateIso });
          humidityArray.push({ y: sensorDataEx[i - 1].humidity, t: dateIso });

          pointRadius0.push(1);
          pointRadius1.push(1);
          pointRadius2.push(1);
          pointRadius3.push(1);
          pointRadius4.push(1);
        }
      }

      tempArray.push({ y: sensorDataEx[i].temp, t: sensorDataEx[i].ingestionTimestamp });
      tValveArray.push({ y: sensorDataEx[i].tValve, t: sensorDataEx[i].ingestionTimestamp });
      tSchedArray.push({ y: sensorDataEx[i].tScheduledTarget, t: sensorDataEx[i].ingestionTimestamp });
      tCurrentArray.push({ y: sensorDataEx[i].tCurrentTarget, t: sensorDataEx[i].ingestionTimestamp });
      humidityArray.push({ y: sensorDataEx[i].humidity, t: sensorDataEx[i].ingestionTimestamp });

      pointRadius0.push(sensorDataEx[i].setTempSended ? 4 : 1);
      pointRadius1.push(1);
      pointRadius2.push(1);
      pointRadius3.push(1);
      pointRadius4.push(1);

      // Calculate min/max
      if (maxValue < sensorDataEx[i].temp && sensorDataEx[i].temp > 0) maxValue = sensorDataEx[i].temp;
      if (maxValue < sensorDataEx[i].tValve && sensorDataEx[i].tValve > 0) maxValue = sensorDataEx[i].tValve;
      if (maxValue < sensorDataEx[i].tScheduledTarget && sensorDataEx[i].tScheduledTarget > 0)
        maxValue = sensorDataEx[i].tScheduledTarget;
      if (maxValue < sensorDataEx[i].tCurrentTarget && sensorDataEx[i].tCurrentTarget > 0)
        maxValue = sensorDataEx[i].tCurrentTarget;

      if (minValue > sensorDataEx[i].temp && sensorDataEx[i].temp > 0) minValue = sensorDataEx[i].temp;
      if (minValue > sensorDataEx[i].tValve && sensorDataEx[i].tValve > 0) minValue = sensorDataEx[i].tValve;
      if (minValue > sensorDataEx[i].tScheduledTarget && sensorDataEx[i].tScheduledTarget > 0)
        minValue = sensorDataEx[i].tScheduledTarget;
      if (minValue > sensorDataEx[i].tCurrentTarget && sensorDataEx[i].tCurrentTarget > 0)
        minValue = sensorDataEx[i].tCurrentTarget;
    }

    return {
      datasets: tempArray,
      tValveDatasets: tValveArray,
      tSchedDatasets: tSchedArray,
      tCurrentDatasets: tCurrentArray,
      humidityDatasets: humidityArray,
      pointRadius0,
      pointRadius1,
      pointRadius2,
      pointRadius3,
      pointRadius4,
      maxValue,
      minValue,
    };
  };

  const processedData = processData();

  const data = {
    datasets: [
      {
        label: 'Temp',
        data: processedData.datasets,
        backgroundColor: 'rgba(255, 61, 113, 0.3)',
        borderColor: '#ff3d71',
        tension: 0,
        fill: false,
        pointRadius: processedData.pointRadius0,
        yAxisID: 'yTemperature',
      },
      {
        label: 'T Valve',
        data: processedData.tValveDatasets,
        backgroundColor: 'rgba(51, 102, 255, 0.3)',
        borderColor: '#3366ff',
        tension: 0,
        fill: false,
        pointRadius: processedData.pointRadius1,
        hidden: true,
        yAxisID: 'yTemperature',
      },
      {
        label: 'T Scheduled',
        data: processedData.tSchedDatasets,
        backgroundColor: 'rgba(0, 214, 143, 0.3)',
        borderColor: '#00d68f',
        borderDash: [10, 5],
        tension: 0,
        fill: false,
        pointRadius: processedData.pointRadius2,
        yAxisID: 'yTemperature',
      },
      {
        label: 'T CurrentSched',
        data: processedData.tCurrentDatasets,
        backgroundColor: 'rgba(0, 214, 143, 0.3)',
        borderColor: '#00d68f',
        borderDash: [10, 5],
        tension: 0,
        fill: false,
        pointRadius: processedData.pointRadius3,
        hidden: true,
        yAxisID: 'yTemperature',
      },
      {
        label: 'Humidity',
        data: processedData.humidityDatasets,
        backgroundColor: 'rgba(0, 214, 143, 0.3)',
        borderColor: '#00d68f',
        tension: 0,
        fill: false,
        pointRadius: processedData.pointRadius4,
        yAxisID: 'yHumidity',
      },
    ],
  };

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        type: 'time' as const,
        time: {
          unit: 'minute' as const,
          tooltipFormat: 'dd/MM - HH:mm',
          displayFormats: {
            minute: 'dd/MM - HH:mm',
          },
        },
        grid: {
          display: true,
        },
        ticks: {
          autoSkip: true,
        },
      },
      yTemperature: {
        position: 'left' as const,
        grid: {
          display: true,
        },
        ticks: {
          stepSize: 0.5,
          callback: (value: number | string) => `${value}°`,
        },
        min: processedData.minValue - 0.5,
        max: processedData.maxValue + 0.5,
      },
      yHumidity: {
        position: 'right' as const,
        grid: {
          display: false,
        },
        ticks: {
          stepSize: 5,
          callback: (value: number | string) => `${value}%`,
        },
        min: 15,
        max: 95,
      },
    },
    plugins: {
      legend: {
        display: true,
        position: 'top' as const,
      },
      tooltip: {
        mode: 'index' as const,
        intersect: false,
      },
    },
  };

  return (
    <div className="history-graph">
      <Line data={data} options={options} />
    </div>
  );
};

export default HistoryGraph;
