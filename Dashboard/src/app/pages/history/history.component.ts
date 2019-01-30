import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from "@angular/router";
import { SensorsData, Sensor } from '../../@core/data/sensors';


@Component({
  selector: 'ngx-history',
  styleUrls: ['./history.component.scss'],  
  templateUrl:  './history.component.html',
})
export class HistoryComponent  implements OnInit {
  constructor(private route: ActivatedRoute) { }

  param: string;

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.param = params.get("mac")
    });
  }

  sensorMasterData: Sensor;
}
