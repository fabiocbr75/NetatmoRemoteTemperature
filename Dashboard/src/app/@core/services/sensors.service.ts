import { Injectable } from '@angular/core';
import { of as observableOf,  Observable } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { SensorsData, Sensor } from '../data/sensors';
import { HttpClient, HttpHeaders, HttpErrorResponse, HttpParams } from '@angular/common/http';

@Injectable()
export class SensorsService extends SensorsData {
    
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

  private extractData(res: Response): Sensor[] {
    let body: any = res;

    let ret: Sensor[] = body.map(item => { 
      return { 
        senderMAC: item.senderMAC,
        senderName: item.senderName,
        netatmoSetTemp: item.netatmoSetTemp,
        externalSensor: item.externalSensor,
      }
    });
    
    return ret;
  }

  
  private extractSingleData(res: Response): Sensor {
    let item: any = res;

    let ret: Sensor =
      { 
        senderMAC: item.senderMAC,
        senderName: item.senderName,
        netatmoSetTemp: item.netatmoSetTemp,
        externalSensor: item.externalSensor,
      };

      return ret;
    }


  getSensorMasterData(): Observable<Sensor[]> {
    return this.http.get(this.endpoint + 'sensorMasterData').pipe(
        map(this.extractData));
  }
  
  postSwitchPowerSensorMasterData(senderMAC: string, powerValue: boolean): Observable<Sensor> {
    return this.http.post(this.endpoint + 'sensorMasterData/SwitchPower/' + senderMAC + '?power=' + powerValue, null, ).pipe(
        map(this.extractSingleData));
  }

}
