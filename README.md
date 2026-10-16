# RaceDay API — Part 2 (POE)

**Module:** PROG6212 - Programming 2B
**Assessment:** POE Part 2
**Student:** Decent210810

---

## Overview

RESTful Web API built with ASP.NET Core and Entity Framework Core for the RaceDay event management platform.

## Two Roles

- **Organiser** — Create/manage events, categories, and results
- **Participant** — Browse events, enrol, view results

## Tech Stack

- ASP.NET Core Web API
- Entity Framework Core (Code-First)
- SQL Server
- BCrypt.Net (password hashing)
- Session-based authentication
- xUnit unit tests

## API Endpoints (28 total)

- **Auth** (4): register, login, logout, me
- **Users** (3): view, update, me
- **Events** (6): CRUD + my-events
- **Categories** (5): CRUD for events
- **Enrolments** (5): enrol, view, update status, withdraw
- **Results** (5): capture, view, update, delete

## Setup Instructions

1. Clone the repo
2. Open `RaceDay.API.slnx` in Visual Studio
3. Update `appsettings.json` connection string
4. Run `Update-Database` in Package Manager Console
5. Press F5 to run the API

## Unit Tests

29 comprehensive tests covering:
- Registration, login, validation
- Event CRUD + role checks
- Enrolment workflow
- Role-based access control

**All 29 tests passing.**

## CI/CD

GitHub Actions workflow builds and tests on every push.