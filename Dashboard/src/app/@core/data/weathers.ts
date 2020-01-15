import { Observable } from 'rxjs';

export interface WeatherValue {
  dateOfWeek: string;
  min: number;
  max:number;
}

export interface Weather {
  senderMAC: string;
  senderName: string;
  temperature: number;
  humidity: number;
  date: string;
  weatherValue: WeatherValue[];
}

export abstract class WeathersData {
  abstract getWeatherData(senderMAC: string): Observable<Weather>;
}
