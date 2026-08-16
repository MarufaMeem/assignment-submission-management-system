"use client";

import React, { createContext, useContext, useEffect, useState } from 'react';
import { jwtDecode } from 'jwt-decode';
import { useRouter, usePathname } from 'next/navigation';

export type UserRole = "Admin" | "Teacher" | "Student";

interface DecodedToken {
    sub: string;
    email: string;
    role: UserRole;
    exp: number;
}

interface AuthContextType {
    user: DecodedToken | null;
    login: (token: string) => void;
    logout: () => void;
    isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const [user, setUser] = useState<DecodedToken | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const router = useRouter();
    const pathname = usePathname();

    useEffect(() => {
        const token = localStorage.getItem('token');
        if (token) {
            try {
                const decoded = jwtDecode<DecodedToken>(token);
                if (decoded.exp * 1000 > Date.now()) {
                    setUser(decoded);
                } else {
                    localStorage.removeItem('token');
                }
            } catch {
                localStorage.removeItem('token');
            }
        }
        setIsLoading(false);
    }, []);

    // Routing Logic / Middleware substitute
    useEffect(() => {
        if (isLoading) return;

        const publicPaths = ['/login'];
        // if user is on a protected route but not logged in
        if (!user && !publicPaths.includes(pathname)) {
            router.replace('/login');
        } else if (user && pathname === '/') { // Redirect root to dashboard
            switch (user.role) {
                case 'Admin': router.replace('/admin'); break;
                case 'Teacher': router.replace('/teacher'); break;
                case 'Student': router.replace('/student'); break;
            }
        } else if (user && publicPaths.includes(pathname)) {
            // Already logged in, no need to be on login page
            switch (user.role) {
                case 'Admin': router.replace('/admin'); break;
                case 'Teacher': router.replace('/teacher'); break;
                case 'Student': router.replace('/student'); break;
            }
        }
    }, [user, isLoading, pathname, router]);

    const login = (token: string) => {
        localStorage.setItem('token', token);
        const decoded = jwtDecode<DecodedToken>(token);
        setUser(decoded);

        switch (decoded.role) {
            case 'Admin': router.push('/admin'); break;
            case 'Teacher': router.push('/teacher'); break;
            case 'Student': router.push('/student'); break;
            default: router.push('/');
        }
    };

    const logout = () => {
        localStorage.removeItem('token');
        setUser(null);
        router.push('/login');
    };

    return (
        <AuthContext.Provider value={{ user, login, logout, isLoading }}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
}
