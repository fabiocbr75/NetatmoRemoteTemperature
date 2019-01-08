import { Component, ViewChild, AfterViewInit } from '@angular/core';
import { jqxChartComponent } from 'jqwidgets-scripts/jqwidgets-ts/angular_jqxchart';
import { jqxDateTimeInputComponent } from 'jqwidgets-scripts/jqwidgets-ts/angular_jqxdatetimeinput';
import { RestService } from './rest.service';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent implements AfterViewInit {
    @ViewChild('myChart') myChart: jqxChartComponent;
    @ViewChild('myDateTimeInput') myDateTimeInput; jqxDateTimeInputComponent;
    // title = 'TemperatureHubUI';
    // from: Date;
    // to: Date;
    // rangeDates: Date[];

    constructor(public rest: RestService) {

    }

    ngAfterViewInit(): void {
        let date1 = new Date();
        date1.setFullYear(2013, 7, 7);
        let date2 = new Date();
        date2.setFullYear(2013, 7, 15);

        setTimeout(_ => this.myDateTimeInput.setRange(date1, date2))
        let sensorData = this.rest.getSensorData("", date1, date2);
        this.dateOnChange();
    }

    dateOnChange(): void {
        let selection = this.myDateTimeInput.getRange();
    }    

    data: any[] = [
        { Temperature: 10, VTemp: 20 },
        { Temperature: 12, VTemp: 14 },
        { Temperature: 10, VTemp: 11 },
        { Temperature: 19, VTemp: 13 },
        { Temperature: 17, VTemp: 21 },
        { Temperature: 17, VTemp: 21 },
        { Temperature: 18, VTemp: 25 },
        { Temperature: 21, VTemp: 21 },
        { Temperature: 23, VTemp: 10 }
    ];

    padding: any = { left: 10, top: 5, right: 10, bottom: 5 };
    titlePadding: any = { left: 50, top: 0, right: 0, bottom: 10 };

    getWidth(): any {
        if (document.body.offsetWidth < 850) {
            return '90%';
        }

        return 850;
    }

    xAxis: any =
        {
            unitInterval: 1,
            gridLines: { interval: 2 },
            valuesOnTicks: false
        };

    valueAxis: any =
        {
            minValue: 10,
            maxValue: 25,
            title: { text: 'Temperature' },
            labels: { horizontalAlignment: 'right' }
        };

    seriesGroups: any =
        [
            {
                type: 'line',
                series: [
                    { dataField: 'Temperature', displayText: 'Temperature' },
                    { dataField: 'VTemp', displayText: 'VTemp' }
                ]
            }
        ];

    seriesList: string[] = ['splinearea', 'spline', 'column', 'scatter', 'stackedcolumn', 'stackedsplinearea', 'stackedspline'];

    seriesOnChange(event: any): void {
        let args = event.args;
        if (args) {
            let value = args.item.value;
            let isLine = value.indexOf('line') != -1;
            let isArea = value.indexOf('area') != -1;
            let group = this.myChart.seriesGroups()[0];
            group.series[0].opacity = group.series[1].opacity = isArea ? 0.7 : 1;
            group.series[0].lineWidth = group.series[1].lineWidth = isLine ? 2 : 1;
            group.type = value;
            this.myChart.update();
        }
    }
}
