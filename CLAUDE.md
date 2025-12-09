# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Microsoft Teams Call Center application with a compliance recording bot and real-time dashboard. The bot auto-joins Teams calls via compliance recording policy and streams call events to a Svelte dashboard via SignalR.

## Build & Run Commands

### Backend (.NET 8)

```bash
# Build solution
dotnet build TeamsCallCenter.sln

# Run API (Terminal 1)
dotnet run --project src/TeamsCallCenter.Api

# Run Bot (Terminal 2) - requires SSL cert and public IP
dotnet run --project src/TeamsCallCenter.Bot
```

### Dashboard (Svelte/Vite)

```bash
cd dashboard
npm install
npm run dev      # Development server on localhost:5173
npm run build    # Production build to build/
npm run preview  # Preview production build
```

## Architecture

Three-tier architecture with real-time communication:

1. **TeamsCallCenter.Bot** - .NET Worker Service that handles Teams call recording
   - Uses Microsoft Graph Communications SDK for call handling
   - `BotService` manages call lifecycle and media sessions
   - `CallHandler` wraps individual calls, processes state changes
   - `BotMediaStream` handles audio stream processing
   - Publishes events to API via `SignalRCallEventPublisher`

2. **TeamsCallCenter.Api** - ASP.NET Core Web API with SignalR
   - `CallsHub` at `/hubs/calls` broadcasts call events to dashboard clients
   - `InMemoryCallStateService` tracks active calls (intended for Redis/SQL in production)
   - REST endpoints in `CallsController` and `CallingController`
   - Swagger UI available in development mode

3. **TeamsCallCenter.Shared** - Shared models library
   - `CallInfo`, `CallStatus`, `CallDirection` - call state models
   - `CallStatistics`, `AgentInfo` - dashboard data models

4. **dashboard/** - SvelteKit + TypeScript frontend
   - `src/lib/signalr.ts` - SignalR client with auto-reconnect
   - `src/lib/stores.ts` - Svelte stores for reactive state
   - `src/lib/teams.ts` - Teams JS SDK integration
   - Uses `@microsoft/teams-js` for Teams Tab embedding

## Key Data Flow

Bot receives call → publishes via SignalR → API broadcasts to all clients → Dashboard updates reactively

SignalR events: `CallStarted`, `CallEnded`, `CallUpdated`, `StatisticsUpdated`, `InitialState`

## Configuration

- Bot config: `src/TeamsCallCenter.Bot/appsettings.json` (AppId, AppSecret, CertificateThumbprint)
- API config: `src/TeamsCallCenter.Api/appsettings.json`
- Dashboard API URL: `VITE_API_URL` env var (defaults to `https://localhost:7001`)

## Requirements

- .NET 8 SDK
- Node.js 18+
- Azure Bot registration with `Calls.AccessMedia.All` and `Calls.JoinGroupCall.All` permissions
- SSL certificate and public IP for Bot media streams (cannot run Bot fully locally)
