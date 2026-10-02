# CropTrack

[![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core/)
[![Entity Framework Core](https://img.shields.io/badge/Entity%20Framework-Core-6B3FA0)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![MAUI](https://img.shields.io/badge/.NET%20MAUI-In%20Development-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/maui/)

A general agricultural management application built with **C#** and **ASP.NET Core**. CropTrack pairs a secure REST API with a cross-platform frontend that is currently in development, and tracks farmers, crops, and related records in a relational SQL Server database.

> Started as part of a 60-hour internship at [ScaleFocus](https://scalefocus.com/).

---

## Overview

CropTrack is a two-part application:

- A **REST API backend** with a layered architecture built on the Service and Repository patterns. It offers full CRUD operations and JWT-based authentication to protect its endpoints.
- A **cross-platform frontend** built with .NET MAUI, currently in development.

The goal of this project was to practice building a clean, maintainable backend end to end: relational database design, separation of concerns across layers, and token-based security.

---

## Tech Stack

**Backend**
- C# / ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT bearer authentication

**Frontend**
- .NET MAUI (in development)

---

## Features

-  **Layered architecture** - controllers, services, and repositories kept separate for clear responsibilities and easier testing
-  **Full CRUD operations** - create, read, update, and delete across the application's entities
-  **JWT authentication** - token-based access to protected endpoints
-  **Relational data model** - SQL Server tables designed around agricultural entities such as farmers, crops, and related records
-  **Cross-platform client** - .NET MAUI app in development

---

## Project Structure

```text
CropTrack/
├── croptrack/            Backend REST API (ASP.NET Core)
├── CropTrack.Shared/     Code shared between projects
├── CropTrackApp/         .NET MAUI frontend (in development)
├── demo/                 Demo material
└── croptrack.sln         Visual Studio solution
```

---

## Getting Started

### Requirements

- .NET SDK
- SQL Server or SQL Server LocalDB
- Visual Studio with the .NET MAUI workload (for the frontend)

### Backend

```bash
cd croptrack
dotnet restore
dotnet ef database update
dotnet run
```

Configure your connection string and JWT settings in `appsettings.Development.json`, or keep them out of the repository with [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets).

### Frontend

Open `croptrack.sln` in Visual Studio, set `CropTrackApp` as the startup project, choose a target platform, and run.
