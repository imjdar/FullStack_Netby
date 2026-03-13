import { useState, useEffect } from 'react';
import { api } from '../services/api';
import toast from 'react-hot-toast';

interface Transaction {
  id: number;
  date: string;
  type: string;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  detail: string;
}

export default function HistoryPage() {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [filterType, setFilterType] = useState('');

  useEffect(() => { loadTransactions(); }, [filterType]);

  async function loadTransactions() {
    try {
      const params: Record<string, string> = {};
      if (filterType) params.type = filterType;
      const res = await api.transactions.getAll(params);
      setTransactions(res.data);
    } catch { toast.error('Error al cargar historial'); }
    finally { setLoading(false); }
  }

  if (loading) return <div className="empty-state">Cargando...</div>;

  const totalCompras = transactions.filter(t => t.type === 'Compra').reduce((s, t) => s + t.totalPrice, 0);
  const totalVentas = transactions.filter(t => t.type === 'Venta').reduce((s, t) => s + t.totalPrice, 0);

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Historial</h1>
          <p className="page-subtitle">{transactions.length} movimientos registrados</p>
        </div>
        <select className="form-input" style={{ width: 180 }} value={filterType} onChange={e => setFilterType(e.target.value)}>
          <option value="">Todos</option>
          <option value="0">Compras</option>
          <option value="1">Ventas</option>
        </select>
      </div>

      <div className="stats-grid">
        <div className="card">
          <div className="stat-label">Compras</div>
          <div className="stat-value stock-ok">${totalCompras.toLocaleString('es-CO', { minimumFractionDigits: 2 })}</div>
        </div>
        <div className="card">
          <div className="stat-label">Ventas</div>
          <div className="stat-value" style={{ color: 'var(--accent)' }}>${totalVentas.toLocaleString('es-CO', { minimumFractionDigits: 2 })}</div>
        </div>
        <div className="card">
          <div className="stat-label">Movimientos</div>
          <div className="stat-value">{transactions.length}</div>
        </div>
      </div>

      <div className="card">
        {transactions.length === 0 ? (
          <div className="empty-state">No hay movimientos registrados.</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Fecha</th>
                <th>Tipo</th>
                <th>Producto</th>
                <th>Cantidad</th>
                <th>P. Unitario</th>
                <th>Total</th>
              </tr>
            </thead>
            <tbody>
              {transactions.map(t => (
                <tr key={t.id}>
                  <td style={{ fontSize: 13, color: 'var(--text-secondary)' }}>
                    {new Date(t.date).toLocaleDateString('es-CO', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' })}
                  </td>
                  <td>
                    <span className={`badge ${t.type === 'Compra' ? 'badge-green' : 'badge-red'}`}>
                      {t.type === 'Compra' ? 'Entrada' : 'Salida'}
                    </span>
                  </td>
                  <td style={{ fontWeight: 500 }}>{t.productName || `#${t.productId}`}</td>
                  <td>{t.quantity} uds.</td>
                  <td>${t.unitPrice.toLocaleString('es-CO', { minimumFractionDigits: 2 })}</td>
                  <td style={{ fontWeight: 600 }}>${t.totalPrice.toLocaleString('es-CO', { minimumFractionDigits: 2 })}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
