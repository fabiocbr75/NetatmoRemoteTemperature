import { Injectable } from '@angular/core';
import { of as observableOf,  Observable } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { WeathersData, Weather, WeatherValue } from '../data/weathers';
import { HttpClient, HttpHeaders, HttpErrorResponse, HttpParams } from '@angular/common/http';

@Injectable()
export class WeatherService extends WeathersData {
    
  private endpoint: string = 'http://192.168.2.40:5000/api/';
  private httpOptions: any = {
    headers: new HttpHeaders({
        'Content-Type':  'application/json'
    })
  };

  constructor(private http: HttpClient) {
      super();
  }  

  private extractData(res: Response): Weather {
    let body: any = res;

    
    let weatherValue: WeatherValue[] = new Array();
    let i = 0;
    for (var item of body.weatherInfo) {
      weatherValue[i] = {
        dateOfWeek: item.dateOfWeek,
        min: item.min,
        max: item.max,
      };
      i++;
    }

    let ret: Weather = { 
        senderMAC: body.senderMAC,
        senderName: body.senderName,
        temperature: body.temperature,
        date: body.date,
        weatherValue: weatherValue,
      };

  
    return ret;
  }

  getWeatherData(senderMAC: string): Observable<Weather> {
    return this.http.get(this.endpoint + 'Weather/' + senderMAC).pipe(
        map(this.extractData));
  }

}
