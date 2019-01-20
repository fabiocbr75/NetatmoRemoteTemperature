import {Component, OnDestroy} from '@angular/core';
import { NbThemeService } from '@nebular/theme';
import { takeWhile } from 'rxjs/operators' ;
import { SolarData } from '../../@core/data/solar';

@Component({
  selector: 'ngx-dashboard',
  styleUrls: ['./dashboard.component.scss'],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnDestroy {

  private alive = true;

  SensorMasterData = [{"senderMAC":"80:7D:3A:47:5C:C5","senderName":"Camera","roomId":"3716460054","enabled":true},{"senderMAC":"80:7D:3A:47:59:86","senderName":"Cameretta","roomId":"3702889680","enabled":true},{"senderMAC":"84:F3:EB:0D:BC:23","senderName":"Bagno","roomId":"3575883469","enabled":true},{"senderMAC":"80:7D:3A:47:5C:B2","senderName":"Studio","roomId":"1541168514","enabled":false},{"senderMAC":"80:7D:3A:47:5B:62","senderName":"Cucina","roomId":"2809735084","enabled":true},{"senderMAC":"80:7D:3A:57:F2:50","senderName":"Sala","roomId":"2935863693","enabled":true}];

  solarValue: number;
  
  constructor(private themeService: NbThemeService,
              private solarService: SolarData) {
    
    // this.themeService.getJsTheme()
    //   .pipe(takeWhile(() => this.alive))
    //   .subscribe(theme => {
    //     // this.statusCards = this.statusCardsByThemes[theme.name];
    // });

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
