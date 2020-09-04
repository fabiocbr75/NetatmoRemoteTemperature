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
  tCurrentTarget: number;
  setTempSended: boolean;
}

export interface MinMaxData4Day {
  mac: string;
  minTemp: number;
  maxTemp: number;
  day: string;
}


export abstract class TemperatureHumidityData {
  abstract getTemperatureData(senderMAC: string): Observable<Temperature>;
  abstract getSensorDataEx(senderMAC: string, from: string, to:string): Observable<SensorDataEx[]>;
  abstract getMinMaxData4Day(senderMAC: string, from: string, to:string): Observable<MinMaxData4Day[]>;
  abstract getHumidityData(): Observable<Temperature>;
}
