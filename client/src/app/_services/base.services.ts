import { HttpClient, HttpParams } from "@angular/common/http";
import { map } from "rxjs/operators";
import { environment } from "src/environments/environment";
import { PaginatedResult } from "../_models/pagination";

export abstract class BaseService {
    readonly baseUrl: string = environment.apiUrl;

    constructor(public http: HttpClient) { }

    createUrl(endpoint: string): string {
        return `${this.baseUrl}${endpoint}`;
    }

    public getPaginatedResult<T>(url: string, params: HttpParams) {
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

    public getPaginationHeaders(pageNumber: number, pageSize: number) {
        let params = new HttpParams();

        params = params.append('pageNumber', pageNumber.toString());
        params = params.append('pageSize', pageSize.toString());

        return params;
    }
}