import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';

import { map, catchError, tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class RestService {
  private endpoint: string = 'http://192.168.2.40:5000/api/';
  private httpOptions: any = {
    headers: new HttpHeaders({
      'Content-Type':  'application/json'
    })
  };

  constructor(private http: HttpClient) {
  }

  private extractData(res: Response) {
    let body = res;
    return body || { };
  }

  getSensorData(senderMac: string, from: Date, to: Date): Observable<any> {
    return of(this.data);
    //http://192.168.2.40:5000/api/sensorData/80:7D:3A:57:F2:50?from=2018-12-19T11:00:00Z&to=2019-12-16T00:00:01Z
    let httpParams = new HttpParams();
    httpParams.append('from', from.toISOString());
    httpParams.append('to', to.toISOString());
    return this.http.get(this.endpoint + 'sensorData/' + senderMac, { params: httpParams}).pipe(
      map(this.extractData));
  }

  getSensorMasterData(): Observable<any> {
    return of(this.masterData);
    //http://192.168.2.40:5000/api/sensorMasterData
    return this.http.get(this.endpoint + 'sensorMasterData').pipe(
      map(this.extractData));
  }

  masterData: any = [
    {
        "senderMAC": "80:7D:3A:57:F2:50",
        "senderName": "Sala",
        "roomId": "2935863693",
        "enabled": true,
        "html": "Sala"
    },
    {
        "senderMAC": "41:7D:3A:57:F2:50",
        "senderName": "Cucina",
        "roomId": "2935863623",
        "enabled": false,
        "html": "Cucina"
    }
];


  data: any = [
    {
        "mac": "80:7D:3A:57:F2:50",
        "name": "Sala",
        "temp": 20.8,
        "heatIndex": 20.5,
        "humidity": 69.1,
        "ingestionTimestamp": "2018-12-27T11:13:42Z"
    },
    {
        "mac": "80:7D:3A:57:F2:50",
        "name": "Sala",
        "temp": 20.9,
        "heatIndex": 20.9,
        "humidity": 68.4,
        "ingestionTimestamp": "2018-12-27T11:23:42Z"
    },
    {
        "mac": "80:7D:3A:57:F2:50",
        "name": "Sala",
        "temp": 21,
        "heatIndex": 21.2,
        "humidity": 70.7,
        "ingestionTimestamp": "2018-12-27T11:33:42Z"
    }];
  
}
