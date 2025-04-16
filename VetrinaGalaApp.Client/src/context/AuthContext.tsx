import React, { createContext, useState, useContext, useEffect, ReactNode, useCallback } from 'react';
import apiClient, { AuthenticationResult } from '../services/api';
import { useNavigate } from 'react-router-dom';

// Types based on backend Endpoints
type User = {
    subId: string;
    email: string;
};

export interface AuthState {
    token: string | null;
    user: User | null;
    isAuthenticated: boolean;
    isLoading: boolean; // To handle initial loading/checking state
    error: string | null; // To store login/registration errors
}

interface AuthContextType extends AuthState {
    login: (email: string, password: string) => Promise<void>;
    register: (username: string, email: string, password: string) => Promise<void>;
    logout: () => void;
    clearError: () => void; // Function to clear errors manually if needed
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const AUTH_STORAGE_KEY = 'auth';

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [authState, setAuthState] = useState<AuthState>({
        token: null,
        user: null,
        isAuthenticated: false,
        isLoading: true, // Start as loading
        error: null,
    });
    const navigate = useNavigate();

    // Effect to load auth state from storage on initial mount
    useEffect(() => {
        const storedAuth = localStorage.getItem(AUTH_STORAGE_KEY);
        let initialToken: string | null = null;
        let validTokenFound = false;

        if (storedAuth) {
            try {
                const parsed: Pick<AuthState, 'token' | 'user'> = JSON.parse(storedAuth);
                if (parsed.token && parsed.user) {
                    initialToken = parsed.token;
                    // Optimistically set state while verifying token
                    setAuthState({
                        token: parsed.token,
                        user: parsed.user,
                        isAuthenticated: true, // Assume true until check fails
                        isLoading: true,       // Still loading until check completes
                        error: null,
                    });
                    validTokenFound = true; // Mark as potentially valid
                }
            } catch (error) {
                console.error("Failed to parse stored auth data:", error);
                localStorage.removeItem(AUTH_STORAGE_KEY); // Clear invalid data
            }
        }

        // If a token was found in storage, verify it with the backend
        if (validTokenFound && initialToken) {
             console.log("Verifying token with backend...");
            apiClient<void>('/auth/check', 'GET', undefined, true) // Needs auth header
                .then(() => {
                    console.log("Token verified successfully.");
                    // State is already optimistically set, just stop loading
                    setAuthState(prev => ({ ...prev, isLoading: false, error: null }));
                })
                .catch((error) => {
                    console.error("Token verification failed:", error);
                    // Token is invalid, clear state and storage
                    localStorage.removeItem(AUTH_STORAGE_KEY);
                    setAuthState({
                        token: null,
                        user: null,
                        isAuthenticated: false,
                        isLoading: false,
                        error: 'Session expired. Please log in again.', // Inform user
                    });
                    // Optional: Redirect to login if verification fails on a protected page context
                    // navigate('/login');
                });
        } else {
            // No valid token found in storage, stop loading
            setAuthState(prev => ({ ...prev, isLoading: false }));
        }
    }, []); // Run only once on mount

    const handleAuthSuccess = (result: AuthenticationResult) => {
        const newState: AuthState = {
            token: result.token,
            user: { subId: result.subId, email: result.email },
            isAuthenticated: true,
            isLoading: false,
            error: null,
        };
        localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify({ token: newState.token, user: newState.user }));
        setAuthState(newState);
    };

    const login = useCallback(async (email: string, password: string) => {
        setAuthState(prev => ({ ...prev, isLoading: true, error: null }));
        try {
            const result = await apiClient<AuthenticationResult>(
                '/auth/login',
                'POST',
                { email, password },
                false // Login doesn't need prior auth
            );
            handleAuthSuccess(result);
            navigate('/'); // Navigate to home/dashboard after login
        } catch (error: any) {
            console.error("Login failed:", error);
            const errorMessage = error?.data?.title || error?.data || 'Login failed. Please check your credentials.';
            setAuthState(prev => ({ ...prev, isLoading: false, error: errorMessage as string }));
            localStorage.removeItem(AUTH_STORAGE_KEY); // Ensure no partial state remains
        }
    }, [navigate]);

    const register = useCallback(async (userName: string, email: string, password: string) => {
        setAuthState(prev => ({ ...prev, isLoading: true, error: null }));
        try {
            // Assuming register endpoint returns AuthenticationResult upon success
            const result = await apiClient<AuthenticationResult>(
                '/auth/register',
                'POST',
                { userName, email, password },
                false // Register doesn't need prior auth
            );
             console.log("Registration successful, user logged in:", result);
            // Log the user in immediately after successful registration
            handleAuthSuccess(result);
            navigate('/'); // Navigate to home/dashboard after registration + login
        } catch (error: any) {
            console.error("Registration failed:", error);
            let errorMessage = 'Registration failed.';
            if (error?.data?.errors) {
                // Handle validation errors (ProblemDetails format)
                errorMessage = Object.values(error.data.errors).flat().join(' ');
            } else if (error?.data?.title) {
                errorMessage = error.data.title;
            } else if (typeof error?.data === 'string') {
                 errorMessage = error.data;
            }
            setAuthState(prev => ({ ...prev, isLoading: false, error: errorMessage }));
            localStorage.removeItem(AUTH_STORAGE_KEY);
        }
    }, [navigate]);

    const logout = useCallback(() => {
        setAuthState({
            token: null,
            user: null,
            isAuthenticated: false,
            isLoading: false,
            error: null,
        });
        localStorage.removeItem(AUTH_STORAGE_KEY);
        navigate('/login'); // Redirect to login after logout
         console.log("User logged out.");
    }, [navigate]);

     const clearError = useCallback(() => {
        setAuthState(prev => ({ ...prev, error: null }));
    }, []);

    return (
        <AuthContext.Provider value={{ ...authState, login, register, logout, clearError }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = (): AuthContextType => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};