import { Injectable } from '@angular/core';
import { of as observableOf,  Observable } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { TemperatureHumidityData, Temperature } from '../data/temperature-humidity';
import { HttpClient, HttpHeaders, HttpErrorResponse, HttpParams } from '@angular/common/http';

@Injectable()
export class TemperatureHumidityService extends TemperatureHumidityData {
    
  private endpoint: string = 'http://192.168.2.63:5000/api/';
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
  };

  private extractData(res: Response): Temperature {
    let body: any = res;

    let ret: Temperature = {
        min: 10,
        max: 30,
        value: body.temp
    }


    return ret;
  }

  getTemperatureData(senderMAC: string): Observable<Temperature> {
    return this.http.get(this.endpoint + 'sensorData/LastTemperature/' + senderMAC).pipe(
        map(this.extractData));
    
  }

  getHumidityData(): Observable<Temperature> {
    return observableOf(this.humidityDate);
  }
}
