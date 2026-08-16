"use client";

import React, { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import { Book, Clock, AlertCircle } from 'lucide-react';
import Link from 'next/link';

interface StudentAssignmentDto {
    id: number;
    title: string;
    description: string;
    className: string;
    subjectName: string;
    createdByTeacherName: string;
    deadline: string;
    maxMarks: number;
    allowLateSubmission: boolean;
}

export default function StudentAssignmentsPage() {
    const [assignments, setAssignments] = useState<StudentAssignmentDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadAssignments = async () => {
            try {
                const data = await fetchApi('/students/assignments');
                setAssignments(data);
            } catch (err) {
                console.error("Failed to load assignments", err);
            } finally {
                setLoading(false);
            }
        };
        loadAssignments();
    }, []);

    if (loading) {
        return (
            <div className="p-12 flex justify-center">
                <div className="w-8 h-8 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin"></div>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">My Assignments</h1>
                <p className="mt-1 text-sm text-gray-500">View and submit assignments for your class.</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {assignments.map(a => {
                    const isPastDeadline = new Date(a.deadline) < new Date();
                    return (
                        <div key={a.id} className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 flex flex-col hover:shadow-md transition-shadow relative overflow-hidden group">

                            <div className="flex justify-between items-start mb-4">
                                <span className={`px-2.5 py-1 rounded-md text-xs font-semibold ${isPastDeadline && !a.allowLateSubmission ? 'bg-red-100 text-red-700' : 'bg-emerald-100 text-emerald-700'
                                    }`}>
                                    {a.subjectName}
                                </span>
                            </div>

                            <h3 className="text-lg font-bold text-gray-900 line-clamp-1 mb-1">{a.title}</h3>
                            <p className="text-sm text-gray-500 line-clamp-2 mb-4 flex-1">
                                {a.description}
                            </p>

                            <div className="space-y-2 mb-4">
                                <div className="text-sm text-gray-600"> Teacher: <span className="font-medium text-gray-900">{a.createdByTeacherName}</span></div>
                                <div className="text-sm text-gray-600"> Max Marks: <span className="font-medium text-gray-900">{a.maxMarks}</span></div>
                            </div>

                            <div className="pt-4 border-t border-gray-50 mt-auto flex justify-between items-center text-sm">
                                <div className={`flex items-center font-medium ${isPastDeadline ? 'text-red-500' : 'text-gray-500'}`}>
                                    <Clock className="w-4 h-4 mr-1.5" />
                                    {new Date(a.deadline).toLocaleDateString()}
                                </div>
                                <Link href={`/student/assignments/${a.id}`} className="font-semibold text-indigo-600 hover:text-indigo-800 transition-colors bg-indigo-50 px-3 py-1.5 rounded-lg border border-indigo-100">
                                    View Details
                                </Link>
                            </div>
                        </div>
                    );
                })}

                {assignments.length === 0 && (
                    <div className="col-span-full py-16 flex flex-col items-center justify-center text-gray-500 bg-white rounded-2xl border border-dashed border-gray-300">
                        <Book className="w-12 h-12 text-gray-300 mb-4" />
                        <p className="text-lg font-medium">No published assignments.</p>
                        <p className="text-sm">Enjoy your free time!</p>
                    </div>
                )}
            </div>
        </div>
    );
}
