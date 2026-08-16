"use client";

import React from 'react';
import { useAuth } from '@/context/AuthContext';
import { FileSignature, Star, Clock } from 'lucide-react';
import Link from 'next/link';

export default function StudentDashboard() {
    const { user } = useAuth();

    return (
        <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">Welcome, Student!</h1>
                <p className="mt-1 text-sm text-gray-500">View your active assignments and check your grades.</p>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
                <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 flex items-center space-x-4">
                    <div className="p-4 rounded-xl bg-orange-100 text-orange-600">
                        <Clock className="w-8 h-8" />
                    </div>
                    <div>
                        <p className="text-sm font-medium text-gray-500">Pending</p>
                        <p className="text-3xl font-bold text-gray-900">4</p>
                    </div>
                </div>
                <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 flex items-center space-x-4">
                    <div className="p-4 rounded-xl bg-emerald-100 text-emerald-600">
                        <FileSignature className="w-8 h-8" />
                    </div>
                    <div>
                        <p className="text-sm font-medium text-gray-500">Submitted</p>
                        <p className="text-3xl font-bold text-gray-900">12</p>
                    </div>
                </div>
                <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 flex items-center space-x-4">
                    <div className="p-4 rounded-xl bg-indigo-100 text-indigo-600">
                        <Star className="w-8 h-8" />
                    </div>
                    <div>
                        <p className="text-sm font-medium text-gray-500">Grades Received</p>
                        <p className="text-3xl font-bold text-gray-900">9</p>
                    </div>
                </div>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 mt-8">
                <h2 className="text-lg font-bold text-gray-900 mb-4">Start Learning</h2>
                <Link href="/student/assignments" className="inline-block px-6 py-3 bg-indigo-600 hover:bg-indigo-700 text-white font-medium rounded-xl transition-colors shadow-sm shadow-indigo-200">
                    Browse My Assignments
                </Link>
            </div>
        </div>
    );
}
