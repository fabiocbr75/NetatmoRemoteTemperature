import { Component } from '@angular/core';

@Component({
  selector: 'ngx-footer',
  styleUrls: ['./footer.component.scss'],
  template: `
    <span class="created-by">Created with by <b><a href="https://github.com/fabiocbr75/NetatmoRemoteTemperature" target="_blank">Fabio</a></b> 2019</span>
    <div class="socials">
      <a href="https://github.com/fabiocbr75/NetatmoRemoteTemperature" target="_blank" class="ion ion-social-github"></a>
    </div>
  `,
})
export class FooterComponent {
}
