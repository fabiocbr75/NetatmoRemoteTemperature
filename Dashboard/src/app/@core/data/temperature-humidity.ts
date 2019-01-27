import { Observable } from 'rxjs';

export interface Temperature {
  value: number;
  min: number;
  max: number;
  ingestionTimestamp: string;
  batteryLevel: string;
}

export abstract class TemperatureHumidityData {
  abstract getTemperatureData(senderMAC: string): Observable<Temperature>;
  abstract getHumidityData(): Observable<Temperature>;
}
