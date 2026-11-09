# 🏃 RaceDay - Event Management System

## 📖 Overview

**RaceDay** is a full-stack web-based event management system designed specifically for the South African road running, walking, and cycling community. 

> **Module:** PROG6212 - Programming 2B  
> **Assessment:** Portfolio of Evidence (POE)  
> **Year:** 2026

---

## 👥 System Roles

### 🏗️ Organiser
- Create, edit, and delete events
- Manage event categories
- Upload event banner images
- View and approve enrolments
- Capture participant results

### 🏃 Participant
- Browse upcoming events
- View event details and categories
- Enrol in events
- View personal enrolments and results
- Update profile and upload profile picture

---

## 📂 Repository Structure

### Part 1: System Planning and Database
- **ERD Diagram**: Entity Relationship Diagram (`docs/ERD.png`)
- **API Endpoint Plan**: Complete API specification (`docs/API_Endpoint_Plan.md`)
- **Database Script**: SQL script for creating the database (`docs/Database_Script.sql`)

### Part 2: RESTful API Development
- **API Project**: ASP.NET Core Web API (`RaceDay.API/`)
- **Test Project**: xUnit Unit Tests (`RaceDay.API.Tests/`)
- **CI/CD**: GitHub Actions Workflow (`.github/workflows/ci-cd.yml`)
- **29 Unit Tests** passing covering Authentication, Events, Enrolments, and Role-based access.

### Part 3: MVC Frontend & Docker
- **MVC Project**: ASP.NET Core MVC Application (`RaceDay.MVC/`)
- **Dockerfile**: Containerisation setup (`RaceDay.MVC/Dockerfile`)
- **Azure Blob Storage**: Integration for uploading images via the API.
- **Complete UI**: Role-based navigation, dashboards, and form validation.

---

## 🛠️ Tech Stack

| Technology | Purpose |
|------------|---------|
| ASP.NET Core Web API | RESTful API framework (Part 2) |
| ASP.NET Core MVC | Web frontend (Part 3) |
| Entity Framework Core | ORM (Code-First) |
| SQL Server | Database |
| BCrypt.Net | Password hashing |
| xUnit + EF InMemory | Unit testing (Part 2) |
| Azure Blob Storage (Azurite) | Image storage |
| Docker | Containerisation (Part 3) |
| GitHub Actions | CI/CD |

---

## 🚀 Setup & Run Instructions

### Prerequisites
- Visual Studio 2022 (or later)
- .NET SDK (8.0 or later)
- SQL Server (LocalDB, Express, or full)

### Steps
1. **Clone the repository:**
   ```bash
   git clone https://github.com/Decent210810/PROG-PART-2-POE-ASSIGNMENT-.git
   cd PROG-PART-2-POE-ASSIGNMENT-
   ## Docker Setup

To run the MVC application in a Docker container:

1. Build the image:
   ```bash
   docker build -t raceday-mvc -f RaceDay.MVC/Dockerfile .