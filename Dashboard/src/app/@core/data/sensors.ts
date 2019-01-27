import { Observable } from 'rxjs';

export interface Sensor {
  senderMAC: string;
  senderName: string;
  enabled: boolean;
}

export abstract class SensorsData {
  abstract getSensorsData(): Observable<Sensor[]>;
}