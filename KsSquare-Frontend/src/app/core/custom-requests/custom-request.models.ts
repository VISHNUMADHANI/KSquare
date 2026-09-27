export interface CustomRequestOption { id: string; type: string; name: string; imageUrl: string | null; }
export interface CustomRequestMedia { id: string; url: string; contentType: string; contentLength: number; displayOrder: number; }
export interface CustomJewelryRequest { id: string; requestNumber: string; categoryName: string; customerName: string; email: string; phone: string; description: string; status: string; fixedPrice: number | null; currency: 'USD'; options: CustomRequestOption[]; media: CustomRequestMedia[]; createdAt: string; updatedAt: string | null; }
export interface CustomRequestAccess { id: string; token: string; requestNumber: string; }
export interface CreateCustomRequestResult { request: CustomJewelryRequest; accessToken: string; }
