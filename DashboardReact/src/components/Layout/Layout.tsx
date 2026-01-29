import { Outlet, Link } from 'react-router-dom';
import './Layout.css';

const Layout = () => {
  return (
    <div className="layout">
      <header className="header">
        <div className="header-content">
          <h1 className="header-title">IoT Dashboard</h1>
          <nav className="nav">
            <Link to="/iot-dashboard" className="nav-link">Dashboard</Link>
          </nav>
        </div>
      </header>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
};

export default Layout;
