export interface Product {
  id: number;
  name: string;
  description: string;
  category: string;
  price: number;
  stockQuantity: number;
  imageUrl?: string;
}

export interface ProductCreate {
  name: string;
  description: string;
  category: string;
  price: number;
  stockQuantity: number;
  archivoImagen?: File;
}

export type TransactionType = 'Compra' | 'Venta';

export interface Transaction {
  id: number;
  date: string;
  type: TransactionType;
  productId: number;
  productName?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  detail: string;
}

export interface TransactionCreate {
  productId: number;
  type: number; // 0 for Compra, 1 for Venta
  quantity: number;
  unitPrice: number;
  detail: string;
}
