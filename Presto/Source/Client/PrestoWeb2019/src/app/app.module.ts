import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppComponent } from './app.component';
import { ServersComponent } from './servers/servers.component';
import { ServerComponent } from './server/server.component';
import { ApplicationsComponent } from './applications/applications.component';

const routes: Routes = [
  { path: 'servers', component: ServersComponent },
  { path: 'server', component: ServerComponent },
  { path: 'applications', component: ApplicationsComponent }
]

@NgModule({
  declarations: [
    AppComponent,
    ServersComponent,
    ServerComponent,
    ApplicationsComponent
  ],
  imports: [
    BrowserModule,
    RouterModule.forRoot(routes),
    HttpClientModule,
    FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
