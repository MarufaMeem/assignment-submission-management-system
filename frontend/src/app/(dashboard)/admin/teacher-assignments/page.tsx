"use client";

import React, { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import { GraduationCap, Link as LinkIcon, Plus, X } from 'lucide-react';

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

    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const [form, setForm] = useState({ teacherId: '', classId: '', subjectId: '' });

    useEffect(() => {
        loadAssignments();
    }, []);

    const loadAssignments = async () => {
        try {
            setLoading(true);
            const data = await fetchApi('/teacher-assignments');
            setAssignments(data);
        } catch (err: any) {
            console.error("Failed to fetch teacher assignments", err);
        } finally {
            setLoading(false);
        }
    };

    const handleCreate = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await fetchApi('/teacher-assignments', {
                method: 'POST',
                body: JSON.stringify({
                    teacherId: parseInt(form.teacherId),
                    classId: parseInt(form.classId),
                    subjectId: parseInt(form.subjectId)
                })
            });
            setIsCreateOpen(false);
            setForm({ teacherId: '', classId: '', subjectId: '' });
            loadAssignments();
        } catch (err: any) { alert(err.message); }
    };

    const handleRemove = async (id: number) => {
        if (!confirm("Are you sure you want to remove this mapping?")) return;
        try {
            await fetchApi(`/teacher-assignments/${id}`, { method: 'DELETE' });
            loadAssignments();
        } catch (err: any) { alert(err.message); }
    };

    if (loading && assignments.length === 0) {
        return <div className="p-12 flex justify-center"><div className="w-8 h-8 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin"></div></div>;
    }

    return (
        <div className="space-y-6 relative">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Teacher Assignments</h1>
                    <p className="mt-1 text-sm text-gray-500">Map teachers to classes and subjects.</p>
                </div>
                <button onClick={() => setIsCreateOpen(true)} className="flex items-center space-x-2 bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-xl transition-colors shadow-sm">
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
                                            <div className="space-y-1">
                                                <div className="text-sm font-bold text-gray-900">{assignment.teacherName}</div>
                                                <div className="text-xs text-gray-500">ID: {assignment.teacherId}</div>
                                            </div>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700">
                                        <span className="font-semibold">{assignment.className}</span> <span className="text-gray-400 text-xs ml-1">(ID: {assignment.classId})</span>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700">
                                        <span className="font-semibold">{assignment.subjectName}</span> <span className="text-gray-400 text-xs ml-1">(ID: {assignment.subjectId})</span>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                        <button onClick={() => handleRemove(assignment.id)} className="text-red-500 hover:text-red-700 bg-red-50 px-3 py-1 rounded-md transition-colors hover:bg-red-100">
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

            {/* Create Mapping Modal */}
            {isCreateOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-gray-900/50 backdrop-blur-sm p-4 animate-in fade-in">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm">
                        <div className="p-6 border-b border-gray-100 flex justify-between items-center bg-gray-50">
                            <h2 className="text-xl font-bold">Assign Teacher</h2>
                            <button onClick={() => setIsCreateOpen(false)}><X className="text-gray-400" /></button>
                        </div>
                        <form onSubmit={handleCreate} className="p-6 space-y-4">
                            <div>
                                <label className="block text-sm font-semibold mb-1">Teacher User ID</label>
                                <input required type="number" min="1" value={form.teacherId} onChange={e => setForm({ ...form, teacherId: e.target.value })} className="w-full px-4 py-2 border rounded-xl" />
                            </div>
                            <div>
                                <label className="block text-sm font-semibold mb-1">Class ID</label>
                                <input required type="number" min="1" value={form.classId} onChange={e => setForm({ ...form, classId: e.target.value })} className="w-full px-4 py-2 border rounded-xl" />
                            </div>
                            <div>
                                <label className="block text-sm font-semibold mb-1">Subject ID</label>
                                <input required type="number" min="1" value={form.subjectId} onChange={e => setForm({ ...form, subjectId: e.target.value })} className="w-full px-4 py-2 border rounded-xl" />
                            </div>
                            <button type="submit" className="w-full py-2.5 bg-indigo-600 text-white rounded-xl font-medium pt-3">Create Mapping</button>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}
