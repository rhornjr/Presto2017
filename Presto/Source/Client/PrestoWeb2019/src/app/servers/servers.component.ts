import { Component, OnInit } from '@angular/core';
import { HttpService } from '../http.service';

@Component({
  selector: 'app-servers',
  templateUrl: './servers.component.html',
  styleUrls: ['./servers.component.css']
})
export class ServersComponent implements OnInit {
  servers: Object;

  constructor(private _http: HttpService) { }

  ngOnInit() {
    this._http.getServers().subscribe(data => {
      this.servers = data;
    });
  }
}
