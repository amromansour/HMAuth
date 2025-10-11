export interface ApiResponse<T> {
    _ResponseCode: number;
    message: string;
    data: T | null;
    additionalData?: any;
}
