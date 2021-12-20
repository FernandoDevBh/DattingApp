import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Member } from '../_models/member';
import { BaseService } from './base.services';

@Injectable({
  providedIn: 'root'
})
export class MembersService extends BaseService {
  constructor(private http: HttpClient) { super(); }

  getMembers() {
    return this.http.get<Member[]>(this.createUrl('users'))
  }

  getMember(userName: string) {
    return this.http.get<Member>(this.createUrl(`users/${userName}`));
  }
}
