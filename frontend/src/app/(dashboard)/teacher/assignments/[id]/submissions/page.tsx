"use client";

import React, { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import { CheckCircle, MessageSquare, Edit3 } from 'lucide-react';
import { useParams } from 'next/navigation';

export default function SubmissionsPage() {
    const { id } = useParams();
    const [submissions, setSubmissions] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadSubmissions = async () => {
            try {
                const data = await fetchApi(`/assignments/${id}/submissions`);
                setSubmissions(data);
            } catch (err) {
                console.error(err);
            } finally {
                setLoading(false);
            }
        };
        if (id) loadSubmissions();
    }, [id]);

    const reviewSubmission = async (subId: number, marks: number, feedback: string) => {
        try {
            await fetchApi(`/assignments/${id}/submissions/${subId}/review`, {
                method: 'POST',
                body: JSON.stringify({ marks, feedback })
            });
            // Update local state logically
            setSubmissions(submissions.map(s => s.id === subId ? { ...s, status: "Reviewed", marks, feedback } : s));
        } catch (err: any) {
            alert(err.message);
        }
    };

    if (loading) return <div className="animate-pulse p-10 bg-gray-100 rounded-xl" />;

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">Submissions Review</h1>
                <p className="mt-1 text-sm text-gray-500">Review student answers for assignment #{id}</p>
            </div>

            <div className="grid grid-cols-1 gap-6">
                {submissions.map((s) => (
                    <div key={s.id} className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 flex flex-col sm:flex-row sm:items-start sm:justify-between gap-6">
                        <div className="flex-1 space-y-4">
                            <div className="flex items-center justify-between">
                                <h3 className="font-bold text-gray-900">{s.studentName}</h3>
                                <span className={`px-2.5 py-1 rounded-md text-xs font-semibold ${s.status === 'Reviewed' ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-100 text-amber-700'
                                    }`}>
                                    {s.status}
                                </span>
                            </div>
                            <div className="bg-gray-50 rounded-xl p-4 text-sm text-gray-700 border border-gray-100 font-mono whitespace-pre-wrap">
                                {s.answerText}
                            </div>

                            {s.status === 'Reviewed' ? (
                                <div className="mt-4 bg-emerald-50/50 p-4 rounded-xl border border-emerald-100 flex items-start space-x-3">
                                    <CheckCircle className="w-5 h-5 text-emerald-600 shrink-0 mt-0.5" />
                                    <div>
                                        <p className="font-bold text-emerald-900">Marks: {s.marks}</p>
                                        <p className="text-sm text-emerald-700 mt-1">{s.feedback}</p>
                                    </div>
                                </div>
                            ) : (
                                <div className="flex items-end gap-3 mt-4 pt-4 border-t border-gray-50">
                                    <button onClick={() => {
                                        const marks = prompt("Enter marks:");
                                        const feedback = prompt("Enter feedback:");
                                        if (marks !== null) reviewSubmission(s.id, Number(marks), feedback || "");
                                    }} className="flex items-center px-4 py-2 bg-indigo-50 text-indigo-700 hover:bg-indigo-100 font-medium text-sm rounded-xl transition-colors">
                                        <Edit3 className="w-4 h-4 mr-2" />
                                        Review Now
                                    </button>
                                </div>
                            )}
                        </div>
                    </div>
                ))}

                {submissions.length === 0 && (
                    <div className="py-16 flex flex-col items-center justify-center text-gray-500 bg-white rounded-2xl border border-dashed border-gray-300">
                        <MessageSquare className="w-12 h-12 text-gray-300 mb-4" />
                        <p className="text-lg font-medium">No submissions yet.</p>
                    </div>
                )}
            </div>
        </div>
    );
}
