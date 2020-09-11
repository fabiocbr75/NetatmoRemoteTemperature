import { NgModule } from '@angular/core';
import { ThemeModule } from '../../@theme/theme.module';
import { HistoryComponent } from './history.component';
import { HistoryGraphComponent } from './historygraph/historygraph.component';
import { HistoryAnalysisComponent } from './historyanalysis/historyanalysis.component';
import { ChartModule } from 'angular2-chartjs';
import { NbCheckboxModule } from '@nebular/theme';

@NgModule({
  imports: [
    ThemeModule, ChartModule, NbCheckboxModule
  ],
  declarations: [
    HistoryComponent,
    HistoryGraphComponent,
    HistoryAnalysisComponent
  ],
})
export class HistoryModule { }
