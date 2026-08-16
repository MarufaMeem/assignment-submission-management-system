"use client";

import React from 'react';

// The root page is intrinsically protected by AuthContext inside layout.tsx
// which will handle redirecting the user to /login or their correct role dashboard.
export default function HomePage() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="w-12 h-12 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin"></div>
    </div>
  );
}
