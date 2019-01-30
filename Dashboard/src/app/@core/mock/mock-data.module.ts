import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { UserService } from './users.service';
import { ElectricityService } from './electricity.service';
import { SmartTableService } from './smart-table.service';
import { ProfitBarAnimationChartService } from './profit-bar-animation-chart.service';
import { TemperatureHumidityService } from './temperature-humidity.service';
import { SolarService } from './solar.service';
import { StatsBarService } from './stats-bar.service';
import { StatsProgressBarService } from './stats-progress-bar.service';

const SERVICES = [
  UserService,
  ElectricityService,
  SmartTableService,
  ProfitBarAnimationChartService,
  TemperatureHumidityService,
  SolarService,
  StatsBarService,
  StatsProgressBarService,
];

@NgModule({
  imports: [
    CommonModule,
  ],
  providers: [
    ...SERVICES,
  ],
})
export class MockDataModule {
  static forRoot(): ModuleWithProviders {
    return <ModuleWithProviders>{
      ngModule: MockDataModule,
      providers: [
        ...SERVICES,
      ],
    };
  }
}
