import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Message } from '../_models/message';
import { BaseService } from './base.services';

@Injectable({
  providedIn: 'root'
})
export class MessageService extends BaseService {
  constructor(http: HttpClient) {
    super(http);
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = this.getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return this.getPaginatedResult<Message[]>(this.createUrl('messages'), params);
  }

  getMessagesThread(username: string) {
    return this.http.get<Message[]>(this.createUrl(`messages/thread/${username}`))
  }

  sendMessage(recipientUsername: string, content: string) {
    return this.http.post(this.createUrl(`messages`), { recipientUsername, content });
  }

  deleteMessage(id: number){
    return this.http.delete(this.createUrl(`messages/${id}`));
  }
}
