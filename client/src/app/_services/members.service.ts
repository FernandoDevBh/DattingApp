import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { Member } from '../_models/member';
import { BaseService } from './base.services';

@Injectable({
  providedIn: 'root'
})
export class MembersService extends BaseService {
  members: Member[] = [];

  constructor(private http: HttpClient) { super(); }

  getMembers() {
    if(this.members.length > 0) return of(this.members);

    return this.http.get<Member[]>(this.createUrl('users'))
                    .pipe(
                      map(members => {
                        this.members = members;
                        return members;
                      })
                    );
  }

  getMember(userName: string) {
    const member = this.members.find(m => m.username === userName);
    if(member !== undefined) return of(member)

    return this.http.get<Member>(this.createUrl(`users/${userName}`));
  }

  updateMember(member: Member){
    return this.http.put(this.createUrl('users'), member)
                    .pipe(
                      map(() => {
                        const index = this.members.indexOf(member);
                        this.members[index] = member;
                      })
                    );
  }

  setMainPhoto(photoId: number){
    return this.http.put(this.createUrl(`users/set-main-photo/${photoId}`), {});
  }

  deletePhoto(photoId: number){
    return this.http.delete(this.createUrl(`users/delete-photo/${photoId}`));
  }
}
