import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { User } from '../_models/user';
import { BaseService } from './base.services';

@Injectable({
  providedIn: 'root'
})
export class PresenceService extends BaseService{
  private hubConnection: HubConnection;
  private onLineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onLineUsersSource.asObservable();

  constructor(http: HttpClient, private toastr: ToastrService, private router: Router) {
    super(http);
  }

  createHubConnection(user: User){
    this.hubConnection = new HubConnectionBuilder()
        .withUrl(this.createHubUrl('presence'), {
          accessTokenFactory: () => user.token
        })
        .withAutomaticReconnect()
        .build();
    
    this.hubConnection
        .start()
        .catch(error => console.log(error));

    this.hubConnection.on('UserIsOnline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe(usernames => {
        this.onLineUsersSource.next([...usernames, username]);
      });
    });

    this.hubConnection.on('UserIsOffline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe(usernames => {
        this.onLineUsersSource.next([...usernames.filter(u => u !== username)]);
      });
    });

    this.hubConnection.on('GetOnlineUsers', (usernames:string[]) =>{
      this.onLineUsersSource.next(usernames);
    });

    this.hubConnection.on("NewMessageReceived", ({username, knowAs}) => {
      this.toastr
          .info(`${knowAs} has sent you a new message!`)
          .onTap
          .pipe(take(1))
          .subscribe(() => this.router.navigateByUrl(`/members/${username}?tab=3`))

    });
  }

  stopHubConnection(){
    this.hubConnection
        .stop()
        .catch(error => console.log(error));
  }
}
