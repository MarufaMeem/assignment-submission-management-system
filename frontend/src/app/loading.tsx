"use client";

export default function Loading() {
    return (
        <div className="flex flex-col items-center justify-center min-h-[60vh] space-y-4">
            <div className="w-12 h-12 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin shadow-lg"></div>
            <p className="text-gray-500 font-medium animate-pulse">Loading content...</p>
        </div>
    );
}
