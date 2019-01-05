import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'TemperatureHubUI';
  from: Date;
  to: Date;
  rangeDates: Date[];
  values: number[] = [102, 115, 130, 137];
  constructor() {
  }
}
