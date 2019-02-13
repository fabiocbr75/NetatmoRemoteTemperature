import { Component, OnDestroy, Input, OnInit } from '@angular/core';
import { NbThemeService } from '@nebular/theme';
import { Weather, WeathersData, WeatherValue } from '../../../@core/data/weathers';
import { takeWhile } from 'rxjs/operators';


@Component({
  selector: 'ngx-weather',
  styleUrls: ['./weather.component.scss'],
  templateUrl: './weather.component.html',
})

export class WeatherComponent implements OnDestroy, OnInit {
  @Input() senderMAC: string;
  @Input() senderName: string;
  temperature: string;
  date: string;
  lastWeather: WeatherValue[] = [ {dateOfWeek: "", min: 0, max: 0},
                                  {dateOfWeek: "", min: 0, max: 0},
                                  {dateOfWeek: "", min: 0, max: 0},
                                  {dateOfWeek: "", min: 0, max: 0},
                                  {dateOfWeek: "", min: 0, max: 0}];

  private alive = true;
  colors: any;

  constructor(private theme: NbThemeService, private weatherService: WeathersData) {
      this.theme.getJsTheme()
        .pipe(takeWhile(() => this.alive))
        .subscribe(config => {
          this.colors = config.variables;
        });

  }

  ngOnDestroy() {
    this.alive = false;
  }

  ngOnInit() {
    this.weatherService.getWeatherData(this.senderMAC)
    .subscribe((weatherData: Weather) => {
      this.temperature = weatherData.temperature + "°";
      this.date = weatherData.date;
      this.lastWeather = weatherData.weatherValue;
    });    
  }
}
