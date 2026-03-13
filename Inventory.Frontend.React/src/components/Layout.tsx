import { NavLink, Outlet } from 'react-router-dom';
import { LayoutGrid, ArrowLeftRight, History } from 'lucide-react';
import { Toaster } from 'react-hot-toast';

export default function Layout() {
  return (
    <div className="app-layout">
      <Toaster position="top-right" />
      
      <aside className="sidebar">
        <div className="sidebar-logo">Inventory</div>
        <nav className="sidebar-nav">
          <NavLink to="/" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
            <LayoutGrid size={18} />
            Productos
          </NavLink>
          <NavLink to="/transactions" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
            <ArrowLeftRight size={18} />
            Movimientos
          </NavLink>
          <NavLink to="/history" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
            <History size={18} />
            Historial
          </NavLink>
        </nav>
      </aside>

      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
