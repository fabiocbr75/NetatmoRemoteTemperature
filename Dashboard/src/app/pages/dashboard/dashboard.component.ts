import {Component, OnDestroy} from '@angular/core';
import { NbThemeService } from '@nebular/theme';
import { takeWhile } from 'rxjs/operators' ;
import { SolarData } from '../../@core/data/solar';
import { SensorsData, Sensor } from '../../@core/data/sensors';

@Component({
  selector: 'ngx-dashboard',
  styleUrls: ['./dashboard.component.scss'],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnDestroy {

  private alive = true;

  sensorMasterData: Sensor[];
  solarValue: number;
  
  constructor(private themeService: NbThemeService,
              private solarService: SolarData, 
              private sensorsService: SensorsData ) {
    
    // this.themeService.getJsTheme()
    //   .pipe(takeWhile(() => this.alive))
    //   .subscribe(theme => {
    //     // this.statusCards = this.statusCardsByThemes[theme.name];
    // });
    this.sensorsService.getSensorMasterData()
                .subscribe((data) => {
                  this.sensorMasterData = data;
                });

    this.solarService.getSolarData()
      .pipe(takeWhile(() => this.alive))
      .subscribe((data) => {
        this.solarValue = data;
      });
  }

  ngOnDestroy() {
    this.alive = false;
  }
}
