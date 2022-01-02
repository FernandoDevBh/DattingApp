import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { BaseService } from './base.services';

@Injectable({
  providedIn: 'root'
})
export class AdminService extends BaseService{

  constructor(http: HttpClient) {
    super(http);
  }

  getUsersWithRoles(){
    return this.http.get<Partial<User[]>>(this.createUrl(`admin/users-with-roles`));
  }

  updateUserRoles(username: string, roles: string[]){
    return this.http.post(this.createUrl(`admin/edit-roles/${username}?roles=${roles}`), {}); 
  }
}
