# RaceDay API — Part 2 (POE)

**Module:** PROG6212 - Programming 2B  
**Assessment:** Portfolio of Evidence (POE) — Part 2  
**Student:** Decent210810  
**Year:** 2026

---

## 📖 Overview

The **RaceDay API** is a RESTful Web API built with **ASP.NET Core** and **Entity Framework Core** (Code-First). It powers the RaceDay event management platform for the South African road running, walking, and cycling community.

This is **Part 2** of the POE, delivering the complete backend API with role-based authentication, event management, enrolments, and results capture.

---

## 👥 Two User Roles

### 🏗️ Organiser
- Register and log in
- Create, update, and delete events
- Manage event categories
- Capture participant results
- View all event enrolments

### 🏃 Participant
- Register and log in
- Browse events
- Enrol in events by selecting a category
- View personal enrolment history
- Track personal race results

---

## 🛠️ Tech Stack

| Technology | Purpose |
|------------|---------|
| ASP.NET Core Web API | RESTful API framework |
| Entity Framework Core | ORM (Code-First) |
| SQL Server | Database |
| BCrypt.Net | Password hashing |
| Session-based Authentication | Auth mechanism |
| xUnit + EF InMemory | Unit testing |
| GitHub Actions | CI/CD |

---

## 📡 API Endpoints (28 total)

| Controller | Endpoints | Description |
|------------|-----------|-------------|
| **Auth** | 4 | Register, Login, Logout, Me |
| **Users** | 3 | View profile, Update profile, Get current user |
| **Events** | 6 | List, Get, Create, Update, Delete, My events |
| **Categories** | 5 | List, Get, Create, Update, Delete |
| **Enrolments** | 5 | Enrol, My enrolments, Get, Event enrolments, Update status, Withdraw |
| **Results** | 5 | Capture, My results, Event results, Update, Delete |

Full OpenAPI spec available at `/openapi/v1.json` when the API is running locally.

---

## 🚀 Setup & Run Instructions

### Prerequisites
- Visual Studio 2022 (or later)
- .NET SDK (9.0 or 10.0)
- SQL Server (LocalDB, Express, or full)

### Steps

1. **Clone the repository:**