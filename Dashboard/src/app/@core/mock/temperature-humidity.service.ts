import { Injectable } from '@angular/core';
import { of as observableOf,  Observable } from 'rxjs';
import { TemperatureHumidityData, Temperature, SensorDataEx, MinMaxData4Day } from '../data/temperature-humidity';

@Injectable()
export class TemperatureHumidityService extends TemperatureHumidityData {

  private temperatureDate: Temperature = {
    value: 24,
    min: 12,
    max: 30,
    ingestionTimestamp: '',
    batteryLevel: '',
    scheduledTemperature: 0,
  };

  private humidityDate: Temperature = {
    value: 87,
    min: 0,
    max: 100,
    ingestionTimestamp: '',
    batteryLevel: '',
    scheduledTemperature: 0,
  };

  private sensorDataEx: SensorDataEx[] = [{
    mac: '',
    name: '',
    temp: 0,
    heatIndex: 0,
    humidity: 0,
    batteryLevel: '',
    ingestionTimestamp: '',
    tValve: 0,
    tScheduledTarget: 0,
    tCurrentTarget: 0,
    setTempSended: false,
  }]

  private minMaxData4Day: MinMaxData4Day[] = [{
    mac: '',
    minTemp: 0,
    maxTemp: 100,
    day: ''
  }];

  getTemperatureData(senderMAC: string): Observable<Temperature> {
    return observableOf(this.temperatureDate);
  }

  getSensorDataEx(senderMAC: string, from: string, to:string): Observable<SensorDataEx[]> {
    return observableOf(this.sensorDataEx);
  }

  getHumidityData(): Observable<Temperature> {
    return observableOf(this.humidityDate);
  }

  getMinMaxData4Day(senderMAC: string, from: string, to:string): Observable<MinMaxData4Day[]>{
    return observableOf(this.minMaxData4Day);
  }
}
