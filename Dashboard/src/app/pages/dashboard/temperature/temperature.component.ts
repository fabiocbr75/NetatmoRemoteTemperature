import { Component, OnDestroy, Input, OnInit } from '@angular/core';
import { NbThemeService } from '@nebular/theme';
import { Temperature, TemperatureHumidityData } from '../../../@core/data/temperature-humidity';
import { Sensor, SensorsData } from '../../../@core/data/sensors';
import { takeWhile } from 'rxjs/operators';

@Component({
  selector: 'ngx-temperature',
  styleUrls: ['./temperature.component.scss'],
  templateUrl: './temperature.component.html',
})
export class TemperatureComponent implements OnDestroy, OnInit {

  private alive = true;

  @Input() senderName: string = ''
  @Input() senderMAC: string = ''
  @Input() temperatureOff: boolean = false;

  temperatureData: Temperature = { min : 0, max: 100, value: 0, batteryLevel: '', ingestionTimestamp: '', scheduledTemperature: 0};
  temperature: number = 0.0;
  info: string = '';
  

  colors: any;
  themeSubscription: any;

  constructor(private theme: NbThemeService,
              private temperatureHumidityService: TemperatureHumidityData,
              private sensorDataService: SensorsData) {
    this.theme.getJsTheme()
      .pipe(takeWhile(() => this.alive))
      .subscribe(config => {
      this.colors = config.variables;
    });

  }
  
  ngOnInit() {
    this.temperatureHumidityService.getTemperatureData(this.senderMAC)
      .subscribe((temperatureData: Temperature) => {
        this.temperatureData = temperatureData;
        this.temperature = this.temperatureData.value;
        var date = new Date(this.temperatureData.ingestionTimestamp);
        var time = ('0'+ date.getHours()).slice(-2) + ":" + ('0'+ date.getMinutes()).slice(-2);
        this.info =  time + ' - ' + temperatureData.scheduledTemperature + '° - ' + temperatureData.batteryLevel + 'v';
      });
  }

  switchPower(event) {
    this.sensorDataService.postSwitchPowerSensorMasterData(this.senderMAC, event).subscribe((sensorData: Sensor) => {
      this.temperatureOff = !sensorData.netatmoSetTemp;
    });
  }

  
  
  ngOnDestroy() {
    this.alive = false;
  }
}
