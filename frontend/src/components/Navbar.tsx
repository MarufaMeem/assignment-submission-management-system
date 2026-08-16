"use client";

import { useAuth } from "@/context/AuthContext";
import { LogOut, Menu } from "lucide-react";

interface NavbarProps {
    onMenuClick: () => void;
}

export default function Navbar({ onMenuClick }: NavbarProps) {
    const { user, logout } = useAuth();

    if (!user) return null;

    return (
        <header className="bg-white border-b border-gray-100 shadow-sm sticky top-0 z-20">
            <div className="px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between h-16">
                    <div className="flex items-center md:hidden">
                        <button
                            type="button"
                            className="inline-flex items-center justify-center p-2 rounded-xl text-gray-500 hover:text-gray-900 hover:bg-gray-100 focus:outline-none"
                            onClick={onMenuClick}
                        >
                            <span className="sr-only">Open sidebar</span>
                            <Menu className="h-6 w-6" aria-hidden="true" />
                        </button>
                    </div>

                    <div className="flex-1 flex items-center justify-center md:justify-end">
                        {/* Mobile Logo area or Search can go here */}
                    </div>

                    <div className="flex items-center md:hidden space-x-4">
                        <div className="text-sm text-right hidden sm:block">
                            <p className="font-semibold text-gray-900">{user.email}</p>
                            <p className="text-xs text-gray-500">{user.role}</p>
                        </div>
                        <button
                            onClick={logout}
                            className="p-2 text-red-600 hover:bg-red-50 rounded-xl"
                        >
                            <LogOut className="w-5 h-5" />
                        </button>
                    </div>
                </div>
            </div>
        </header>
    );
}
