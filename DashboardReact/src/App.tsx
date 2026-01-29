import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/Layout/Layout';
import Dashboard from './components/Dashboard/Dashboard';
import History from './components/History/History';
import './App.css';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Navigate to="/iot-dashboard" replace />} />
          <Route path="iot-dashboard" element={<Dashboard />} />
          <Route path="history/:mac" element={<History />} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;
