import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators'
import { User } from '../_models/user';
import { BaseService } from './base.services';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService extends BaseService {  
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(http: HttpClient, private presence: PresenceService) { super(http); }

  login(model: any){
    return this.http.post(this.createUrl('account/login'), model).pipe(
      map((response: User) => {
        const user = response;
        if(user){
          this.setCurrentUser(user);
          this.presence.createHubConnection(user);
        }
      })
    );
  }

  register(model:any){
    return this.http.post(this.createUrl('account/register'), model).pipe(
      map((user: User) => {
        if(user){          
          this.setCurrentUser(user);
          this.presence.createHubConnection(user);
        }
        return user;
      })
    );
  }

  setCurrentUser(user: User){
    user.roles = [];
    const roles = this.getDecodedToken(user.token).role;
    Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  logout(){
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
    this.presence.stopHubConnection();
  }

  getDecodedToken(token: string){
    return JSON.parse(atob(token.split('.')[1]));
  }
}
