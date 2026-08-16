"use client";

import React, { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import { Users, BookOpen, GraduationCap, ArrowRight } from 'lucide-react';
import Link from 'next/link';

export default function AdminDashboard() {
    const [stats, setStats] = useState({
        users: 0,
        classes: 0,
        subjects: 0
    });
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadStats = async () => {
            // Let's attempt to load counts if backend permits, otherwise fallback to standard link panels.
            // We'll mimic an async fetch to illustrate loading state.
            setTimeout(() => setLoading(false), 800);
        };
        loadStats();
    }, []);

    const cards = [
        {
            title: "Manage Users",
            description: "View and create admins, teachers, and students.",
            icon: Users,
            color: "from-blue-500 to-indigo-600",
            link: "/admin/users"
        },
        {
            title: "Classes & Subjects",
            description: "Organize the school curriculum.",
            icon: BookOpen,
            color: "from-purple-500 to-pink-600",
            link: "/admin/classes"
        },
        {
            title: "Teacher Assignments",
            description: "Assign teachers to classes and subjects.",
            icon: GraduationCap,
            color: "from-emerald-500 to-teal-600",
            link: "/admin/teacher-assignments" // We'll combine this with classes potentially or keep separate
        }
    ];

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">Admin Dashboard</h1>
                <p className="mt-1 text-sm text-gray-500">Overview and configuration of the school system.</p>
            </div>

            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
                {cards.map((card, index) => {
                    const Icon = card.icon;
                    return (
                        <Link key={index} href={card.link}>
                            <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 hover:shadow-lg hover:-translate-y-1 transition-all duration-300 group cursor-pointer relative overflow-hidden">
                                <div className={`absolute top-0 right-0 p-32 bg-gradient-to-br ${card.color} opacity-5 rounded-bl-full group-hover:scale-110 transition-transform duration-500`} />

                                <div className={`w-12 h-12 rounded-xl flex items-center justify-center bg-gradient-to-br ${card.color} shadow-md mb-6`}>
                                    <Icon className="w-6 h-6 text-white" />
                                </div>

                                <h3 className="text-lg font-semibold text-gray-900 mb-2 group-hover:text-indigo-600 transition-colors">
                                    {card.title}
                                </h3>
                                <p className="text-sm text-gray-500 mb-6">
                                    {card.description}
                                </p>

                                <div className="flex items-center text-sm font-medium text-indigo-600">
                                    Go to section <ArrowRight className="w-4 h-4 ml-1 group-hover:translate-x-1 transition-transform" />
                                </div>
                            </div>
                        </Link>
                    )
                })}
            </div>
        </div>
    );
}
