import { Observable } from 'rxjs';

export interface Sensor {
  senderMAC: string;
  senderName: string;
  enabled: boolean;
  externalSensor: boolean;
}

export abstract class SensorsData {
  abstract getSensorMasterData(): Observable<Sensor[]>;
  abstract postSwitchPowerSensorMasterData(senderMAC: string, powerValue: boolean): Observable<Sensor>;
}
