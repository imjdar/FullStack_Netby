import { useState, useEffect } from 'react';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { api } from '../services/api';
import toast from 'react-hot-toast';

interface Product {
  id: number;
  name: string;
  description: string;
  category: string;
  price: number;
  stockQuantity: number;
  imageUrl: string;
}

/**
 * Página de administración del catálogo de productos.
 * Permite listar, crear, editar y eliminar productos comunicándose
 * directamente con ProductsAPI. Incluye funcionalidad de subida de imágenes
 * mediante FormData y validaciones previas al envío.
 */
export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<Product | null>(null);

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState('');
  const [price, setPrice] = useState('');
  const [stock, setStock] = useState('');
  const [image, setImage] = useState<File | null>(null);

  useEffect(() => { loadProducts(); }, []);

  async function loadProducts() {
    try {
      const res = await api.products.getAll();
      setProducts(res.data);
    } catch { toast.error('Error al cargar productos'); }
    finally { setLoading(false); }
  }

  function openCreate() {
    setEditing(null);
    setName(''); setDescription(''); setCategory(''); setPrice(''); setStock(''); setImage(null);
    setShowModal(true);
  }

  function openEdit(p: Product) {
    setEditing(p);
    setName(p.name); setDescription(p.description || ''); setCategory(p.category || '');
    setPrice(String(p.price)); setStock(String(p.stockQuantity)); setImage(null);
    setShowModal(true);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    // ── Validaciones Frontend ──
    const pPrice = Number(price);
    const pStock = Number(stock);

    if (pPrice < 0) {
      toast.error('El precio no puede ser negativo');
      return;
    }

    if (pStock < 0) {
      toast.error('El stock no puede ser negativo');
      return;
    }

    const fd = new FormData();
    fd.append('Name', name);
    fd.append('Description', description);
    fd.append('Category', category);
    fd.append('Price', price);
    fd.append('StockQuantity', stock);
    if (image) fd.append(editing ? 'NewImage' : 'ArchivoImagen', image);

    try {
      if (editing) {
        await api.products.update(editing.id, fd);
        toast.success('Producto actualizado');
      } else {
        await api.products.create(fd);
        toast.success('Producto creado');
      }
      setShowModal(false);
      loadProducts();
    } catch { toast.error('Error al guardar producto'); }
  }

  async function handleDelete(id: number) {
    if (!confirm('¿Eliminar este producto?')) return;
    try {
      await api.products.delete(id);
      toast.success('Producto eliminado');
      loadProducts();
    } catch { toast.error('Error al eliminar'); }
  }

  function stockStatus(qty: number) {
    if (qty <= 5) return 'stock-critical';
    if (qty <= 15) return 'stock-low';
    return 'stock-ok';
  }

  if (loading) return <div className="empty-state">Cargando...</div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Productos</h1>
          <p className="page-subtitle">{products.length} productos registrados</p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>
          <Plus size={16} /> Nuevo producto
        </button>
      </div>

      <div className="card">
        {products.length === 0 ? (
          <div className="empty-state">No hay productos registrados.</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th></th>
                <th>Nombre</th>
                <th>Categoría</th>
                <th>Precio</th>
                <th>Stock</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {products.map(p => (
                <tr key={p.id}>
                  <td><img className="product-img" src={p.imageUrl || '/images/default.png'} alt={p.name} /></td>
                  <td>
                    <div style={{ fontWeight: 500 }}>{p.name}</div>
                    {p.description && <div style={{ fontSize: 13, color: 'var(--text-secondary)' }}>{p.description}</div>}
                  </td>
                  <td><span className="badge badge-blue">{p.category || '—'}</span></td>
                  <td style={{ fontWeight: 500 }}>${p.price.toLocaleString('es-CO', { minimumFractionDigits: 2 })}</td>
                  <td><span className={stockStatus(p.stockQuantity)} style={{ fontWeight: 600 }}>{p.stockQuantity} uds.</span></td>
                  <td>
                    <div style={{ display: 'flex', gap: 4 }}>
                      <button className="btn btn-secondary btn-sm" onClick={() => openEdit(p)}><Pencil size={14} /></button>
                      <button className="btn btn-danger btn-sm" onClick={() => handleDelete(p.id)}><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h2 className="modal-title">{editing ? 'Editar producto' : 'Nuevo producto'}</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label className="form-label">Nombre</label>
                <input className="form-input" value={name} onChange={e => setName(e.target.value)} required />
              </div>
              <div className="form-group">
                <label className="form-label">Descripción</label>
                <textarea className="form-input" value={description} onChange={e => setDescription(e.target.value)} />
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="form-group">
                  <label className="form-label">Categoría</label>
                  <input className="form-input" value={category} onChange={e => setCategory(e.target.value)} />
                </div>
                <div className="form-group">
                  <label className="form-label">Precio</label>
                  <input className="form-input" type="number" min="0" step="0.01" value={price} onChange={e => setPrice(e.target.value)} required />
                </div>
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="form-group">
                  <label className="form-label">Stock inicial</label>
                  <input className="form-input" type="number" min="0" value={stock} onChange={e => setStock(e.target.value)} required />
                </div>
                <div className="form-group">
                  <label className="form-label">Imagen</label>
                  <input className="form-input" type="file" accept="image/*" onChange={e => setImage(e.target.files?.[0] || null)} />
                </div>
              </div>
              <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 16 }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowModal(false)}>Cancelar</button>
                <button type="submit" className="btn btn-primary">{editing ? 'Guardar' : 'Crear'}</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
