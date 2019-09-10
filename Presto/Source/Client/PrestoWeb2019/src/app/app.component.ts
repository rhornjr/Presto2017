import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  ngOnInit(): void {
    // When we click a navbar item, collapse the navbar:
    const navbarItems = document.querySelectorAll('.navbar-nav>li');
    navbarItems.forEach(navbarItem => {
      navbarItem.addEventListener('click', () => {
        const navbar = document.querySelector('.navbar-collapse');
        navbar.classList.remove('show');
      })
    });
  }
  title = 'PrestoWeb2019';
}
