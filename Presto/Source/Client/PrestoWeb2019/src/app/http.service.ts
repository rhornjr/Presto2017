import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class HttpService {
  constructor(private http: HttpClient) { }

  getServers() {
    return this.http.get('http://localhost/PrestoWeb/api/servers');
  }

  getServer(id: string): any {
    const uri = 'http://localhost/PrestoWeb/api/server/?id=' + encodeURI(id);
    return this.http.get(uri);
  }

  saveServer(server: object): void {
    const uri = 'http://localhost/PrestoWeb/api/server/save';
    this.http.post(uri, server).subscribe({
      next: result => console.log(result)
    });
  }
}
