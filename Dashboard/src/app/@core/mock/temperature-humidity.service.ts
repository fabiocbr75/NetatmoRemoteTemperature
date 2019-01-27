import { Injectable } from '@angular/core';
import { of as observableOf,  Observable } from 'rxjs';
import { TemperatureHumidityData, Temperature } from '../data/temperature-humidity';

@Injectable()
export class TemperatureHumidityService extends TemperatureHumidityData {

  private temperatureDate: Temperature = {
    value: 24,
    min: 12,
    max: 30,
    ingestionTimestamp: '',
    batteryLevel: '',
  };

  private humidityDate: Temperature = {
    value: 87,
    min: 0,
    max: 100,
    ingestionTimestamp: '',
    batteryLevel: '',
  };

  getTemperatureData(senderMAC: string): Observable<Temperature> {
    return observableOf(this.temperatureDate);
  }

  getHumidityData(): Observable<Temperature> {
    return observableOf(this.humidityDate);
  }
}
