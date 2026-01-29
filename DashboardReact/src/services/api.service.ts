import axios from 'axios';
import type { Sensor, Temperature, SensorDataEx, MinMaxData4Day, Weather } from '../types/sensor.types';

// Default endpoint - can be configured via environment variable
const API_ENDPOINT = import.meta.env.VITE_API_ENDPOINT || 'http://192.168.2.40:5000/api';

const apiClient = axios.create({
  baseURL: API_ENDPOINT,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const sensorService = {
  /**
   * Get all sensor master data
   */
  getSensorMasterData: async (): Promise<Sensor[]> => {
    const response = await apiClient.get<Sensor[]>('/sensorMasterData');
    return response.data;
  },

  /**
   * Switch power for a sensor
   */
  switchPower: async (senderMAC: string, powerValue: boolean): Promise<Sensor> => {
    const response = await apiClient.post<Sensor>(
      `/sensorMasterData/SwitchPower/${senderMAC}?power=${powerValue}`
    );
    return response.data;
  },
};

export const temperatureService = {
  /**
   * Get last temperature data for a specific sensor
   */
  getLastTemperature: async (senderMAC: string): Promise<Temperature> => {
    const response = await apiClient.get<{
      temp: number;
      ingestionTimestamp: string;
      batteryLevel: string;
      scheduledTemperature: number;
    }>(`/sensorData/LastTemperature/${senderMAC}`);
    
    // Transform the response to match Temperature interface
    return {
      min: 10,
      max: 30,
      value: response.data.temp,
      ingestionTimestamp: response.data.ingestionTimestamp,
      batteryLevel: response.data.batteryLevel,
      scheduledTemperature: response.data.scheduledTemperature,
    };
  },

  /**
   * Get sensor data for a specific time range
   */
  getSensorDataEx: async (
    senderMAC: string,
    from: string,
    to: string
  ): Promise<SensorDataEx[]> => {
    const response = await apiClient.get<SensorDataEx[]>(`/sensorData/${senderMAC}`, {
      params: { from, to },
    });
    return response.data;
  },

  /**
   * Get min/max data for each day in a time range
   */
  getMinMaxData4Day: async (
    senderMAC: string,
    from: string,
    to: string
  ): Promise<MinMaxData4Day[]> => {
    interface ApiMinMaxData {
      mac: string;
      min: number;
      max: number;
      day: string;
      minTime: string;
      maxTime: string;
    }
    
    const response = await apiClient.get<ApiMinMaxData[]>(`/minMaxData4Day/${senderMAC}`, {
      params: { from, to },
    });
    
    // Transform the response to match MinMaxData4Day interface
    return response.data.map((item: ApiMinMaxData) => ({
      mac: item.mac,
      minTemp: item.min,
      maxTemp: item.max,
      day: item.day,
      minTime: item.minTime,
      maxTime: item.maxTime,
    }));
  },
};

export const weatherService = {
  /**
   * Get weather data for a specific sensor
   */
  getWeatherData: async (senderMAC: string): Promise<Weather> => {
    // Note: The actual endpoint might need to be adjusted based on the backend API
    const response = await apiClient.get<Weather>(`/weather/${senderMAC}`);
    return response.data;
  },
};
