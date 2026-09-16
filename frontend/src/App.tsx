import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { RutaProtegida } from './components/RutaProtegida'
import { AuthProvider } from './context/AuthContext'
import { LoginPage } from './pages/LoginPage'
import { RegistroPage } from './pages/RegistroPage'
import { SimuladorPage } from './pages/SimuladorPage'

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/registro" element={<RegistroPage />} />

          <Route element={<RutaProtegida />}>
            <Route path="/simulador" element={<SimuladorPage />} />
          </Route>

          <Route path="*" element={<Navigate to="/simulador" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
