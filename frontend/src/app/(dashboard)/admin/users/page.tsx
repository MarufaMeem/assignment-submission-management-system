"use client";

import React, { useEffect, useState } from 'react';
import { fetchApi } from '@/lib/api';
import { Plus, Search, X } from 'lucide-react';

interface UserDto {
    id: number;
    fullName: string;
    email: string;
    role: string;
    isActive: boolean;
}

export default function UsersPage() {
    const [users, setUsers] = useState<UserDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [searchTerm, setSearchTerm] = useState('');

    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const [createForm, setCreateForm] = useState({ fullName: '', email: '', password: '', role: 'Teacher', classId: '' });
    const [creating, setCreating] = useState(false);

    useEffect(() => {
        loadUsers();
    }, []);

    const loadUsers = async () => {
        try {
            setLoading(true);
            const data = await fetchApi('/users');
            setUsers(data);
        } catch (err: any) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    const toggleStatus = async (userId: number, currentStatus: boolean) => {
        try {
            await fetchApi(`/users/${userId}/status`, {
                method: 'PUT',
                body: JSON.stringify({ isActive: !currentStatus })
            });
            await loadUsers(); // reload after change
        } catch (err: any) {
            alert("Failed to change status: " + err.message);
        }
    };

    const handleCreateSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setCreating(true);
        try {
            const body = {
                fullName: createForm.fullName,
                email: createForm.email,
                password: createForm.password,
                role: createForm.role,
                ...(createForm.role === 'Student' && createForm.classId ? { classId: parseInt(createForm.classId) } : {})
            };

            await fetchApi('/users', {
                method: 'POST',
                body: JSON.stringify(body)
            });
            await loadUsers();
            setIsCreateOpen(false);
            setCreateForm({ fullName: '', email: '', password: '', role: 'Teacher', classId: '' });
        } catch (err: any) {
            alert("Failed to create user: " + err.message);
        } finally {
            setCreating(false);
        }
    };

    const filteredUsers = users.filter(u =>
        u.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        u.email.toLowerCase().includes(searchTerm.toLowerCase())
    );

    return (
        <div className="space-y-6 relative">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">User Management</h1>
                    <p className="mt-1 text-sm text-gray-500">Manage all users across the platform.</p>
                </div>
                <button
                    onClick={() => setIsCreateOpen(true)}
                    className="flex items-center space-x-2 bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-xl transition-colors shadow-sm"
                >
                    <Plus className="w-5 h-5" />
                    <span>Create User</span>
                </button>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                <div className="p-4 border-b border-gray-100 bg-gray-50/50">
                    <div className="relative max-w-sm">
                        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <Search className="h-5 w-5 text-gray-400" />
                        </div>
                        <input
                            type="text"
                            className="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-xl leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm transition-colors"
                            placeholder="Search users..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>
                </div>

                {error ? (
                    <div className="p-6 text-center text-red-600">{error}</div>
                ) : loading ? (
                    <div className="p-12 flex justify-center">
                        <div className="w-8 h-8 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin"></div>
                    </div>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                                <tr>
                                    <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Name</th>
                                    <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Role</th>
                                    <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Status</th>
                                    <th className="px-6 py-3 text-right text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
                                </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                                {filteredUsers.map((user) => (
                                    <tr key={user.id} className="hover:bg-gray-50/50 transition-colors">
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <div className="flex items-center">
                                                <div className="h-10 w-10 flex-shrink-0">
                                                    <div className="h-10 w-10 rounded-full bg-gradient-to-tr from-indigo-100 to-purple-100 flex items-center justify-center text-indigo-700 font-bold">
                                                        {user.fullName.charAt(0).toUpperCase()}
                                                    </div>
                                                </div>
                                                <div className="ml-4">
                                                    <div className="text-sm font-medium text-gray-900">{user.fullName}</div>
                                                    <div className="text-sm text-gray-500">{user.email}</div>
                                                </div>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <span className={`px-3 py-1 inline-flex text-xs leading-5 font-semibold rounded-full ${user.role === 'Admin' ? 'bg-purple-100 text-purple-800' :
                                                user.role === 'Teacher' ? 'bg-blue-100 text-blue-800' :
                                                    'bg-green-100 text-green-800'
                                                }`}>
                                                {user.role}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <span className={`inline-flex items-center space-x-1.5 ${user.isActive ? 'text-green-600' : 'text-red-500'}`}>
                                                <span className={`h-2 w-2 rounded-full ${user.isActive ? 'bg-green-500' : 'bg-red-500'}`}></span>
                                                <span className="text-sm font-medium">{user.isActive ? 'Active' : 'Inactive'}</span>
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                            <button
                                                onClick={() => toggleStatus(user.id, user.isActive)}
                                                className={`text-${user.isActive ? 'red' : 'green'}-600 hover:text-${user.isActive ? 'red' : 'green'}-900`}
                                            >
                                                {user.isActive ? 'Deactivate' : 'Activate'}
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                                {filteredUsers.length === 0 && (
                                    <tr>
                                        <td colSpan={4} className="px-6 py-12 text-center text-gray-500">
                                            No users found matching "{searchTerm}"
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>

            {/* Create User Modal */}
            {isCreateOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-gray-900/50 backdrop-blur-sm p-4 animate-in fade-in duration-200">
                    <div className="bg-white rounded-3xl shadow-xl w-full max-w-md overflow-hidden flex flex-col">
                        <div className="p-6 border-b border-gray-100 flex justify-between items-center bg-gray-50/50">
                            <h2 className="text-xl font-bold text-gray-900">Create New User</h2>
                            <button onClick={() => setIsCreateOpen(false)} className="text-gray-400 hover:text-gray-600 p-2 bg-white rounded-full shadow-sm hover:shadow transition-all">
                                <X className="w-5 h-5" />
                            </button>
                        </div>

                        <form onSubmit={handleCreateSubmit} className="p-6 overflow-y-auto space-y-4">
                            <div>
                                <label className="block text-sm font-semibold text-gray-700 mb-1">Full Name</label>
                                <input required type="text" value={createForm.fullName} onChange={e => setCreateForm({ ...createForm, fullName: e.target.value })} className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500" placeholder="John Doe" />
                            </div>
                            <div>
                                <label className="block text-sm font-semibold text-gray-700 mb-1">Email</label>
                                <input required type="email" value={createForm.email} onChange={e => setCreateForm({ ...createForm, email: e.target.value })} className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500" placeholder="john@school.edu" />
                            </div>
                            <div>
                                <label className="block text-sm font-semibold text-gray-700 mb-1">Password</label>
                                <input required type="password" minLength={6} value={createForm.password} onChange={e => setCreateForm({ ...createForm, password: e.target.value })} className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500" />
                            </div>
                            <div>
                                <label className="block text-sm font-semibold text-gray-700 mb-1">Role</label>
                                <select value={createForm.role} onChange={e => setCreateForm({ ...createForm, role: e.target.value })} className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 bg-white">
                                    <option value="Teacher">Teacher</option>
                                    <option value="Student">Student</option>
                                    <option value="Admin">Admin</option>
                                </select>
                            </div>
                            {createForm.role === 'Student' && (
                                <div>
                                    <label className="block text-sm font-semibold text-gray-700 mb-1">Class ID</label>
                                    <input required type="number" min="1" value={createForm.classId} onChange={e => setCreateForm({ ...createForm, classId: e.target.value })} className="w-full px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500" placeholder="e.g. 1" />
                                </div>
                            )}

                            <div className="pt-4 border-t border-gray-100 flex justify-end">
                                <button type="submit" disabled={creating} className="px-6 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white font-medium rounded-xl transition-colors disabled:opacity-50 flex items-center">
                                    {creating ? 'Creating...' : 'Create User'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}
