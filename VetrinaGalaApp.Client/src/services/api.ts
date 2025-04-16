import { AuthState } from '../context/AuthContext.tsx';

// Define the shape of the API error response
interface ApiErrorResponse {
    type: string;
    title: string;
    status: number;
    detail?: string;
    errors?: Record<string, string[]>; // For validation errors
}

// Function to get the token from wherever AuthProvider stores it (e.g., localStorage)
const getToken = (): string | null => {
    const authData = localStorage.getItem('auth');
    if (authData) {
        try {
            const parsed: AuthState = JSON.parse(authData);
            return parsed.token;
        } catch (e) {
            console.error("Failed to parse auth data from localStorage", e);
            localStorage.removeItem('auth'); // Clear corrupted data
            return null;
        }
    }
    return null;
};


const apiClient = async <T>(
    endpoint: string,
    method: 'GET' | 'POST' | 'PUT' | 'DELETE' = 'GET',
    body?: unknown,
    needsAuth: boolean = true // Default to needing auth
): Promise<T> => {
    const headers: HeadersInit = {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
    };

    const token = getToken();
    if (needsAuth && token) {
        headers['Authorization'] = `Bearer ${token}`;
    } else if (needsAuth && !token) {
        // Optional: Handle cases where auth is needed but token is missing early
        // For instance, redirect to login or throw a specific error
         console.warn(`Auth required for ${endpoint}, but no token found.`);
        // throw new Error("Authentication token is missing.");
    }

    const config: RequestInit = {
        method: method,
        headers: headers,
    };

    if (body) {
        config.body = JSON.stringify(body);
    }

    // Use the proxy path defined in vite.config.ts
    const response = await fetch(`/api${endpoint}`, config);

    if (!response.ok) {
        let errorData: ApiErrorResponse | string;
        try {
            // Try to parse structured error from backend (like ValidationProblemDetails)
            errorData = await response.json();
        } catch (e) {
            // Fallback if the error response is not JSON
            errorData = await response.text();
        }
        console.error(`API Error (${response.status}) on ${endpoint}:`, errorData);
        // Throw an object that includes the status and parsed data
        throw { status: response.status, data: errorData };
    }

    // Handle potential "204 No Content" responses or similar
    if (response.status === 204 || response.headers.get('content-length') === '0') {
         // If the expected return type T allows null/void, return accordingly.
         // Here we cast to T, assuming the caller expects no content for this case.
         // Adjust if specific handling for T is needed.
        return null as T;
    }

    // If response is OK and has content, parse JSON
    const data: T = await response.json();
    return data;
};

export default apiClient;

// Define backend response types based on AuthEndPoints.cs
export interface AuthenticationResult {
    subId: string; // Guid translates to string in JSON
    email: string;
    token: string;
}

export interface ClaimDto {
    type: string;
    value: string;
}