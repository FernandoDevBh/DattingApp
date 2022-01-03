import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { Group } from '../_models/group';
import { Message } from '../_models/message';
import { User } from '../_models/user';
import { BaseService } from './base.services';

@Injectable({
  providedIn: 'root'
})
export class MessageService extends BaseService {
  private hubConnection: HubConnection; 
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(http: HttpClient) {
    super(http);
  }

  createHubConnection(user: User, otherUsername: string){
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.createHubUrl(`message?user=${otherUsername}`), {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
        .start()
        .catch(error => console.log(error));

    this.hubConnection.on("ReceiveMessageThread", messages => {
      this.messageThreadSource.next(messages);
    });

    this.hubConnection.on("NewMessage", message => {
      this.messageThread$.pipe(take(1)).subscribe(messages =>{
        this.messageThreadSource.next([...messages, message]);
      })
    });

    this.hubConnection.on("UpdatedGroup", (group: Group) => {
      if(group.connections.some(x => x.username === otherUsername)){
        this.messageThread$.pipe(take(1)).subscribe(messages => {
          messages.forEach(message => {
            if(!message.dateRead){
              message.dateRead = new Date(Date.now());
            }            
          })

          this.messageThreadSource.next([...messages]);
        })
      }
    });
  }

  stopHubConnection(){
    if(this.stopHubConnection){
      this.hubConnection.stop();
    }    
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = this.getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return this.getPaginatedResult<Message[]>(this.createUrl('messages'), params);
  }

  getMessagesThread(username: string) {
    return this.http.get<Message[]>(this.createUrl(`messages/thread/${username}`))
  }

  async sendMessage(recipientUsername: string, content: string) {
    return this.hubConnection
               .invoke("SendMessage", { recipientUsername, content })
               .catch(error => console.log(error));
  }

  deleteMessage(id: number){
    return this.http.delete(this.createUrl(`messages/${id}`));
  }
}
