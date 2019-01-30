import { NgModule } from '@angular/core';
import { ThemeModule } from '../../@theme/theme.module';
import { HistoryComponent } from './history.component';

@NgModule({
  imports: [
    ThemeModule,
  ],
  declarations: [
    HistoryComponent,
  ],
})
export class HistoryModule { }
