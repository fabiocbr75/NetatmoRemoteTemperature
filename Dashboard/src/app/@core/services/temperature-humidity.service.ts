import { Injectable } from '@angular/core';
import { of as observableOf,  Observable } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { TemperatureHumidityData, Temperature, SensorDataEx, MinMaxData4Day } from '../data/temperature-humidity';
import { HttpClient, HttpHeaders, HttpErrorResponse, HttpParams } from '@angular/common/http';

@Injectable()
export class TemperatureHumidityService extends TemperatureHumidityData {
    
  private endpoint: string = 'http://192.168.2.40:5000/api/';
  // private endpoint: string = 'http://localhost:5000/api/';
  private httpOptions: any = {
    headers: new HttpHeaders({
        'Content-Type':  'application/json'
    })
  };

  constructor(private http: HttpClient) {
      super();
  }  

  private humidityDate: Temperature = {
    value: 87,
    min: 0,
    max: 100,
    ingestionTimestamp: '',
    batteryLevel: '',
    scheduledTemperature: 0,
  };

  private extractData(res: Response): Temperature {
    let body: any = res;

    let ret: Temperature = {
        min: 10,
        max: 30,
        value: body.temp,
        ingestionTimestamp: body.ingestionTimestamp,
        batteryLevel: body.batteryLevel,
        scheduledTemperature: body.scheduledTemperature,
    }

    return ret;
  }

  private extractSensorDataEx(res: Response): SensorDataEx[] {
    let body: any = res;

    let ret: SensorDataEx[] = body.map(item => { 
      return { 
        mac: item.mac,
        name: item.name,
        temp: item.temp,
        heatIndex: item.heatIndex,
        humidity: item.humidity,
        batteryLevel: item.batteryLevel,
        ingestionTimestamp: item.ingestionTimestamp,
        tValve: item.tValve,
        tScheduledTarget: item.tScheduledTarget,
        tCurrentTarget: item.tCurrentTarget,
        setTempSended: item.setTempSended,
      }
    }); 

    return ret;
  }

  private extractMinMaxData(res: Response): MinMaxData4Day[] {
    let body: any = res;

    let ret: MinMaxData4Day[] = body.map(item => { 
      return { 
        mac: item.mac,
        minTemp: item.min,
        maxTemp: item.max,
        day: item.day,
        minTime: item.minTime,
        maxTime: item.maxTime
      }
    }); 

    return ret;
  }

  getTemperatureData(senderMAC: string): Observable<Temperature> {
    return this.http.get(this.endpoint + 'sensorData/LastTemperature/' + senderMAC).pipe(
        map(this.extractData));
  }
  getSensorDataEx(senderMAC: string, from: string, to:string): Observable<SensorDataEx[]> {
    return this.http.get(this.endpoint + 'sensorData/' + senderMAC, { params: { from: from, to: to }}).pipe(
      map(this.extractSensorDataEx));
  }
  getMinMaxData4Day(senderMAC: string, from: string, to:string): Observable<MinMaxData4Day[]> {
    return this.http.get(this.endpoint + 'minMaxData4Day/' + senderMAC, { params: { from: from, to: to }}).pipe(
      map(this.extractMinMaxData));
  }
  getHumidityData(): Observable<Temperature> {
    return observableOf(this.humidityDate);
  }
}
