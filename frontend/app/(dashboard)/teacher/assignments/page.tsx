"use client";

import React, { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import { Plus, Check, Clock, TrendingUp, MoreVertical } from 'lucide-react';
import Link from 'next/link';

interface AssignmentDto {
    id: number;
    title: string;
    description: string;
    classId: number;
    subjectId: number;
    deadline: string;
    maxMarks: number;
    allowLateSubmission: boolean;
    status: number;
}

export default function TeacherAssignmentsPage() {
    const [assignments, setAssignments] = useState<AssignmentDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadAssignments = async () => {
            try {
                const data = await fetchApi('/assignments');
                setAssignments(data);
            } catch (err) {
                console.error("Failed to load assignments", err);
            } finally {
                setLoading(false);
            }
        };
        loadAssignments();
    }, []);

    const publishAssignment = async (id: number) => {
        try {
            await fetchApi(`/assignments/${id}/publish`, { method: 'POST' });
            setAssignments(assignments.map(a => a.id === id ? { ...a, status: 1 } : a)); // 1 for Published
        } catch (err: any) {
            alert(err.message);
        }
    };

    if (loading) {
        return (
            <div className="p-12 flex justify-center">
                <div className="w-8 h-8 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin"></div>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">My Assignments</h1>
                    <p className="mt-1 text-sm text-gray-500">Create and manage assignments for your assigned classes.</p>
                </div>
                <button className="flex items-center space-x-2 bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-xl transition-colors shadow-sm">
                    <Plus className="w-5 h-5" />
                    <span>Create Assignment</span>
                </button>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
                {assignments.map(a => (
                    <div key={a.id} className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 flex flex-col hover:shadow-md transition-shadow relative overflow-hidden group">

                        <div className="flex justify-between items-start mb-4">
                            <span className={`px-2.5 py-1 rounded-md text-xs font-semibold uppercase tracking-wider ${a.status === 0 ? 'bg-amber-100 text-amber-700' : 'bg-emerald-100 text-emerald-700'
                                }`}>
                                {a.status === 0 ? 'Draft' : 'Published'}
                            </span>
                            <div className="flex space-x-2">
                                {a.status === 0 && (
                                    <button
                                        onClick={() => publishAssignment(a.id)}
                                        className="p-1.5 bg-gray-50 hover:bg-indigo-50 text-indigo-600 rounded-lg transition-colors border border-gray-100"
                                        title="Publish"
                                    >
                                        <TrendingUp className="w-4 h-4" />
                                    </button>
                                )}
                                <button className="p-1.5 bg-gray-50 hover:bg-gray-100 text-gray-600 rounded-lg transition-colors border border-gray-100">
                                    <MoreVertical className="w-4 h-4" />
                                </button>
                            </div>
                        </div>

                        <h3 className="text-lg font-bold text-gray-900 line-clamp-1 mb-1">{a.title}</h3>
                        <p className="text-sm text-gray-500 line-clamp-2 mb-4 flex-1">
                            {a.description}
                        </p>

                        <div className="pt-4 border-t border-gray-50 mt-auto flex justify-between items-center text-sm text-gray-500">
                            <div className="flex items-center text-rose-600 font-medium">
                                <Clock className="w-4 h-4 mr-1.5" />
                                {new Date(a.deadline).toLocaleDateString()}
                            </div>
                            <Link href={`/teacher/assignments/${a.id}/submissions`} className="font-semibold text-indigo-600 hover:text-indigo-800 transition-colors">
                                View Submissions &rarr;
                            </Link>
                        </div>
                    </div>
                ))}

                {assignments.length === 0 && (
                    <div className="col-span-full py-16 flex flex-col items-center justify-center text-gray-500 bg-white rounded-2xl border border-dashed border-gray-300">
                        <TrendingUp className="w-12 h-12 text-gray-300 mb-4" />
                        <p className="text-lg font-medium">No assignments yet</p>
                        <p className="text-sm">Click "Create Assignment" to get started.</p>
                    </div>
                )}
            </div>
        </div>
    );
}
