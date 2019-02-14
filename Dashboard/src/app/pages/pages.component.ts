import { Component, OnInit } from '@angular/core';
import { NbMenuItem } from '@nebular/theme';
import { SensorsData, Sensor } from '../@core/data/sensors';

@Component({
  selector: 'ngx-pages',
  styleUrls: ['pages.component.scss'],
  template: `
    <ngx-sample-layout>
      <nb-menu [items]="menu"></nb-menu>
      <router-outlet></router-outlet>
    </ngx-sample-layout>
  `,
})

export class PagesComponent implements OnInit {
  private menuItem: any[] = [];
  private menuOutdoor: any[] = [];
  menu: NbMenuItem[] = [];

  constructor(private sensorsService: SensorsData ) {
  }
  
  ngOnInit() {
    
    let restCall = this.sensorsService.getSensorMasterData();
    restCall.subscribe((data) => {
          this.menuItem = data 
            .filter((sensor) => sensor.externalSensor == false)
            .map((item) => {
              return {
                title: item.senderName,
                link: '/pages/history/' + item.senderMAC,
              };
            });
          this.menuOutdoor = data
            .filter((sensor) => sensor.externalSensor == true)
            .map((item) => {
              return {
                title: item.senderName,
                link: '/pages/history/' + item.senderMAC,
              };
            });

          this.menu = [
            {
              title: 'IoT Dashboard',
              icon: 'nb-home',
              link: '/pages/iot-dashboard',
              home: true,
            },
            {
              title: 'DETAILS',
              group: true,
            },
            {
              title: 'Rooms',
              icon: 'nb-star',
              children: this.menuItem
            },
            {
              title: 'Outdoor',
              icon: 'nb-location',
              children: this.menuOutdoor
            },            
          ];
        });
  }
}



