import { Component, OnDestroy, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { NbThemeService, NbColorHelper } from '@nebular/theme';
import { MinMaxData4Day } from '../../../@core/data/temperature-humidity';

@Component({
  selector: 'ngx-historyanalysis',
  styleUrls: ['./historyanalysis.component.scss'],
  templateUrl: './historyanalysis.component.html',
})
export class HistoryAnalysisComponent implements OnDestroy, OnChanges {
  @Input() minMaxData4Day: MinMaxData4Day[] = [];
  @ViewChild('chartGraph') chartGraph;
  data: any;
  options: any;
  themeSubscription: any;

  ngOnDestroy(): void {
    this.themeSubscription.unsubscribe();
  }

  ngOnChanges(changes: SimpleChanges) {

    for (let propName in changes) {
      let change = changes[propName];
      if (change.currentValue == null) {
        continue;
      }

      if (propName == "minMaxData4Day") {
        var maxValue: number = 0;
        var minValue: number = 100;
        var tMaxArray = [];
        var tMaxTime = [];
        var tMinArray = [];
        var tMinTime = [];

        var pointRadius0 = [];
        var pointRadius1 = [];

        for (var _i = 0; _i < this.minMaxData4Day.length; _i++) {

          tMaxArray.push({ y: this.minMaxData4Day[_i].maxTemp, t: this.minMaxData4Day[_i].day });
          let tMaxTimeDate = new Date(this.minMaxData4Day[_i].maxTime);
          tMaxTime.push(tMaxTimeDate.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'}));
          tMinArray.push({ y: this.minMaxData4Day[_i].minTemp, t: this.minMaxData4Day[_i].day });
          let tMinTimeDate = new Date(this.minMaxData4Day[_i].minTime);
          tMinTime.push(tMinTimeDate.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'}));
          


          pointRadius0.push(5);
          pointRadius1.push(5);

          this.data.datasets[0].data = tMaxArray;
          this.data.datasets[0].labels = tMaxTime;
          this.data.datasets[1].data = tMinArray;
          this.data.datasets[1].labels = tMinTime;

          this.data.datasets[0].pointRadius = pointRadius0;
          this.data.datasets[1].pointRadius = pointRadius1;
        }

        minValue = Math.min.apply(Math, this.minMaxData4Day.map(function(o) { return o.minTemp; }))
        maxValue = Math.max.apply(Math, this.minMaxData4Day.map(function(o) { return o.maxTemp; }))


        this.options = {
          responsive: true,
          maintainAspectRatio: true,
          tooltips: {
            callbacks: {
                label: function(tooltipItem, data) {
                    var label = 'T:' + tooltipItem.yLabel + ' at:' + data.datasets[tooltipItem.datasetIndex].labels[tooltipItem.index];
                    return label;
                }
            }
          },          
          scales: {
            xAxes: [
              {
                type: 'time',
                time: {
                  unit: 'day',
                  tooltipFormat: 'DD/MM',
                  displayFormats: {
                    minute: 'DD/MM'
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
                id: 'yTemperature',
                gridLines: {
                  display: true,
                },
                ticks: {
                  beginAtZero: true,
                  // stepSize: 0.5,
                  min: minValue - 1,
                  max: maxValue + 1,
                },
              }]
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
          label: 'Tmax',
          backgroundColor: NbColorHelper.hexToRgbA(colors.danger, 0.3),
          borderColor: colors.danger,
          lineTension: 0,
          fill: false,
        }, {
          data: [],
          label: 'Tmin',
          backgroundColor: NbColorHelper.hexToRgbA(colors.primary, 0.3),
          borderColor: colors.primary,
          lineTension: 0,
          fill: false,
          hidden: false,
        }],
      };

      this.options = {
        responsive: true,
        maintainAspectRatio: true,
        scales: {
          xAxes: [
            {
              type: 'time',
              time: {
                unit: 'day',
                tooltipFormat: 'DD/MM',
                displayFormats: {
                  minute: 'DD/MM'
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
                min: 0,
                max: 100,
              },
            }],
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
