import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { HistoryComponent } from './history.component';
import { HistoryGraphComponent } from './historygraph/historygraph.component';

const routes: Routes = [{
  path: '',
  component: HistoryComponent,
  children: [{
    path: '404',
    component: HistoryGraphComponent,
  }],
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class MiscellaneousRoutingModule { }

export const routedComponents = [
  HistoryComponent,
  HistoryGraphComponent,
];
