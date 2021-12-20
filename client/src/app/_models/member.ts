import { Photo } from "./photo";

export interface Member {
  id: number;
  username: string;
  photoUrl: string;
  age: number;
  knowAs: any;
  created: Date;
  lastActive: Date;
  gender: string;
  introduction: string;
  lookinFor: string;
  interests: string;
  city: string;
  country: string;
  photos: Photo[];
}


