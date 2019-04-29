import { Observable } from 'rxjs';

export interface Sensor {
  senderMAC: string;
  senderName: string;
  netatmoSetTemp: boolean;
  externalSensor: boolean;
}

export abstract class SensorsData {
  abstract getSensorMasterData(): Observable<Sensor[]>;
  abstract postSwitchPowerSensorMasterData(senderMAC: string, powerValue: boolean): Observable<Sensor>;
}
