"use client";

import React, { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import { GraduationCap, Link as LinkIcon, Plus } from 'lucide-react';

interface TeacherAssignmentDto {
    id: number;
    teacherId: number;
    teacherName: string;
    classId: number;
    className: string;
    subjectId: number;
    subjectName: string;
}

export default function TeacherAssignmentsPage() {
    const [assignments, setAssignments] = useState<TeacherAssignmentDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadAssignments = async () => {
            try {
                // Using the specific endpoint for fetching all teacher assignments
                const data = await fetchApi('/teacherAssignments');
                setAssignments(data);
            } catch (err: any) {
                console.error("Failed to fetch teacher assignments", err);
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
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Teacher Assignments</h1>
                    <p className="mt-1 text-sm text-gray-500">Map teachers to classes and subjects.</p>
                </div>
                <button className="flex items-center space-x-2 bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-xl transition-colors shadow-sm">
                    <Plus className="w-5 h-5" />
                    <span>Assign Teacher</span>
                </button>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Teacher</th>
                                <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Class</th>
                                <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Subject</th>
                                <th className="px-6 py-3 text-right text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {assignments.map((assignment) => (
                                <tr key={assignment.id} className="hover:bg-gray-50/50 transition-colors">
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <div className="flex items-center space-x-3">
                                            <div className="bg-emerald-100 text-emerald-600 p-2 rounded-lg">
                                                <GraduationCap className="w-4 h-4" />
                                            </div>
                                            <div className="text-sm font-medium text-gray-900">{assignment.teacherName}</div>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700">
                                        {assignment.className}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700">
                                        {assignment.subjectName}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                        <button className="text-red-600 hover:text-red-900">
                                            Remove
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            {assignments.length === 0 && (
                                <tr>
                                    <td colSpan={4} className="px-6 py-12 text-center text-gray-500">
                                        <div className="flex flex-col items-center">
                                            <LinkIcon className="w-10 h-10 text-gray-300 mb-3" />
                                            <p>No teacher assignments found</p>
                                        </div>
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
}
