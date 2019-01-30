import { NbMenuService } from '@nebular/theme';
import { Component } from '@angular/core';

@Component({
  selector: 'ngx-historygraph',
  styleUrls: ['./historygraph.component.scss'],
  templateUrl: './historygraph.component.html',
})
export class HistoryGraphComponent {

  constructor(private menuService: NbMenuService) {
  }

  goToHome() {
    this.menuService.navigateHome();
  }
}
