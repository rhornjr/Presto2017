import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpService } from '../http.service';

@Component({
  selector: 'app-server',
  templateUrl: './server.component.html',
  styleUrls: ['./server.component.css']
})
export class ServerComponent implements OnInit {

  constructor(private route: ActivatedRoute, private _http: HttpService) { }

  serverId: string;
  server: Object;

  ngOnInit(): void {
    this.serverId = this.route.snapshot.queryParamMap.get("id")
    this._http.getServer(this.serverId).subscribe(data => {
      this.server = data;
    });
  }

  saveServer(server) {
    this._http.saveServer(server);
  }
}
