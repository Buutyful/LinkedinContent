// src/context/AuthContext.tsx
import React, {
    createContext,
    useState,
    useContext,
    useEffect,
    ReactNode,
    useCallback
} from 'react';
import apiClient, { AuthenticationResult } from '../services/api';
import { useNavigate } from 'react-router-dom';

// Define the shape of the User object based on what we store
type User = {
    subId: string; // Guid comes as string
    email: string;
};

// Define the shape of the Authentication State
export interface AuthState {
    token: string | null;
    user: User | null;
    isAuthenticated: boolean;
    isLoading: boolean; // To handle initial loading/checking state
    error: string | null; // To store login/registration/callback errors
}

// Define the shape of the Context value
interface AuthContextType extends AuthState {
    login: (email: string, password: string) => Promise<void>;
    register: (username: string, email: string, password: string) => Promise<void>;
    logout: () => void;
    clearError: () => void; // Function to clear errors manually if needed
    handleGoogleLogin: (authResult: AuthenticationResult) => void; // Function to handle Google callback data
}

// Create the context
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Key for storing auth data in localStorage
const AUTH_STORAGE_KEY = 'auth';

// AuthProvider Component
export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    // Initialize state
    const [authState, setAuthState] = useState<AuthState>({
        token: null,
        user: null,
        isAuthenticated: false,
        isLoading: true, // Start as loading until initial check is done
        error: null,
    });
    // Hook for navigation
    const navigate = useNavigate();

    // --- Centralized Auth State Update Function ---
    const updateAuthState = useCallback((result: AuthenticationResult | null) => {
        if (result) {
            // User is authenticated (Login, Register, Google Callback success)
            const newState: AuthState = {
                token: result.token,
                user: { subId: result.subId, email: result.email },
                isAuthenticated: true,
                isLoading: false, // Finished loading/processing auth action
                error: null,       // Clear any previous errors
            };
            // Persist essential data to localStorage
            localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify({ token: newState.token, user: newState.user }));
            // Update the React state
            setAuthState(newState);
            console.log("Auth state updated:", newState.user?.email);
        } else {
            // User is logged out or an error occurred requiring state clear
            localStorage.removeItem(AUTH_STORAGE_KEY); // Clear storage
            // Update the React state to logged-out status
            setAuthState({
                token: null,
                user: null,
                isAuthenticated: false,
                isLoading: false, // Finished loading/processing auth action
                error: null,       // Clear errors on logout
            });
            console.log("Auth state cleared (logout or clear needed).");
        }
    }, []); // No dependencies needed as it only uses its arguments and setAuthState

    // --- Login Function ---
    const login = useCallback(async (email: string, password: string) => {
        setAuthState(prev => ({ ...prev, isLoading: true, error: null })); // Set loading, clear error
        try {
            const result = await apiClient<AuthenticationResult>(
                '/auth/login',
                'POST',
                { email, password },
                false // Login doesn't need prior auth
            );
            updateAuthState(result); // Update state using the common function
            navigate('/'); // Navigate to home on successful login
        } catch (error: any) {
            console.error("Login failed:", error);
            const errorMessage = error?.data?.title || error?.data || 'Login failed. Please check your credentials.';
            // Update state to show error and ensure logged-out status
            setAuthState(prev => ({
                ...prev,
                isLoading: false,
                error: errorMessage as string,
                isAuthenticated: false,
                user: null,
                token: null
            }));
            localStorage.removeItem(AUTH_STORAGE_KEY); // Clean up potentially partial storage
        }
    }, [navigate, updateAuthState]); // Dependencies: navigate and updateAuthState

    // --- Registration Function ---
    const register = useCallback(async (userName: string, email: string, password: string) => {
        setAuthState(prev => ({ ...prev, isLoading: true, error: null })); // Set loading, clear error
        try {
            // Assuming register endpoint returns AuthenticationResult upon success
            const result = await apiClient<AuthenticationResult>(
                '/auth/register',
                'POST',
                { userName, email, password },
                false // Register doesn't need prior auth
            );
            console.log("Registration successful, logging user in:", result);
            updateAuthState(result); // Log the user in immediately after successful registration
            navigate('/'); // Navigate to home
        } catch (error: any) {
            console.error("Registration failed:", error);
            // Parse potential validation errors or generic errors
            let errorMessage = 'Registration failed.';
            if (error?.data?.errors) {
                errorMessage = Object.values(error.data.errors).flat().join(' ');
            } else if (error?.data?.title) {
                errorMessage = error.data.title;
            } else if (typeof error?.data === 'string') {
                errorMessage = error.data;
            }
             // Update state to show error and ensure logged-out status
            setAuthState(prev => ({
                ...prev,
                isLoading: false,
                error: errorMessage,
                isAuthenticated: false,
                user: null,
                token: null
            }));
            localStorage.removeItem(AUTH_STORAGE_KEY); // Clean up storage
        }
    }, [navigate, updateAuthState]); // Dependencies: navigate and updateAuthState

    // --- Google Login Callback Handler ---
    const handleGoogleLogin = useCallback((authResult: AuthenticationResult) => {
        console.log("Handling Google login callback data in AuthContext.");
        setAuthState(prev => ({ ...prev, isLoading: true, error: null })); // Set loading briefly
        updateAuthState(authResult); // Use the common function to set state
        // Navigation is handled by the GoogleCallbackPage component itself after calling this
    }, [updateAuthState]); // Dependency: updateAuthState

    // --- Logout Function ---
    const logout = useCallback(() => {
        console.log("Logout requested.");
        updateAuthState(null); // Use the common function to clear state
        navigate('/login'); // Redirect to login page after logout
    }, [navigate, updateAuthState]); // Dependencies: navigate and updateAuthState

    // --- Function to manually clear displayed errors ---
    const clearError = useCallback(() => {
        setAuthState(prev => ({ ...prev, error: null }));
    }, []);

    // --- Effect to Load and Verify Auth State on Initial Mount ---
    useEffect(() => {
        console.log("AuthProvider mounted. Checking auth status...");
        const storedAuth = localStorage.getItem(AUTH_STORAGE_KEY);
        let initialToken: string | null = null;
        let initialUser: User | null = null;
        let needsVerification = false;

        if (storedAuth) {
            try {
                const parsed: Pick<AuthState, 'token' | 'user'> = JSON.parse(storedAuth);
                // Basic validation: check if token and user (with expected fields) exist
                if (parsed.token && parsed.user?.subId && parsed.user?.email) {
                    initialToken = parsed.token;
                    initialUser = parsed.user;
                    needsVerification = true; // Found potentially valid token, need to check it
                    console.log("Found token in localStorage for user:", initialUser.email);
                } else {
                     console.warn("Stored auth data is incomplete or malformed.");
                     localStorage.removeItem(AUTH_STORAGE_KEY); // Clear invalid data
                }
            } catch (error) {
                console.error("Failed to parse stored auth data:", error);
                localStorage.removeItem(AUTH_STORAGE_KEY); // Clear corrupted data
            }
        } else {
             console.log("No auth data found in localStorage.");
        }

        // If a token was found, attempt to verify it with the backend
        if (needsVerification && initialToken && initialUser) {
            // Optimistically set state, but keep isLoading true until verification completes
            setAuthState({
                token: initialToken,
                user: initialUser,
                isAuthenticated: true, // Assume true for now
                isLoading: true,       // Still loading (verifying)
                error: null,
            });

            console.log("Verifying token with backend via /auth/check...");
            apiClient<void>('/auth/check', 'GET', undefined, true) // Needs auth header
                .then(() => {
                    console.log("Token verification successful.");
                    // Token is valid, confirmation state is already set, just stop loading
                    setAuthState(prev => ({ ...prev, isLoading: false }));
                })
                .catch((error) => {
                    console.error("Token verification failed:", error);
                    // Token is invalid or expired, clear the state completely
                    updateAuthState(null); // Use common function to clear everything
                    // Set loading false and potentially show an error message
                    setAuthState(prev => ({ ...prev, isLoading: false, error: "Your session has expired. Please log in again." }));
                    // Optional: Force navigation to login if the current route needs auth?
                    // This is often handled by ProtectedRoute instead.
                });
        } else {
            // No token found or it was invalid from the start, finish initial loading
             console.log("No token verification needed or token was invalid initially.");
            setAuthState(prev => ({ ...prev, isLoading: false }));
        }
    }, [updateAuthState]); // Dependency: updateAuthState (safe because it's memoized)

    // Provide the context value to children
    return (
        <AuthContext.Provider value={{ ...authState, login, register, logout, clearError, handleGoogleLogin }}>
            {children}
        </AuthContext.Provider>
    );
};

// Custom hook to use the AuthContext
export const useAuth = (): AuthContextType => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        // This error prevents using the hook outside of the AuthProvider
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};