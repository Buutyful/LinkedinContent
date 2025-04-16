import React, { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext'; // Assuming handleAuthSuccess is exported or recreated here
import { AuthenticationResult } from '../services/api'; // Import the type

const GoogleCallbackPage: React.FC = () => {
    const navigate = useNavigate();
    const location = useLocation();
    // Need a way to set the auth state - let's add a function to AuthContext
    // Or replicate the logic here if preferred (less ideal)
    const { handleGoogleLogin, isLoading } = useAuth(); // We'll add handleGoogleLogin to useAuth
    const [error, setError] = useState<string | null>(null);
    const [processed, setProcessed] = useState(false); // Prevent double processing

    useEffect(() => {
        if (processed) return; // Ensure this runs only once

        const params = new URLSearchParams(location.search);
        const token = params.get('token');
        const email = params.get('email');
        const subId = params.get('subId'); // Guid comes as string

        if (token && email && subId) {
            setProcessed(true); // Mark as processed
             console.log("Received Google auth details:", { token, email, subId });
            const authResult: AuthenticationResult = { token, email, subId };

            // Call a function in AuthContext to set the state and local storage
            handleGoogleLogin(authResult);

            // Redirect to the home page after successful login
            // Use replace to remove the callback URL from history
            navigate('/', { replace: true });

        } else {
            setProcessed(true); // Mark as processed even on error
            console.error("Missing token, email, or subId in Google callback parameters.");
            setError("Login failed: Incomplete information received from Google sign-in.");
            // Optionally redirect back to login page with error after a delay
             setTimeout(() => navigate('/login?error=googlecallbackfail', { replace: true }), 3000);
        }

    }, [location.search, navigate, handleGoogleLogin, processed]); // Add handleGoogleLogin and processed dependency

    // Display loading or error message
    if (isLoading || !processed) { // Show loading if context is loading or we haven't processed yet
        return <div>Processing Google login... Please wait.</div>;
    }

    if (error) {
        return <div>Error during Google Login: {error}</div>;
    }

    // Should ideally redirect away before rendering anything else
    return <div>Redirecting...</div>;
};

export default GoogleCallbackPage;