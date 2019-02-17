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
        var maxValue: number = 0;
        var minValue: number = 100;
        var tempArray = [];
        var tValveArray = [];
        var tSchedArray = [];
        var tCurrentArray = [];
        var pointRadius0=[];
        var pointRadius1=[];
        var pointRadius2=[];
        var pointRadius3=[];

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
              tCurrentArray.push({y: this.sensorDataEx[_i-1].tCurrentTarget, t: dateIso});
              
              pointRadius0.push(1);
              pointRadius1.push(1);
              pointRadius2.push(1);                  
              pointRadius3.push(1);
            }
          }
          tempArray.push({y: this.sensorDataEx[_i].temp, t: this.sensorDataEx[_i].ingestionTimestamp});
          tValveArray.push({y: this.sensorDataEx[_i].tValve, t: this.sensorDataEx[_i].ingestionTimestamp});
          tSchedArray.push({y: this.sensorDataEx[_i].tScheduledTarget, t: this.sensorDataEx[_i].ingestionTimestamp});
          tCurrentArray.push({y: this.sensorDataEx[_i-1].tCurrentTarget, t: this.sensorDataEx[_i].ingestionTimestamp});

          if (this.sensorDataEx[_i].setTempSended == true) {
            pointRadius0.push(4);
          } else {
            pointRadius0.push(1);
          }

          pointRadius1.push(1);
          pointRadius2.push(1);          
          pointRadius3.push(1);

          if (maxValue < this.sensorDataEx[_i].temp && this.sensorDataEx[_i].temp > 0)  {
            maxValue = this.sensorDataEx[_i].temp;
          }
          if (maxValue < this.sensorDataEx[_i].tValve && this.sensorDataEx[_i].tValve > 0) {
            maxValue = this.sensorDataEx[_i].tValve;
          }
          if (maxValue < this.sensorDataEx[_i].tScheduledTarget && this.sensorDataEx[_i].tScheduledTarget > 0) {
            maxValue = this.sensorDataEx[_i].tScheduledTarget;
          }
          if (maxValue < this.sensorDataEx[_i].tCurrentTarget && this.sensorDataEx[_i].tCurrentTarget > 0) {
            maxValue = this.sensorDataEx[_i].tCurrentTarget;
          }


          if (minValue > this.sensorDataEx[_i].temp && this.sensorDataEx[_i].temp > 0) {
            minValue = this.sensorDataEx[_i].temp;
          }
          if (minValue > this.sensorDataEx[_i].tValve && this.sensorDataEx[_i].tValve > 0) {
            minValue = this.sensorDataEx[_i].tValve;
          }
          if (minValue > this.sensorDataEx[_i].tScheduledTarget && this.sensorDataEx[_i].tScheduledTarget > 0) {
            minValue = this.sensorDataEx[_i].tScheduledTarget;
          }
          if (minValue > this.sensorDataEx[_i].tCurrentTarget && this.sensorDataEx[_i].tCurrentTarget > 0) {
            minValue = this.sensorDataEx[_i].tCurrentTarget;
          }          
        }

        this.data.datasets[0].data = tempArray;
        this.data.datasets[1].data = tValveArray;
        this.data.datasets[2].data = tSchedArray;
        this.data.datasets[3].data = tCurrentArray;
        
        this.data.datasets[0].pointRadius = pointRadius0;
        this.data.datasets[1].pointRadius = pointRadius1;
        this.data.datasets[2].pointRadius = pointRadius2;
        this.data.datasets[3].pointRadius = pointRadius3;


        this.options = {
          responsive: true,
          maintainAspectRatio: false,
          scales: {
            xAxes: [
              {
                type: 'time',
                time: {
                  unit: 'minute',
                  tooltipFormat: 'DD/MM - HH:mm',
                  displayFormats: {
                    minute: 'DD/MM - HH:mm'
                  }
                },
                gridLines: {
                  display: true,
                },
                ticks: {
                  autoSkip: true,
                  // maxTicksLimit: 30
                },                
              },
            ],
            yAxes: [
              {
                gridLines: {
                  display: true,
                },
                ticks: {
                  beginAtZero: true,
                  stepSize: 0.5,
                  min: minValue - 0.5,
                  max: maxValue + 0.5,
                },
              },
            ],
          },
        };
       
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
          hidden: true,
        }, {
          data: [],
          label: 'T Scheduled',
          backgroundColor: NbColorHelper.hexToRgbA(colors.info, 0.3),
          borderDash: [10,5],
          fill: false,
          borderColor: colors.info,
          lineTension: 0,        
        },
        {
          data: [],
          label: 'T CurrentSched',
          backgroundColor: NbColorHelper.hexToRgbA(colors.success, 0.3),
          borderDash: [10,5],
          fill: false,
          borderColor: colors.info,
          lineTension: 0,        
          hidden: true,
        },        
        ],
      };

      this.options = {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          xAxes: [
            {
              type: 'time',
              time: {
                unit: 'minute',
                tooltipFormat: 'DD/MM - HH:mm',
                displayFormats: {
                  minute: 'DD/MM - HH:mm'
                }
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
                min: 16,
                max: 23,
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
