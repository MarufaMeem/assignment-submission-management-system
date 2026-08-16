"use client";

import React, { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import { Book, Columns, Plus } from 'lucide-react';

interface ClassDto { id: number; name: string; }
interface SubjectDto { id: number; name: string; }

export default function ClassesAndSubjectsPage() {
    const [classes, setClasses] = useState<ClassDto[]>([]);
    const [subjects, setSubjects] = useState<SubjectDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadAll = async () => {
            try {
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
        loadAll();
    }, []);

    if (loading) {
        return (
            <div className="p-12 flex justify-center">
                <div className="w-8 h-8 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin"></div>
            </div>
        );
    }

    return (
        <div className="space-y-8">
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
                        <button className="p-2 text-indigo-600 hover:bg-indigo-50 rounded-lg">
                            <Plus className="w-5 h-5" />
                        </button>
                    </div>
                    <div className="p-6 flex-1">
                        <ul className="space-y-3">
                            {classes.map((cls) => (
                                <li key={cls.id} className="flex items-center justify-between p-4 rounded-xl border border-gray-100 hover:border-indigo-200 hover:bg-indigo-50/30 transition-colors">
                                    <span className="font-medium text-gray-900">{cls.name}</span>
                                    <span className="text-xs font-semibold text-gray-400 bg-gray-100 px-2 py-1 rounded-md">ID: {cls.id}</span>
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
                        <button className="p-2 text-indigo-600 hover:bg-indigo-50 rounded-lg">
                            <Plus className="w-5 h-5" />
                        </button>
                    </div>
                    <div className="p-6 flex-1">
                        <ul className="space-y-3">
                            {subjects.map((sub) => (
                                <li key={sub.id} className="flex items-center justify-between p-4 rounded-xl border border-gray-100 hover:border-indigo-200 hover:bg-indigo-50/30 transition-colors">
                                    <span className="font-medium text-gray-900">{sub.name}</span>
                                    <span className="text-xs font-semibold text-gray-400 bg-gray-100 px-2 py-1 rounded-md">ID: {sub.id}</span>
                                </li>
                            ))}
                            {subjects.length === 0 && <li className="text-sm text-gray-500 text-center py-4">No subjects found.</li>}
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    );
}
