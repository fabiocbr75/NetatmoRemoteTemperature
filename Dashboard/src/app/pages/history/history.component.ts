import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from "@angular/router";
import { SensorsData, Sensor } from '../../@core/data/sensors';
import { TemperatureHumidityData, SensorDataEx } from '../../@core/data/temperature-humidity';


@Component({
  selector: 'ngx-history',
  styleUrls: ['./history.component.scss'],  
  templateUrl:  './history.component.html',
})
export class HistoryComponent  implements OnInit {
  private mac: string;
  private from: string;
  private to: string;
  sensorDataEx: SensorDataEx[];

  constructor(private route: ActivatedRoute, private temperatureHumidityService: TemperatureHumidityData) { 
    var now = new Date;
    this.to = now.toISOString();
    var startDay = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate(), 0, 0, 0, 0));
    this.from = startDay.toISOString();
  }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.mac = params.get("mac")
      this.temperatureHumidityService.getSensorDataEx(this.mac, this.from, this.to).subscribe((data) => {
        this.sensorDataEx = data;
      });
    });
  }


}
