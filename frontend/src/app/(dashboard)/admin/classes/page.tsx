"use client";

import React, { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import { Book, Columns, Plus, X } from 'lucide-react';

interface ClassDto { id: number; name: string; description: string; }
interface SubjectDto { id: number; name: string; code: string; }

export default function ClassesAndSubjectsPage() {
    const [classes, setClasses] = useState<ClassDto[]>([]);
    const [subjects, setSubjects] = useState<SubjectDto[]>([]);
    const [loading, setLoading] = useState(true);

    const [isCreateClassOpen, setIsCreateClassOpen] = useState(false);
    const [classForm, setClassForm] = useState({ name: '', description: '' });

    const [isCreateSubjOpen, setIsCreateSubjOpen] = useState(false);
    const [subjForm, setSubjForm] = useState({ name: '', code: '' });

    useEffect(() => {
        loadAll();
    }, []);

    const loadAll = async () => {
        try {
            setLoading(true);
            const [classesData, subjectsData] = await Promise.all([
                fetchApi('/classes'),
                fetchApi('/subjects')
            ]);
            setClasses(classesData);
            setSubjects(subjectsData);
        } catch (err: any) {
            console.error("Failed to fetch classes/subjects", err);
        } finally {
            setLoading(false);
        }
    };

    const handleCreateClass = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await fetchApi('/classes', { method: 'POST', body: JSON.stringify(classForm) });
            setIsCreateClassOpen(false);
            setClassForm({ name: '', description: '' });
            loadAll(); // reload
        } catch (err: any) { alert(err.message); }
    };

    const handleCreateSubject = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await fetchApi('/subjects', { method: 'POST', body: JSON.stringify(subjForm) });
            setIsCreateSubjOpen(false);
            setSubjForm({ name: '', code: '' });
            loadAll(); // reload
        } catch (err: any) { alert(err.message); }
    };

    if (loading && classes.length === 0) {
        return <div className="p-12 flex justify-center"><div className="w-8 h-8 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin"></div></div>;
    }

    return (
        <div className="space-y-8 relative">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">Curriculum Management</h1>
                <p className="mt-1 text-sm text-gray-500">Manage school classes and subjects.</p>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                {/* Classes Panel */}
                <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden flex flex-col">
                    <div className="p-6 border-b border-gray-100 bg-gray-50/50 flex justify-between items-center">
                        <div className="flex items-center space-x-3">
                            <div className="p-2 bg-blue-100 rounded-lg text-blue-600">
                                <Columns className="w-5 h-5" />
                            </div>
                            <h2 className="text-lg font-bold text-gray-900">Classes</h2>
                        </div>
                        <button onClick={() => setIsCreateClassOpen(true)} className="p-2 text-indigo-600 hover:bg-indigo-50 rounded-lg">
                            <Plus className="w-5 h-5" />
                        </button>
                    </div>
                    <div className="p-6 flex-1">
                        <ul className="space-y-3">
                            {classes.map((cls) => (
                                <li key={cls.id} className="flex items-center justify-between p-4 rounded-xl border border-gray-100 hover:border-indigo-200 hover:bg-indigo-50/30 transition-colors">
                                    <div className="space-y-1">
                                        <p className="font-medium text-gray-900">{cls.name}</p>
                                        <p className="text-sm text-gray-500 line-clamp-1">{cls.description}</p>
                                    </div>
                                    <span className="text-xs font-semibold text-blue-600 bg-blue-50 px-2.5 py-1 rounded-md ml-4">ID: {cls.id}</span>
                                </li>
                            ))}
                            {classes.length === 0 && <li className="text-sm text-gray-500 text-center py-4">No classes found.</li>}
                        </ul>
                    </div>
                </div>

                {/* Subjects Panel */}
                <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden flex flex-col">
                    <div className="p-6 border-b border-gray-100 bg-gray-50/50 flex justify-between items-center">
                        <div className="flex items-center space-x-3">
                            <div className="p-2 bg-purple-100 rounded-lg text-purple-600">
                                <Book className="w-5 h-5" />
                            </div>
                            <h2 className="text-lg font-bold text-gray-900">Subjects</h2>
                        </div>
                        <button onClick={() => setIsCreateSubjOpen(true)} className="p-2 text-indigo-600 hover:bg-indigo-50 rounded-lg">
                            <Plus className="w-5 h-5" />
                        </button>
                    </div>
                    <div className="p-6 flex-1">
                        <ul className="space-y-3">
                            {subjects.map((sub) => (
                                <li key={sub.id} className="flex items-center justify-between p-4 rounded-xl border border-gray-100 hover:border-indigo-200 hover:bg-indigo-50/30 transition-colors">
                                    <div className="space-y-1">
                                        <p className="font-medium text-gray-900">{sub.name}</p>
                                    </div>
                                    <span className="text-xs font-semibold text-purple-600 bg-purple-50 px-2.5 py-1 rounded-md ml-4">Code: {sub.code} (ID: {sub.id})</span>
                                </li>
                            ))}
                            {subjects.length === 0 && <li className="text-sm text-gray-500 text-center py-4">No subjects found.</li>}
                        </ul>
                    </div>
                </div>
            </div>

            {/* Create Class Modal */}
            {isCreateClassOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-gray-900/50 backdrop-blur-sm p-4 animate-in fade-in">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
                        <div className="p-6 border-b border-gray-100 flex justify-between items-center">
                            <h2 className="text-xl font-bold">New Class</h2>
                            <button onClick={() => setIsCreateClassOpen(false)}><X className="text-gray-400 hover:text-gray-600" /></button>
                        </div>
                        <form onSubmit={handleCreateClass} className="p-6 space-y-4">
                            <div>
                                <label className="block text-sm font-semibold mb-1">Class Name</label>
                                <input required value={classForm.name} onChange={e => setClassForm({ ...classForm, name: e.target.value })} className="w-full px-4 py-2 border rounded-xl" placeholder="e.g. Grade 12" />
                            </div>
                            <div>
                                <label className="block text-sm font-semibold mb-1">Description</label>
                                <input value={classForm.description} onChange={e => setClassForm({ ...classForm, description: e.target.value })} className="w-full px-4 py-2 border rounded-xl" placeholder="e.g. Science Batch" />
                            </div>
                            <button type="submit" className="w-full py-2.5 bg-indigo-600 text-white rounded-xl">Create</button>
                        </form>
                    </div>
                </div>
            )}

            {/* Create Subject Modal */}
            {isCreateSubjOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-gray-900/50 backdrop-blur-sm p-4 animate-in fade-in">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
                        <div className="p-6 border-b border-gray-100 flex justify-between items-center">
                            <h2 className="text-xl font-bold">New Subject</h2>
                            <button onClick={() => setIsCreateSubjOpen(false)}><X className="text-gray-400 hover:text-gray-600" /></button>
                        </div>
                        <form onSubmit={handleCreateSubject} className="p-6 space-y-4">
                            <div>
                                <label className="block text-sm font-semibold mb-1">Subject Name</label>
                                <input required value={subjForm.name} onChange={e => setSubjForm({ ...subjForm, name: e.target.value })} className="w-full px-4 py-2 border rounded-xl" placeholder="e.g. Biology" />
                            </div>
                            <div>
                                <label className="block text-sm font-semibold mb-1">Subject Code</label>
                                <input required value={subjForm.code} onChange={e => setSubjForm({ ...subjForm, code: e.target.value })} className="w-full px-4 py-2 border rounded-xl" placeholder="e.g. BIO201" />
                            </div>
                            <button type="submit" className="w-full py-2.5 bg-indigo-600 text-white rounded-xl">Create</button>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}
