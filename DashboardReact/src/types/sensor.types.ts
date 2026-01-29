export interface Sensor {
  senderMAC: string;
  senderName: string;
  netatmoSetTemp: boolean;
  externalSensor: boolean;
}

export interface Temperature {
  value: number;
  min: number;
  max: number;
  ingestionTimestamp: string;
  batteryLevel: string;
  scheduledTemperature: number;
}

export interface SensorDataEx {
  mac: string;
  name: string;
  temp: number;
  heatIndex: number;
  humidity: number;
  batteryLevel: string;
  ingestionTimestamp: string;
  tValve: number;
  tScheduledTarget: number;
  tCurrentTarget: number;
  setTempSended: boolean;
}

export interface MinMaxData4Day {
  mac: string;
  minTemp: number;
  maxTemp: number;
  day: string;
  minTime: string;
  maxTime: string;
}

export interface Weather {
  temperature: number;
  humidity: number;
  date: string;
  weatherValue: WeatherValue[];
}

export interface WeatherValue {
  dateOfWeek: string;
  min: number;
  max: number;
}
