import { Component, OnDestroy, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { NbThemeService, NbColorHelper } from '@nebular/theme';
import { SensorDataEx } from '../../../@core/data/temperature-humidity';

@Component({
  selector: 'ngx-historygraph',
  styleUrls: ['./historygraph.component.scss'],
  templateUrl: './historygraph.component.html',
})
export class HistoryGraphComponent implements OnDestroy, OnChanges {
  @Input() sensorDataEx: SensorDataEx[] = [];
  @ViewChild('chartGraph') chartGraph; 
  data: any;
  options: any;
  themeSubscription: any;
  
  ngOnDestroy(): void {
    this.themeSubscription.unsubscribe();
  }

  ngOnChanges (changes: SimpleChanges) {

    for (let propName in changes) {  
      let change = changes[propName];
      if (change.currentValue == null) {
        continue;
      }

      if (propName == "sensorDataEx")
      {
        var tempArray = [];
        var tValveArray = [];
        var tSchedArray = [];
        for (var _i = 0; _i < this.sensorDataEx.length; _i++)
        {
          if (_i > 0)
          {
            var dateNewValue = new Date(this.sensorDataEx[_i].ingestionTimestamp);
            var dateOldValue = new Date(this.sensorDataEx[_i-1].ingestionTimestamp);
            var diff = Math.abs(dateNewValue.getTime() - dateOldValue.getTime());
            var minutes = Math.floor(diff/60000);
            if (minutes > 15) //greater than deepsleep (10 minutes).
            {
              var dateIntermediate = new Date(dateNewValue.getTime() - (10*60*1000)); // 10 minutes before
              var dateIso = dateIntermediate.toISOString();
              tempArray.push({y: this.sensorDataEx[_i-1].temp, t: dateIso});
              tValveArray.push({y: this.sensorDataEx[_i-1].tValve, t: dateIso});
              tSchedArray.push({y: this.sensorDataEx[_i-1].tScheduledTarget, t: dateIso});
            }
          }
          tempArray.push({y: this.sensorDataEx[_i].temp, t: this.sensorDataEx[_i].ingestionTimestamp});
          tValveArray.push({y: this.sensorDataEx[_i].tValve, t: this.sensorDataEx[_i].ingestionTimestamp});
          tSchedArray.push({y: this.sensorDataEx[_i].tScheduledTarget, t: this.sensorDataEx[_i].ingestionTimestamp});
        }
        this.data.datasets[0].data = tempArray;
        this.data.datasets[1].data = tValveArray;
        this.data.datasets[2].data = tSchedArray;

        this.chartGraph.chart.update();
      }
    }
  }

  constructor(private theme: NbThemeService) {
    this.themeSubscription = this.theme.getJsTheme().subscribe(config => {

      const colors: any = config.variables;
      const chartjs: any = config.variables.chartjs;

      this.data = {
        labels: [],
        datasets: [{
          data: [],
          label: 'Temp',
          backgroundColor: NbColorHelper.hexToRgbA(colors.danger, 0.3),
          borderColor: colors.danger,
          lineTension: 0,        
          fill: false,
        }, {
          data: [],
          label: 'T Valve',
          backgroundColor: NbColorHelper.hexToRgbA(colors.primary, 0.3),
          borderColor: colors.primary,
          lineTension: 0,        
          fill: false,
        }, {
          data: [],
          label: 'T Scheduled',
          backgroundColor: NbColorHelper.hexToRgbA(colors.info, 0.3),
          borderDash: [10,5],
          fill: false,
          borderColor: colors.info,
          lineTension: 0,        
        },
        ],
      };

      this.options = {
        responsive: true,
        maintainAspectRatio: true,
        scales: {
          xAxes: [
            {
              type: 'time',
              time: {
                unit: 'minute',
                tooltipFormat: 'h:mm a'
              },
              gridLines: {
                display: true,
                color: chartjs.axisLineColor,
              },
              ticks: {
                fontColor: chartjs.textColor,
              },
            },
          ],
          yAxes: [
            {
              gridLines: {
                display: true,
                color: chartjs.axisLineColor,
              },
              ticks: {
                fontColor: chartjs.textColor,
                beginAtZero: true,
                stepSize: 0.5,
                min: 15,
                max: 22,
              },
            },
          ],
        },
        legend: {
          labels: {
            fontColor: chartjs.textColor,
          },
        },
      };
    });
  }
}
