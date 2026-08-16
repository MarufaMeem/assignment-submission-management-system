"use client";

import React, { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import { ArrowLeft, Clock, Save, Edit, CheckCircle } from 'lucide-react';
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';

export default function StudentSubmissionPage() {
    const { id } = useParams();
    const router = useRouter();

    const [assignment, setAssignment] = useState<any>(null);
    const [submission, setSubmission] = useState<any>(null);

    const [answerText, setAnswerText] = useState('');
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState('');

    useEffect(() => {
        const load = async () => {
            try {
                // parallel load assignment and submission
                const assignmentData = await fetchApi(`/students/assignments/${id}`);
                setAssignment(assignmentData);

                try {
                    const submissionData = await fetchApi(`/students/assignments/${id}/submissions/my`);
                    setSubmission(submissionData);
                    setAnswerText(submissionData.answerText);
                } catch (err: any) {
                    if (!err.message.includes("404")) {
                        throw err;
                    }
                }
            } catch (err: any) {
                setError("Failed to load: " + err.message);
            } finally {
                setLoading(false);
            }
        };
        if (id) load();
    }, [id]);

    const handleSave = async () => {
        setSaving(true);
        setError('');
        try {
            if (submission) {
                // Update
                const res = await fetchApi(`/students/assignments/${id}/submissions/${submission.id}`, {
                    method: 'PUT',
                    body: JSON.stringify({ answerText })
                });
                setSubmission(res);
            } else {
                // Create
                const res = await fetchApi(`/students/assignments/${id}/submissions`, {
                    method: 'POST',
                    body: JSON.stringify({ answerText })
                });
                setSubmission(res);
            }
        } catch (err: any) {
            setError(err.message);
        } finally {
            setSaving(false);
        }
    };

    if (loading) return <div className="p-12 flex justify-center"><div className="w-8 h-8 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin"></div></div>;
    if (error && !assignment) return <div className="text-red-500">{error}</div>;

    const isPastDeadline = assignment ? new Date(assignment.deadline) < new Date() : false;
    const isReviewed = submission?.status === 'Reviewed';
    const isReadOnly = isReviewed || (isPastDeadline && !assignment.allowLateSubmission);

    return (
        <div className="space-y-6 max-w-4xl mx-auto">
            <Link href="/student/assignments" className="inline-flex items-center text-sm font-medium text-gray-500 hover:text-gray-900 mb-2">
                <ArrowLeft className="w-4 h-4 mr-1" /> Back to Assignments
            </Link>

            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                <div className="p-6 sm:p-8 bg-gradient-to-br from-indigo-50 to-white border-b border-gray-100">
                    <h1 className="text-2xl font-bold text-gray-900 mb-2">{assignment.title}</h1>
                    <div className="flex flex-wrap gap-4 text-sm text-gray-600 mt-4">
                        <div className="bg-white px-3 py-1 rounded-md shadow-sm border border-gray-100 font-semibold">{assignment.subjectName}</div>
                        <div className="flex items-center text-rose-600 font-medium">
                            <Clock className="w-4 h-4 mr-1.5" />
                            {new Date(assignment.deadline).toLocaleString()}
                        </div>
                    </div>
                </div>
                <div className="p-6 sm:p-8">
                    <h3 className="font-semibold text-gray-900 mb-2">Description</h3>
                    <p className="text-gray-700 whitespace-pre-wrap">{assignment.description}</p>
                </div>
            </div>

            {error && <div className="p-4 bg-red-50 text-red-700 rounded-xl">{error}</div>}

            {isReviewed && (
                <div className="bg-emerald-50 rounded-2xl p-6 sm:p-8 border border-emerald-100">
                    <div className="flex items-center space-x-3 mb-4">
                        <CheckCircle className="w-6 h-6 text-emerald-600" />
                        <h3 className="text-lg font-bold text-emerald-900">Graded by {assignment.createdByTeacherName}</h3>
                    </div>
                    <div className="flex items-end gap-2 mb-4">
                        <span className="text-4xl font-extrabold text-emerald-700">{submission.marks}</span>
                        <span className="text-emerald-600 font-medium mb-1">/ {assignment.maxMarks}</span>
                    </div>
                    <div>
                        <p className="text-sm font-semibold text-emerald-800 uppercase tracking-wider mb-1">Feedback</p>
                        <p className="text-emerald-900">{submission.feedback || 'No written feedback provided.'}</p>
                    </div>
                </div>
            )}

            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden flex flex-col">
                <div className="p-6 border-b border-gray-50 flex justify-between items-center">
                    <h2 className="text-lg font-bold text-gray-900">Your Submission</h2>
                    <span className="text-xs font-semibold text-gray-500 uppercase">{submission ? (isReviewed ? 'Locked' : 'Draft saved') : 'Not submitted'}</span>
                </div>
                <div className="p-6">
                    <textarea
                        value={answerText}
                        onChange={(e) => setAnswerText(e.target.value)}
                        disabled={isReadOnly}
                        placeholder="Type your answer here..."
                        className="w-full h-48 p-4 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 resize-y transition-colors disabled:bg-gray-50 disabled:text-gray-600"
                    />
                </div>
                {!isReadOnly && (
                    <div className="p-6 border-t border-gray-50 bg-gray-50/50 flex justify-end">
                        <button
                            onClick={handleSave}
                            disabled={saving || !answerText.trim()}
                            className="flex items-center space-x-2 bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 text-white px-6 py-2.5 rounded-xl transition-colors font-medium shadow-sm shadow-indigo-200"
                        >
                            {saving ? (
                                <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                            ) : (
                                submission ? <><Edit className="w-5 h-5" /><span>Update Answer</span></> : <><Save className="w-5 h-5" /><span>Submit Answer</span></>
                            )}
                        </button>
                    </div>
                )}
            </div>

        </div>
    );
}
