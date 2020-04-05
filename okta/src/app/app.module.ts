import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthRoutingModule } from './auth-routing.module';
import { ValuesApi } from './shared/backEndApi/oktaBackend';

@NgModule({
  declarations: [AppComponent],
  imports: [BrowserModule, AppRoutingModule, AuthRoutingModule],
  providers: [ValuesApi],
  bootstrap: [AppComponent],
})
export class AppModule {}
