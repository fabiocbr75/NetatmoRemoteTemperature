import { Injectable } from '@angular/core';
import { SettingData } from '../data/settings';
import { HttpClient, HttpHeaders, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { of as observableOf,  Observable } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';

@Injectable()
export class SettingService extends SettingData {
    
  private endpoint: string = 'http://192.168.2.40:5000/api/';
  private httpOptions: any = {
    headers: new HttpHeaders({
        'Content-Type':  'application/json'
    })
  };

  constructor(private http: HttpClient) {
      super();
  }  

  postClearCache() {
    this.http.post(this.endpoint + 'Setting/ClearCache', null).pipe(map(
      (response) => {
        return response;
      }))
      .subscribe(
        res => {
          console.log(res);
        },
        err => {
          console.log('Error occured');
        }
      );
  }
}
