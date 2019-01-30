import { NgModule } from '@angular/core';

import { NgxEchartsModule } from 'ngx-echarts';

import { ThemeModule } from '../../@theme/theme.module';
import { DashboardComponent } from './dashboard.component';
import { ContactsComponent } from './contacts/contacts.component';
import { TemperatureComponent } from './temperature/temperature.component';
import { TemperatureDraggerComponent } from './temperature/temperature-dragger/temperature-dragger.component';
import { ElectricityComponent } from './electricity/electricity.component';
import { ElectricityChartComponent } from './electricity/electricity-chart/electricity-chart.component';
import { WeatherComponent } from './weather/weather.component';
import { SolarComponent } from './solar/solar.component';

@NgModule({
  imports: [
    ThemeModule,
    NgxEchartsModule,
  ],
  declarations: [
    DashboardComponent,
    TemperatureDraggerComponent,
    ContactsComponent,
    TemperatureComponent,
    ElectricityComponent,
    ElectricityChartComponent,
    WeatherComponent,
    SolarComponent,
  ],
})
export class DashboardModule { }
