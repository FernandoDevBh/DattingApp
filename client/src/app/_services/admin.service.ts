import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Photo } from '../_models/photo';
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

  getPhotosForApproval() {
    return this.http.get<Photo[]>(this.createUrl('admin/photos-to-moderate'));
  }

  approvePhoto(photoId: number) {
    return this.http.post(this.createUrl(`admin/approve-photo/${photoId}`), {});
  }

  rejectPhoto(photoId: number) {
    return this.http.post(this.createUrl(`admin/reject-photo/${photoId}`), {});
  }
}
