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
}
