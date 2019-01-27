import { Component, OnDestroy, Input, OnInit } from '@angular/core';
import { NbThemeService } from '@nebular/theme';
import { Temperature, TemperatureHumidityData } from '../../../@core/data/temperature-humidity';
import { takeWhile } from 'rxjs/operators';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'ngx-temperature',
  styleUrls: ['./temperature.component.scss'],
  templateUrl: './temperature.component.html',
})
export class TemperatureComponent implements OnDestroy, OnInit {

  private alive = true;

  @Input() senderName :string = ''
  @Input() senderMAC :string = ''

  temperatureData: Temperature;
  temperature: number;
  info: string = '';
  temperatureOff = false;

  colors: any;
  themeSubscription: any;

  constructor(private theme: NbThemeService,
              private temperatureHumidityService: TemperatureHumidityData) {
    this.theme.getJsTheme()
      .pipe(takeWhile(() => this.alive))
      .subscribe(config => {
      this.colors = config.variables;
    });

  }
  
  ngOnInit() {
    forkJoin(
      this.temperatureHumidityService.getTemperatureData(this.senderMAC),
      this.temperatureHumidityService.getHumidityData(),
    )
      .subscribe(([temperatureData, humidityData]: [Temperature, Temperature]) => {
        this.temperatureData = temperatureData;
        this.temperature = this.temperatureData.value;
        var date = new Date(this.temperatureData.ingestionTimestamp);
        var time = date.getHours() + ":" + date.getMinutes();
        this.info =  time + ' - ' + temperatureData.batteryLevel + 'v';
      });
  }
  
  ngOnDestroy() {
    this.alive = false;
  }
}
