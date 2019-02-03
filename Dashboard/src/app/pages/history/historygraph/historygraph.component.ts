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
        this.data.labels = this.sensorDataEx.map(item => item.ingestionTimestamp);
        this.data.datasets[0].data = this.sensorDataEx.map(item => item.temp);
        this.data.datasets[1].data = this.sensorDataEx.map(item => item.tValve);
        this.data.datasets[2].data = this.sensorDataEx.map(item => item.tScheduledTarget);
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
          backgroundColor: NbColorHelper.hexToRgbA(colors.primary, 0.3),
          borderColor: colors.primary,
        }, {
          data: [],
          label: 'T Valve',
          backgroundColor: NbColorHelper.hexToRgbA(colors.danger, 0.3),
          borderColor: colors.danger,
        }, {
          data: [],
          label: 'T Scheduled',
          backgroundColor: NbColorHelper.hexToRgbA(colors.info, 0.3),
          borderColor: colors.info,
        },
        ],
      };

      this.options = {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          xAxes: [
            {
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
