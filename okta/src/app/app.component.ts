import { Component, OnInit } from '@angular/core';
import { OktaAuthService } from '@okta/okta-angular';
import { ValuesApi } from './shared/backEndApi/oktaBackend';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  title: any;
  isAuthenticated: boolean;

  constructor(public oktaAuth: OktaAuthService, public valuesApi: ValuesApi) {}

  async ngOnInit() {
    this.isAuthenticated = await this.oktaAuth.isAuthenticated();
    this.oktaAuth.$authenticationState.subscribe(
      (isAuthenticated: boolean) => (this.isAuthenticated = isAuthenticated)
    );
  }

  getData() {
    this.valuesApi.getAll().subscribe((response) => (this.title = response));
  }
}
