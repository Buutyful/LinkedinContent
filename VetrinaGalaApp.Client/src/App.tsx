import React from 'react';
import { Routes, Route, Link, Outlet } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ProtectedRoute from './components/ProtectedRoute';
import { useAuth } from './context/AuthContext';
import './App.css';

// Example Placeholder Components
const HomePage: React.FC = () => {
    const { user, logout } = useAuth();
    return (
        <div>
            <h2>Welcome, {user?.email}!</h2>
            <p>This is the protected home page.</p>
             <p>Your Sub ID: {user?.subId}</p>
            <button onClick={logout}>Logout</button>
        </div>
    );
};

const PublicPage: React.FC = () => <h2>Public Page</h2>;

// Layout component for consistent structure (optional but good practice)
const Layout: React.FC = () => {
    const { isAuthenticated, logout, user, isLoading } = useAuth();

    return (
        <div>
            <nav>
                <ul>
                    <li><Link to="/">Home (Protected)</Link></li>
                    <li><Link to="/public">Public Page</Link></li>
                    {!isLoading && !isAuthenticated && (
                         <>
                            <li><Link to="/login">Login</Link></li>
                             <li><Link to="/register">Register</Link></li>
                         </>
                     )}
                     {isAuthenticated && user && (
                         <li>
                             <span>Logged in as {user.email} </span>
                             <button onClick={logout} style={{ marginLeft: '10px', background: 'none', border: 'none', color: 'blue', textDecoration: 'underline', cursor: 'pointer' }}>Logout</button>
                         </li>
                    )}
                     {isLoading && <li>Loading...</li>}
                </ul>
            </nav>
            <hr />
            <main>
                {/* Outlet renders the matched child route component */}
                <Outlet />
            </main>
        </div>
    );
}


function App() {
    return (
        <Routes>
            <Route path="/" element={<Layout />}> {/* Use Layout for structure */}
                {/* Public Routes */}
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/public" element={<PublicPage />} />

                 {/* Protected Routes */}
                <Route
                    index // Makes HomePage the default route for "/" when authenticated
                    element={
                        <ProtectedRoute>
                            <HomePage />
                        </ProtectedRoute>
                    }
                />
                 {/* Add other protected routes here */}
                 {/* Example:
                 <Route
                    path="/dashboard"
                    element={
                        <ProtectedRoute>
                            <DashboardPage />
                        </ProtectedRoute>
                    }
                />
                */}

                 {/* Catch-all for undefined routes (optional) */}
                 <Route path="*" element={<h2>404 Not Found</h2>} />
            </Route>
        </Routes>
    );
}

export default App;