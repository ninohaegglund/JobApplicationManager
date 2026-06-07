# JobApplicationManager

A fullstack web application for saving and managing job applications in one place. The project is built as a personal job application manager where users can track application statu and keep a structured overview of their job search process.

## Overview

JobApplicationManager is designed to solve a practical problem: keeping track of job opportunities and applications during an active job search. Instead of storing job ads, notes, and application statuses in separate places, the application provides a centralized system for managing the process.

The project demonstrates fullstack development with .NET, API integration, authentication, database handling, and a modern frontend structure.

## Features

* Track application status
* Organize applications by status, for example saved, applied, interview, rejected, or offer
* Store user-specific application data
* Authentication and authorization
* Backend API built with ASP.NET Core
* Database persistence with Entity Framework Core
* Frontend built with React and TypeScript
* Structured API communication between frontend and backend

## Tech Stack

### Backend

* C#
* .NET / ASP.NET Core
* Entity Framework Core
* REST API
* Authentication / Authorization
* SQL database

### Frontend

* React
* TypeScript
* HTML
* CSS
* API integration using HTTP requests

## Project Purpose

The purpose of this project is to build a realistic application that combines several important parts of modern web development:

* Working with external APIs
* Handling user accounts and protected data
* Structuring backend services and controllers
* Persisting data in a relational database
* Building a frontend that communicates with a backend API
* Creating a useful tool based on a real-world problem

## Architecture

The application follows a client-server architecture:

```text
Frontend (React + TypeScript)
        |
        | HTTP requests
        v
Backend API (ASP.NET Core)
        |
        | Entity Framework Core
        v
Database
        |
        | External API integration
        v
Job Search API
```

The backend is responsible for business logic, authentication, data persistence, and communication with the external job search API. The frontend is responsible for presenting data and providing an interactive user experience.

## Getting Started

### Prerequisites

Make sure you have the following installed:

* .NET SDK
* Node.js
* npm
* SQL Server or another configured database provider
* Git

## Installation

### 1. Clone the repository

```bash
git clone https://github.com/ninohaegglund/JobApplicationManager.git
cd JobApplicationManager
```

### 2. Backend setup

Navigate to the backend project folder:

```bash
cd backend
```

Restore dependencies:

```bash
dotnet restore
```

Update the database:

```bash
dotnet ef database update
```

Run the backend:

```bash
dotnet run
```

### 3. Frontend setup

https://github.com/ninohaegglund/JobApplicationManagerUI


Install dependencies:

```bash
npm install
```

Start the development server:

```bash
npm run dev
```

## Configuration

The backend requires configuration for database connection and external API usage. Add the required values in `appsettings.json` or use user secrets during development.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-database-connection-string"
  },
  "JobSearchApi": {
    "BaseUrl": "https://jobsearch.api.jobtechdev.se"
  }
}
```

Do not commit sensitive values such as connection strings, API keys, or secrets to the repository.

## Possible Application Statuses

Example statuses that can be used in the application:

* Saved
* Applied
* Interview
* Rejected
* Offer

## Planned Improvements

* Add filtering by status, date, company, and role
* Improve search filters
* Add pagination for search results
* Add validation and better error handling
* Improve responsive design
* Add tests for backend services and controllers

## What I Learned

Through this project, I practiced:

* Building a fullstack application with .NET and React
* Creating REST endpoints in ASP.NET Core
* Working with Entity Framework Core and database migrations
* Integrating an external API
* Structuring application data around real user needs
* Handling authentication and user-specific data
* Designing a project that solves a practical problem


## Author

Nino Hägglund
Web Developer .NET student

## License

This project is intended for portfolio and educational purposes.
