import { Observable } from 'rxjs';

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
}

export abstract class TemperatureHumidityData {
  abstract getTemperatureData(senderMAC: string): Observable<Temperature>;
  abstract getSensorDataEx(senderMAC: string, from: string, to:string): Observable<SensorDataEx[]>;
  abstract getHumidityData(): Observable<Temperature>;
}
