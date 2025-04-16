import React, { ReactNode } from 'react'; 
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface ProtectedRouteProps {
    children: ReactNode;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
    const { isAuthenticated, isLoading } = useAuth();
    const location = useLocation();

    if (isLoading) {
        // Show a loading indicator while checking auth status
        // Important to prevent flickering or redirecting before auth check is complete
        return <div>Loading authentication status...</div>;
    }

    if (!isAuthenticated) {
        // Redirect them to the /login page, but save the current location they were
        // trying to go to. This allows us to send them back there after they log in.
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    return children; // User is authenticated, render the requested component
};

export default ProtectedRoute;