export interface Category {
  id: string; name: string; slug: string; parentId: string | null; parentName: string | null;
  isActive: boolean; showInCustom: boolean; imageUrl: string | null; productCount: number; childCount: number;
}
export interface SaveCategory { name: string; parentId: string | null; isActive: boolean; showInCustom: boolean; }
export type ProductOptionType = 'CustomPendantStyle' | 'PendantSize' | 'ChainSize' | 'ChainWidth' | 'ChainDiamondSize' | 'RingSize' | 'Color' | 'BraceletSize' | 'BraceletStoneSize';
export interface ProductOption { sizeGuideUrl?: string | null; id: string; type: ProductOptionType; name: string; imageUrl: string | null; displayOrder: number; isActive: boolean; productCount: number; }
export interface SaveProductOption { type: ProductOptionType; name: string; displayOrder: number; isActive: boolean; }
export interface ProductOptionSummary { id: string; type: ProductOptionType; name: string; imageUrl: string | null; displayOrder: number; }
export interface ProductSubcategory { id: string; name: string; slug: string; }
export interface ProductMedia { id: string; objectKey: string; url: string; contentType: string; contentLength: number; altText: string; displayOrder: number; }
export interface ProductChainVariant { id: string; chainSizeOptionId: string; chainWidthOptionId: string | null; chainDiamondSizeOptionId: string | null; originalPrice: number; finalPrice: number; isAvailable: boolean; }
export interface SaveProductChainVariant { chainSizeOptionId: string; chainWidthOptionId: string | null; chainDiamondSizeOptionId: string | null; originalPrice: number; isAvailable: boolean; }
export interface ProductBraceletVariant { id: string; braceletSizeOptionId: string | null; braceletStoneSizeOptionId: string | null; originalPrice: number; finalPrice: number; isAvailable: boolean; }
export interface SaveProductBraceletVariant { braceletSizeOptionId: string | null; braceletStoneSizeOptionId: string | null; originalPrice: number; isAvailable: boolean; }
export interface SaveProductPendantVariant { pendantSizeOptionId:string; originalPrice:number; isAvailable:boolean; }
export interface ProductPendantVariant extends SaveProductPendantVariant { id:string; finalPrice:number; }
export interface Product {
  id: string; name: string; sku: string; description: string; originalPrice: number;
  discountPercentage: number; finalPrice: number; currency: 'USD'; categoryId: string;
  categoryName: string; subcategories: ProductSubcategory[]; isAvailable: boolean; isActive: boolean; supportsNamePersonalization: boolean; isFinalSale?: boolean; includedNameLetters?: number; nameFixedPrice: number; namePricePerLetter: number; showInCustom: boolean;
  media: ProductMedia[]; options: ProductOptionSummary[]; chainVariants: ProductChainVariant[]; braceletVariants: ProductBraceletVariant[]; pendantVariants?: ProductPendantVariant[]; createdAt: string; updatedAt: string | null;
}
export interface SaveProduct {
  name: string; sku: string | null; description: string; originalPrice: number;
  discountPercentage: number; categoryId: string; isAvailable: boolean; isActive: boolean; supportsNamePersonalization: boolean; isFinalSale?: boolean; includedNameLetters?: number; nameFixedPrice: number; namePricePerLetter: number; showInCustom: boolean; subcategoryIds: string[]; optionIds: string[]; chainVariants: SaveProductChainVariant[]; braceletVariants: SaveProductBraceletVariant[]; pendantVariants?: SaveProductPendantVariant[];
}
