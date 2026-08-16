"use client";

import React from 'react';
import { useAuth } from '@/context/AuthContext';
import { FileText, CheckCircle, Clock } from 'lucide-react';
import Link from 'next/link';

export default function TeacherDashboard() {
    const { user } = useAuth();

    const stats = [
        { title: "Active Assignments", value: 12, icon: FileText, color: "text-blue-600", bg: "bg-blue-100" },
        { title: "Submissions to Review", value: 45, icon: Clock, color: "text-amber-600", bg: "bg-amber-100" },
        { title: "Reviewed Submissions", value: 128, icon: CheckCircle, color: "text-emerald-600", bg: "bg-emerald-100" },
    ];

    return (
        <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">Welcome, {user?.email.split('@')[0]}!</h1>
                <p className="mt-1 text-sm text-gray-500">Manage your assignments and evaluate student submissions.</p>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                {stats.map((stat, i) => {
                    const Icon = stat.icon;
                    return (
                        <div key={i} className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 flex items-center space-x-4">
                            <div className={`p-4 rounded-xl ${stat.bg} ${stat.color}`}>
                                <Icon className="w-8 h-8" />
                            </div>
                            <div>
                                <p className="text-sm font-medium text-gray-500">{stat.title}</p>
                                <p className="text-3xl font-bold text-gray-900">{stat.value}</p>
                            </div>
                        </div>
                    );
                })}
            </div>

            <div className="mt-8">
                <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                    <div className="p-6 border-b border-gray-100 bg-gray-50/50 flex justify-between items-center">
                        <h2 className="text-lg font-bold text-gray-900">Quick Actions</h2>
                    </div>
                    <div className="p-6 grid grid-cols-1 md:grid-cols-2 gap-4">
                        <Link href="/teacher/assignments" className="flex items-center p-4 bg-gray-50 hover:bg-indigo-50 border border-gray-100 rounded-xl transition-colors group text-left">
                            <div className="p-3 bg-white rounded-lg shadow-sm mr-4 group-hover:bg-indigo-600 group-hover:text-white transition-colors">
                                <FileText className="w-6 h-6" />
                            </div>
                            <div>
                                <h3 className="font-semibold text-gray-900 group-hover:text-indigo-700">Manage Assignments</h3>
                                <p className="text-sm text-gray-500 line-clamp-1">Create, edit, or publish assignments for your classes.</p>
                            </div>
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
}
