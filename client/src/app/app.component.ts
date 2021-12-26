import { OnInit } from '@angular/core';
import { Component } from '@angular/core';

import { User } from './_models/user';
import { AccountService } from './_services/account.service';

import { TimeagoIntl } from 'ngx-timeago';
import { strings as portugueseStrings } from 'ngx-timeago/language-strings/pt-br';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dating App';
  users: any;

  constructor(private accountService: AccountService, intl: TimeagoIntl) {
    intl.strings = portugueseStrings;
    intl.changes.next();
  }

  ngOnInit() {
    this.setCurrentUser();
  }

  setCurrentUser(){
    const user:User = JSON.parse(localStorage.getItem('user'));
    this.accountService.setCurrentUser(user);
  }
}
