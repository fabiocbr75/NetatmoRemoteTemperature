import { NgModule } from '@angular/core';
import { ThemeModule } from '../../@theme/theme.module';
import { HistoryComponent } from './history.component';
import { HistoryGraphComponent } from './historygraph/historygraph.component';
import { ChartModule } from 'angular2-chartjs';

@NgModule({
  imports: [
    ThemeModule, ChartModule
  ],
  declarations: [
    HistoryComponent,
    HistoryGraphComponent,
  ],
})
export class HistoryModule { }
