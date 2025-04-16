import React, { useState, FormEvent, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { Link, useSearchParams } from 'react-router-dom';

const LoginPage: React.FC = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const { login, isLoading, error, clearError } = useAuth();
    const [searchParams] = useSearchParams();
    // Clear errors when the component mounts or email/password changes
    useEffect(() => {
        clearError();
    }, [clearError]);


    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        clearError(); // Clear previous errors before attempting login
        await login(email, password);
        // Navigation is handled within the login function on success
    };
    const handleGoogleLoginClick = () => {
        // Directly navigate the browser to the backend's initiation endpoint        
        window.location.href = '/api/login/google-initiate';
    };

    return (
        <div>
            <h2>Login</h2>
            <form onSubmit={handleSubmit}>
                <div>
                    <label htmlFor="email">Email:</label>
                    <input
                        type="email"
                        id="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                        disabled={isLoading}
                    />
                </div>
                <div>
                    <label htmlFor="password">Password:</label>
                    <input
                        type="password"
                        id="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                        disabled={isLoading}
                    />
                </div>
                 {error && <p style={{ color: 'red' }}>{error}</p>}
                <button type="submit" disabled={isLoading}>
                    {isLoading ? 'Logging in...' : 'Login'}
                </button>
            </form>
              {/* Google Login Button */}
              <button
                onClick={handleGoogleLoginClick}
                disabled={isLoading}
                style={{ marginTop: '15px', backgroundColor: '#db4437' }} // Example Google color
            >
                {isLoading ? 'Loading...' : 'Login with Google'}
            </button>
            <p>
                Don't have an account? <Link to="/register">Register here</Link>
            </p>
        </div>
    );
};

export default LoginPage;