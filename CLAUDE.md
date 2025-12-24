# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SimpChat is a real-time chat application backend built with ASP.NET Core 8.0. It features JWT authentication, SignalR WebSocket messaging, PostgreSQL persistence, and MinIO file storage.

**Status:** Active development (incomplete) - recent work on permissions and banning features.

## Build & Run Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run the web API (from repo root)
dotnet run --project src/Simpchat.Web

# Run with Docker Compose (includes PostgreSQL and MinIO)
docker-compose up --build
```

## Database Commands

EF Core migrations are in `src/Simpchat.Infrastructure/Persistence/Migrations/`. The DbContext is in the Infrastructure project.

```bash
# Add migration
dotnet ef migrations add <MigrationName> --project src/Simpchat.Infrastructure --startup-project src/Simpchat.Web

# Update database
dotnet ef database update --project src/Simpchat.Infrastructure --startup-project src/Simpchat.Web
```

Note: Migrations are auto-applied on startup in Development environment.

## Architecture

This is a 5-layer Clean Architecture solution:

```
src/
├── Simpchat.Domain        # Entities, Enums (no dependencies)
├── Simpchat.Application   # Services, Validators, DTOs, Interfaces
├── Simpchat.Infrastructure# DbContext, Repositories, External services (MinIO, Email, JWT)
├── Simpchat.Shared        # Configuration DTOs shared across layers
└── Simpchat.Web           # Controllers, SignalR Hub, Middlewares, Program.cs
```

**Dependency flow:** Web → Infrastructure → Application → Domain (+ Shared used across layers)

### Key Patterns

- **Repository Pattern:** All data access through repositories in `Infrastructure/Persistence/Repositories/`
- **Service Pattern:** Business logic in `Application/Features/` (e.g., `ChatService`, `MessageService`)
- **DI Registration:** Each layer has a `DependencyInjection.cs` file that registers its services
- **Validation:** FluentValidation validators in `Application/Validators/`
- **Mapping:** AutoMapper profiles in Application layer

### Core Domain Entities

- `User` - authentication, profiles, permissions
- `Chat` - base for groups/channels/DMs
- `Message` - with reactions and replies support
- `ChatPermission` / `ChatUserPermission` - role-based and per-user permissions
- `ChatBan` - user banning from specific chats

### API Structure

Controllers in `Web/Controllers/` follow RESTful conventions:
- `AuthController` - registration, login, password reset, OTP
- `ChatController` - direct messages, banning
- `GroupController` / `ChannelController` - group and channel management
- `MessageController` - send, edit, delete, reactions
- `PermissionController` - grant/revoke chat permissions

SignalR hub at `/hubs/chat` for real-time messaging (100KB message limit).

## Configuration

- `appsettings.json` - database connection, JWT settings, MinIO, RabbitMQ, email
- User Secrets ID: `a910f646-e8b4-484c-9fcc-5f316ecd5d0f`
- Docker services: API (5000/5001), PostgreSQL (5432), MinIO (9000/9001)

