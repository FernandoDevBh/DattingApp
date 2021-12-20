import { environment } from "src/environments/environment";

export abstract class BaseService {
    readonly baseUrl: string = environment.apiUrl;

    createUrl(endpoint: string): string {
        return `${this.baseUrl}${endpoint}`;
    }
}