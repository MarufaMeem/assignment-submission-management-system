# EduTrack: Enterprise Assignment Management SaaS

[![.NET Core](https://img.shields.io/badge/.NET_Core-8.0-512BD4?logo=dotnet)](#)
[![Next.js](https://img.shields.io/badge/Next.js-14-black?logo=next.js)](#)
[![TypeScript](https://img.shields.io/badge/TypeScript-007ACC?logo=typescript)](#)
[![xUnit](https://img.shields.io/badge/Testing-xUnit_%7C_Moq-brightgreen)](#)

> **Note to the Hiring Team:**
> I developed this project explicitly to demonstrate my readiness for the **Assistant Software Engineer (.NET)** role. It is a full-stack, scalable application built directly in alignment with your technical stack: robust backend APIs in **C# & ASP.NET Core**, frontend interfaces using **Next.js, React, & TypeScript**, and rigorous unit testing using **Moq & xUnit**. I have poured my passion for writing clean, reusable, and maintainable code into this product.
>
> 🎥 **Project Demo Video:** 
> 
> [🎬 Click here to watch the full Project Demo Video](./assignment_submission_project.mp4)

---

## � Alignment with Job Requirements

I engineered EduTrack to serve as a comprehensive proof-of-concept for the skills required in your product engineering team:

- **Scalable REST APIs:** Developed highly decoupled ASP.NET Core Web APIs using N-Tier architecture (Controllers, Services, DTOs).
- **Modern Web Interfaces:** Built a fully responsive, user-friendly frontend utilizing Next.js, React hooks, TypeScript, and modern CSS practices to seamlessly integrate with backend endpoints.
- **Authentication & Security:** Implemented rigorous JWT-based authentication, BCrypt password hashing, and Role-Based Access Control (Admin, Teacher, Student).
- **Database Design & EF Core:** Designed normalized relational database tables and relationships using Entity Framework Core, relying heavily on standard LINQ queries and distinct data access layers.
- **Test-Driven Reliability:** Authored comprehensive unit tests for all critical business logic using **xUnit** and **Moq** to ensure production-level stability.
- **Code Quality:** Strictly adhered to object-oriented principles, dependency injection, clear Git workflows, and clean coding standards prioritizing maintainability.

---

## 🛠️ Technology Stack Implemented

### **Backend (Modular API)**
*   **Framework:** C# / ASP.NET Core 8 Web API
*   **ORM:** Entity Framework Core (Code-First Approach)
*   **Database:** Configured for seamless PostgreSQL / SQL integration
*   **Testing:** xUnit alongside Moq mocking frameworks
*   **Security:** JSON Web Tokens (JWT) & Authorization Middleware

### **Frontend (Modern Web)**
*   **Framework:** Next.js & React
*   **Language:** TypeScript
*   **Design:** Responsive HTML/CSS layout targeting cross-device compatibility
*   **Integration:** Custom Fetch API wrappers for robust frontend-to-backend communication

---

## 🚀 Key Features

*   **SaaS Administration:** Secure endpoint provisioning mimicking enterprise SaaS modules to register classes, subjects, and allocate teacher-to-curriculum mappings.
*   **Teacher Dashboards:** Comprehensive tooling for educators to draft, publish, and evaluate student assignments with fluid UI feedback.
*   **Student Portals:** Deadline-enforced submission workflows simulating real-world data validation constraints gracefully communicated through the UI.
*   **Dynamic Telemetry:** Real-time metrics generated organically from live backend queries, emphasizing performance optimization.

---

## 💻 Running the Project Locally

To run this stack, ensure the **.NET 8 SDK** and **Node.js** are installed.

### 1. Launch the Backend API
```bash
cd backend/AssignmentSystem.Api
dotnet restore
dotnet run --urls="http://localhost:5033"
```
*(The infrastructure utilizes EF Core to auto-generate and heavily seed the test database upon startup.)*

### 2. Launch the Next.js Frontend
```bash
cd frontend
npm install
npm run dev
```

### 🏷️ Seeded Test Accounts
Use these to explore the complex authorization matrices implemented across the backend:
- **Admin Access:** `admin@school.edu` / `Admin@123`
- **Teacher Access:** `alice.teacher@school.edu` / `Teacher@123`
- **Student Access:** `charlie.student@school.edu` / `Student@123`

---

## 💡 Why Me?
Building EduTrack has fortified my understanding of exactly how scalable modular APIs integrate securely with modern frontend features. I have a proactive, ownership-driven attitude, strong problem-solving skills, and an immense willingness to rapidly learn your SaaS modules and start delivering value.

I look forward to discussing how my analytical skills and this project align with your team's goals at the technical interview.
