export interface LoginResponse {

    userName: string;
    accessToken: string;
    refreshToken: string;
    expiresInMin: number;
}