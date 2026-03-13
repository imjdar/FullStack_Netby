import { useState, useEffect } from 'react';
import { api } from '../services/api';
import toast from 'react-hot-toast';

interface Product {
  id: number;
  name: string;
  stockQuantity: number;
  price: number;
}

/**
 * Página principal de transacciones (Entradas y Salidas).
 * Maneja la selección de productos, validación de stock, registro 
 * de la operación interactuando con TransactionsAPI.
 */
export default function TransactionsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [tab, setTab] = useState<'Compra' | 'Venta'>('Compra');
  const [productId, setProductId] = useState('');
  const [quantity, setQuantity] = useState('1');
  const [unitPrice, setUnitPrice] = useState('');
  const [detail, setDetail] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => { loadProducts(); }, []);

  async function loadProducts() {
    try {
      const res = await api.products.getAll();
      setProducts(res.data);
    } catch { toast.error('Error al cargar productos'); }
  }

  function onProductChange(id: string) {
    setProductId(id);
    const p = products.find(x => x.id === Number(id));
    if (p) setUnitPrice(String(p.price));
  }

  const selectedProduct = products.find(p => p.id === Number(productId));
  const total = (Number(quantity) || 0) * (Number(unitPrice) || 0);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!productId) { toast.error('Selecciona un producto'); return; }
    
    const qty = Number(quantity);
    const price = Number(unitPrice);

    // ── Validaciones Frontend ──
    if (qty <= 0) {
      toast.error('La cantidad debe ser mayor a 0');
      return;
    }
    
    if (price < 0) {
      toast.error('El precio no puede ser negativo');
      return;
    }
    
    if (tab === 'Venta' && selectedProduct) {
      if (qty > selectedProduct.stockQuantity) {
        toast.error(`Stock insuficiente. Disponible: ${selectedProduct.stockQuantity}`);
        return;
      }
    }

    setSubmitting(true);

    try {
      await api.transactions.create({
        productId: Number(productId),
        type: tab === 'Compra' ? 0 : 1,
        quantity: Number(quantity),
        unitPrice: Number(unitPrice),
        detail
      });
      toast.success(`${tab} registrada`);
      setQuantity('1'); setDetail('');
      loadProducts();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Error al registrar';
      toast.error(msg);
    } finally { setSubmitting(false); }
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Movimientos</h1>
          <p className="page-subtitle">Registrar entrada o salida de inventario</p>
        </div>
      </div>

      <div className="card">
        <div className="tabs">
          <button className={`tab ${tab === 'Compra' ? 'active' : ''}`} onClick={() => setTab('Compra')}>Compra (Entrada)</button>
          <button className={`tab ${tab === 'Venta' ? 'active' : ''}`} onClick={() => setTab('Venta')}>Venta (Salida)</button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Producto</label>
            <select className="form-input" value={productId} onChange={e => onProductChange(e.target.value)} required>
              <option value="">Seleccionar producto...</option>
              {products.map(p => (
                <option key={p.id} value={p.id}>{p.name} — Stock: {p.stockQuantity}</option>
              ))}
            </select>
          </div>

          {selectedProduct && (
            <div className="stats-grid" style={{ marginBottom: 16 }}>
              <div className="card" style={{ padding: 12 }}>
                <div className="stat-label">Stock actual</div>
                <div className="stat-value" style={{ fontSize: 20 }}>{selectedProduct.stockQuantity} uds.</div>
              </div>
              <div className="card" style={{ padding: 12 }}>
                <div className="stat-label">Después de {tab === 'Compra' ? 'entrada' : 'salida'}</div>
                <div className="stat-value" style={{ fontSize: 20 }}>
                  {tab === 'Compra'
                    ? selectedProduct.stockQuantity + (Number(quantity) || 0)
                    : selectedProduct.stockQuantity - (Number(quantity) || 0)} uds.
                </div>
              </div>
            </div>
          )}

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div className="form-group">
              <label className="form-label">Cantidad</label>
              <input className="form-input" type="number" min="1" value={quantity} onChange={e => setQuantity(e.target.value)} required />
            </div>
            <div className="form-group">
              <label className="form-label">Precio unitario</label>
              <input className="form-input" type="number" min="0" step="0.01" value={unitPrice} onChange={e => setUnitPrice(e.target.value)} required />
            </div>
          </div>

          <div className="form-group">
            <label className="form-label">Notas</label>
            <textarea className="form-input" value={detail} onChange={e => setDetail(e.target.value)} placeholder="Opcional..." />
          </div>

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 20 }}>
            <div>
              <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>Total: </span>
              <span style={{ fontSize: 18, fontWeight: 700 }}>${total.toLocaleString('es-CO', { minimumFractionDigits: 2 })}</span>
            </div>
            <button className="btn btn-primary" type="submit" disabled={submitting}>
              {submitting ? 'Procesando...' : `Registrar ${tab}`}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
