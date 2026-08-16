"use client";

import React, { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import { Plus, Check, Clock, TrendingUp, MoreVertical, X } from 'lucide-react';
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
    status: string;
}

export default function TeacherAssignmentsPage() {
    const [assignments, setAssignments] = useState<AssignmentDto[]>([]);
    const [loading, setLoading] = useState(true);

    // Modal states
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const [createForm, setCreateForm] = useState({
        title: '',
        description: '',
        classId: 1,
        subjectId: 1,
        deadline: '',
        maxMarks: 100,
        allowLateSubmission: false
    });
    const [creating, setCreating] = useState(false);

    useEffect(() => {
        loadAssignments();
    }, []);

    const loadAssignments = async () => {
        try {
            setLoading(true);
            const data = await fetchApi('/assignments');
            setAssignments(data);
        } catch (err) {
            console.error("Failed to load assignments", err);
        } finally {
            setLoading(false);
        }
    };

    const publishAssignment = async (id: number) => {
        try {
            await fetchApi(`/assignments/${id}/publish`, { method: 'POST' });
            setAssignments(assignments.map(a => a.id === id ? { ...a, status: 'Published' } : a));
        } catch (err: any) {
            alert(err.message);
        }
    };

    const handleCreateSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setCreating(true);
        try {
            const result = await fetchApi('/assignments', {
                method: 'POST',
                body: JSON.stringify({
                    ...createForm,
                    deadline: new Date(createForm.deadline).toISOString()
                })
            });
            // Add to list instantly
            setAssignments([...assignments, result]);
            setIsCreateOpen(false); // Close modal
            // Reset form
            setCreateForm({
                title: '',
                description: '',
                classId: 1,
                subjectId: 1,
                deadline: '',
                maxMarks: 100,
                allowLateSubmission: false
            });
        } catch (err: any) {
            alert("Failed to create assignment: " + err.message);
        } finally {
            setCreating(false);
        }
    };

    if (loading && assignments.length === 0) {
        return (
            <div className="p-12 flex justify-center">
                <div className="w-8 h-8 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin"></div>
            </div>
        );
    }

    return (
        <div className="space-y-6 relative">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">My Assignments</h1>
                    <p className="mt-1 text-sm text-gray-500">Create and manage assignments for your assigned classes.</p>
                </div>
                <button
                    onClick={() => setIsCreateOpen(true)}
                    className="flex items-center space-x-2 bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-xl transition-colors shadow-sm"
                >
                    <Plus className="w-5 h-5" />
                    <span>Create Assignment</span>
                </button>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
                {assignments.map(a => (
                    <div key={a.id} className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 flex flex-col hover:shadow-md transition-shadow relative overflow-hidden group">
                        <div className="flex justify-between items-start mb-4">
                            <span className={`px-2.5 py-1 rounded-md text-xs font-semibold uppercase tracking-wider ${a.status === 'Draft' ? 'bg-amber-100 text-amber-700' : 'bg-emerald-100 text-emerald-700'}`}>
                                {a.status === 'Draft' ? 'Draft' : 'Published'}
                            </span>
                            <div className="flex space-x-2">
                                {a.status === 'Draft' && (
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

            {/* Create Assignment Modal Overlay */}
            {isCreateOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-gray-900/50 backdrop-blur-sm p-4 animate-in fade-in duration-200">
                    <div className="bg-white rounded-3xl shadow-xl w-full max-w-lg overflow-hidden flex flex-col max-h-[90vh]">
                        <div className="p-6 border-b border-gray-100 flex justify-between items-center bg-gray-50/50">
                            <h2 className="text-xl font-bold text-gray-900">Create New Assignment</h2>
                            <button onClick={() => setIsCreateOpen(false)} className="text-gray-400 hover:text-gray-600 p-2 bg-white rounded-full shadow-sm hover:shadow transition-all">
                                <X className="w-5 h-5" />
                            </button>
                        </div>

                        <form onSubmit={handleCreateSubmit} className="p-6 overflow-y-auto space-y-4">
                            <div>
                                <label className="block text-sm font-semibold text-gray-700 mb-1">Title</label>
                                <input required type="text" value={createForm.title} onChange={e => setCreateForm({ ...createForm, title: e.target.value })} className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500" placeholder="e.g. Algebra Chapter 1" />
                            </div>
                            <div>
                                <label className="block text-sm font-semibold text-gray-700 mb-1">Description</label>
                                <textarea required value={createForm.description} onChange={e => setCreateForm({ ...createForm, description: e.target.value })} className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 h-24 resize-none" placeholder="Elaborate on the requirements..."></textarea>
                            </div>

                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-semibold text-gray-700 mb-1">Class ID</label>
                                    <input required type="number" min="1" value={createForm.classId} onChange={e => setCreateForm({ ...createForm, classId: parseInt(e.target.value) })} className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500" />
                                </div>
                                <div>
                                    <label className="block text-sm font-semibold text-gray-700 mb-1">Subject ID</label>
                                    <input required type="number" min="1" value={createForm.subjectId} onChange={e => setCreateForm({ ...createForm, subjectId: parseInt(e.target.value) })} className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500" />
                                </div>
                            </div>

                            <div>
                                <label className="block text-sm font-semibold text-gray-700 mb-1">Deadline</label>
                                <input required type="datetime-local" value={createForm.deadline} onChange={e => setCreateForm({ ...createForm, deadline: e.target.value })} className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500" />
                            </div>

                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-semibold text-gray-700 mb-1">Max Marks</label>
                                    <input required type="number" min="1" value={createForm.maxMarks} onChange={e => setCreateForm({ ...createForm, maxMarks: parseInt(e.target.value) })} className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500" />
                                </div>
                                <div className="flex items-center mt-6 space-x-3">
                                    <input type="checkbox" id="allowLate" checked={createForm.allowLateSubmission} onChange={e => setCreateForm({ ...createForm, allowLateSubmission: e.target.checked })} className="w-5 h-5 text-indigo-600 rounded border-gray-300 focus:ring-indigo-500" />
                                    <label htmlFor="allowLate" className="text-sm font-medium text-gray-700">Allow Late Submission</label>
                                </div>
                            </div>

                            <div className="pt-4 border-t border-gray-100 mt-4 flex justify-end">
                                <button type="submit" disabled={creating} className="px-6 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white font-medium rounded-xl transition-colors shadow-sm disabled:opacity-50 flex items-center">
                                    {creating ? <span className="animate-spin mr-2 w-4 h-4 border-2 border-white border-t-transparent rounded-full"></span> : null}
                                    {creating ? 'Creating...' : 'Create Assignment'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}
