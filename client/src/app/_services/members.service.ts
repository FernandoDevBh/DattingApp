import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { BaseService } from './base.services';

@Injectable({
  providedIn: 'root'
})
export class MembersService extends BaseService {
  members: Member[] = [];
  memberCache = new Map();
  user: User;
  userParams: UserParams;

  constructor(private http: HttpClient, private accountService: AccountService)
  {
    super();
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    });
  }

  getUserParams(){
    return this.userParams;
  }

  setUserParams(params: UserParams){
    this.userParams = params;
  }

  resetUserParams(){
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }

  getMembers(userParams: UserParams) {
    var response = this.memberCache.get(Object.values(userParams).join('_'));
    if(response){
      return of(response);
    }

    console.log(Object.values(userParams).join('_'));
    let params = this.getPaginationHeaders(userParams.pageNumber, userParams.pageSize);

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    return this.getPaginatedResult<Member[]>(this.createUrl('users'),params)
    .pipe(
      map(response => {
        this.memberCache.set(Object.values(userParams).join('_'), response);
        return response;
      })
    );
  }  

  getMember(userName: string) {
    const member = [...this.memberCache.values()]
                   .reduce((arr, elem: PaginatedResult<Member[]>) => arr.concat(elem.result), [])
                   .find((member: Member) => member.username === userName);

    if(member){
      return of(member);
    }    

    return this.http.get<Member>(this.createUrl(`users/${userName}`));
  }

  updateMember(member: Member) {
    return this.http.put(this.createUrl('users'), member)
      .pipe(
        map(() => {
          const index = this.members.indexOf(member);
          this.members[index] = member;
        })
      );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.createUrl(`users/set-main-photo/${photoId}`), {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.createUrl(`users/delete-photo/${photoId}`));
  }

  addLike(username: string){
    return this.http.post(this.createUrl(`likes/${username}`),{});
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number){
    let params = this.getPaginationHeaders(pageNumber, pageSize);
    params = params.append('predicate', predicate);

    return this.getPaginatedResult<Partial<Member[]>>(this.createUrl('likes'), params);
  }

  private getPaginatedResult<T>(url: string, params: HttpParams) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
    return this.http.get<T>(url, { observe: 'response', params })
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') !== null) {
            paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
          }
          return paginatedResult;
        })
      );
  }

  private getPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());

    return params;
  }
}
