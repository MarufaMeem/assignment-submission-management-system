"use client";

import { useAuth } from "@/context/AuthContext";
import { LogOut, BookOpen } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";

export default function Sidebar() {
    const { user, logout } = useAuth();
    const pathname = usePathname();

    if (!user) return null;

    const baseDashboardPath = `/${user.role.toLowerCase()}`;

    let navItems = [
        { name: 'Dashboard', path: baseDashboardPath, icon: 'Home' }
    ];

    if (user.role === 'Admin') {
        navItems = [
            ...navItems,
            { name: 'Users', path: '/admin/users', icon: 'Users' },
            { name: 'Classes & Subjects', path: '/admin/classes', icon: 'Book' },
        ];
    } else if (user.role === 'Teacher') {
        navItems = [
            ...navItems,
            { name: 'Assignments', path: '/teacher/assignments', icon: 'FileText' },
        ];
    } else if (user.role === 'Student') {
        navItems = [
            ...navItems,
            { name: 'My Assignments', path: '/student/assignments', icon: 'FileText' },
        ];
    }

    return (
        <div className="hidden md:flex flex-col w-64 bg-white border-r border-gray-100 min-h-screen shadow-[4px_0_24px_rgba(0,0,0,0.02)]">
            <div className="flex items-center space-x-3 p-6 border-b border-gray-50">
                <div className="bg-indigo-600 p-2 rounded-xl">
                    <BookOpen className="w-5 h-5 text-white" />
                </div>
                <span className="text-xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-indigo-600 to-purple-600">EduSys</span>
            </div>

            <div className="flex-1 py-6 px-4 space-y-2 overflow-y-auto">
                <div className="px-3 pb-2 text-xs font-semibold text-gray-400 uppercase tracking-wider">
                    Menu
                </div>
                {navItems.map((item) => {
                    const isActive = pathname === item.path || pathname.startsWith(`${item.path}/`);
                    return (
                        <Link
                            key={item.path}
                            href={item.path}
                            className={`flex items-center px-4 py-3 text-sm font-medium rounded-xl transition-all duration-200 ${isActive
                                    ? 'bg-indigo-50 text-indigo-700 shadow-sm shadow-indigo-100/50 relative overflow-hidden'
                                    : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
                                }`}
                        >
                            {isActive && <div className="absolute left-0 top-0 bottom-0 w-1 bg-indigo-600 rounded-r-lg" />}
                            <span>{item.name}</span>
                        </Link>
                    )
                })}
            </div>

            <div className="p-4 border-t border-gray-50">
                <div className="bg-gray-50 rounded-2xl p-4 flex flex-col space-y-3">
                    <div className="flex items-center space-x-3">
                        <div className="w-10 h-10 rounded-full bg-gradient-to-tr from-indigo-200 to-purple-200 flex items-center justify-center text-indigo-700 font-bold">
                            {user.email.charAt(0).toUpperCase()}
                        </div>
                        <div className="flex-1 overflow-hidden">
                            <p className="text-sm font-semibold text-gray-900 truncate">{user.email}</p>
                            <p className="text-xs text-gray-500 font-medium">{user.role}</p>
                        </div>
                    </div>
                    <button
                        onClick={logout}
                        className="flex items-center justify-center w-full space-x-2 px-4 py-2 text-sm text-red-600 hover:bg-red-50 rounded-xl transition-colors"
                    >
                        <LogOut className="w-4 h-4" />
                        <span className="font-semibold">Sign Out</span>
                    </button>
                </div>
            </div>
        </div>
    );
}
