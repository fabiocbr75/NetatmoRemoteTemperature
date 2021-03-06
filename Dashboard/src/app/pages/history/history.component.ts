import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { ActivatedRoute } from "@angular/router";
import { SensorsData, Sensor } from '../../@core/data/sensors';
import { TemperatureHumidityData, SensorDataEx, MinMaxData4Day } from '../../@core/data/temperature-humidity';
import { NbRangepickerComponent } from '@nebular/theme/components/datepicker/datepicker.component';
import { NbCalendarRange, NbCheckboxComponent } from '@nebular/theme';



@Component({
  selector: 'ngx-history',
  styleUrls: ['./history.component.scss'],  
  templateUrl:  './history.component.html',
})
export class HistoryComponent  implements OnInit, OnDestroy {
  private mac: string;
  private from: string;
  private to: string;
  sensorDataEx: SensorDataEx[];
  minMaxData4Day: MinMaxData4Day[];
  
  @ViewChild('rangepicker') rangepicker: NbRangepickerComponent<any>;
  @ViewChild('minmax') minMaxAnalysis: NbCheckboxComponent;

  constructor(private route: ActivatedRoute, private temperatureHumidityService: TemperatureHumidityData) { 
    var now = new Date;
    this.to = now.toISOString();
    var startDay = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate(), 0, 0, 0, 0));
    this.from = startDay.toISOString();
  }
  ngOnDestroy() {

  }

    
  toggle(checked: boolean) {
    if (checked)
    {
      let tmpDate = new Date();
      let tmpLastWeek = new Date();
      tmpLastWeek.setDate(tmpDate.getDate() - 7);
      
      this.from = tmpLastWeek.toISOString();
  
      this.temperatureHumidityService.getMinMaxData4Day(this.mac, this.from, this.to).subscribe((data) => {
        this.minMaxData4Day = data;
      });
    }

  }
  
  ngOnInit() {
    
    this.rangepicker.rangeChange.subscribe((val: {start: Date, end: Date}) => {
      var dateFrom = (val.start) ? val.start : null;
      if (dateFrom != null) {
        var d = new Date(Date.UTC(dateFrom.getFullYear(), dateFrom.getMonth(), dateFrom.getDate(),0,0,0))
        this.from = d.toISOString();

        var dateTo = (val.end) ? val.end : null;
        var tmp: Date;
        if (dateTo != null) {
          tmp = new Date(Date.UTC(dateTo.getFullYear(), dateTo.getMonth(), dateTo.getDate(), 23, 59, 59, 999));
          this.to = tmp.toISOString();

          if (this.minMaxAnalysis.value)
          {
            this.temperatureHumidityService.getMinMaxData4Day(this.mac, this.from, this.to).subscribe((data) => {
              this.minMaxData4Day = data;
            });
          }
          else
          {
            this.temperatureHumidityService.getSensorDataEx(this.mac, this.from, this.to).subscribe((data) => {
              this.sensorDataEx = data;
            });
          }          
        }
      }
    });
    
    this.route.paramMap.subscribe(params => {
      this.mac = params.get("mac")

      this.temperatureHumidityService.getSensorDataEx(this.mac, this.from, this.to).subscribe((data) => {
        this.sensorDataEx = data;
      });

    });
  }

}
