import { Observable } from 'rxjs';

export interface Temperature {
  value: number;
  min: number;
  max: number;
}

export abstract class TemperatureHumidityData {
  abstract getTemperatureData(senderMAC: string): Observable<Temperature>;
  abstract getHumidityData(): Observable<Temperature>;
}
