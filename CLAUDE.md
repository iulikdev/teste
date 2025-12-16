# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Microsoft Teams Call Center application with a compliance recording bot and real-time dashboard. The bot auto-joins Teams calls via compliance recording policy and streams call events to a Svelte dashboard via SignalR.

## Build & Run Commands

### Backend (.NET 8)

```bash
# Build solution
dotnet build TeamsCallCenter.sln

# Run API+Bot (single application)
dotnet run --project src/TeamsCallCenter.Api
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

Two-tier architecture with real-time communication:

1. **TeamsCallCenter.Api** - ASP.NET Core Web API with integrated Bot
   - **API Layer:**
     - `CallsHub` at `/hubs/calls` broadcasts call events to dashboard clients
     - `InMemoryCallStateService` tracks active calls (intended for Redis/SQL in production)
     - REST endpoints in `CallsController` and `CallingController`
     - Swagger UI available in development mode
   - **Bot Layer** (`Bot/` folder):
     - Uses Microsoft Graph Communications SDK for call handling
     - `BotService` manages call lifecycle and media sessions
     - `CallHandler` wraps individual calls, processes state changes
     - `BotMediaStream` handles audio stream processing
     - `DirectCallEventPublisher` publishes events directly to SignalR hub
   - **Audio Recording** (`Bot/Audio/`):
     - `AudioRecordingService` manages recording sessions
     - `WavFileWriter` writes PCM audio to WAV files

2. **TeamsCallCenter.Shared** - Shared models library
   - `CallInfo`, `CallStatus`, `CallDirection` - call state models
   - `CallStatistics`, `AgentInfo` - dashboard data models
   - `RecordingInfo` - recording metadata model

3. **dashboard/** - SvelteKit + TypeScript frontend
   - `src/lib/signalr.ts` - SignalR client with auto-reconnect
   - `src/lib/stores.ts` - Svelte stores for reactive state
   - `src/lib/teams.ts` - Teams JS SDK integration
   - Uses `@microsoft/teams-js` for Teams Tab embedding

## Key Data Flow

```
Teams Call → Bot receives notification → BotService processes → DirectCallEventPublisher
    → CallsHub broadcasts to all clients → Dashboard updates reactively
```

SignalR events: `CallStarted`, `CallEnded`, `CallUpdated`, `StatisticsUpdated`, `InitialState`

## API Endpoints

- `POST /api/calling` - Receives Teams Graph notifications
- `POST /api/calling/callback` - Receives Teams callbacks
- `GET /api/calling/calls` - Get active calls from bot
- `DELETE /api/calling/calls/{callId}` - End a specific call
- `GET /api/calls` - Get all tracked calls
- `GET /health` - Health check

## Configuration

All configuration is in `src/TeamsCallCenter.Api/appsettings.json`:

```json
{
  "Bot": {
    "AppId": "YOUR_BOT_APP_ID",
    "AppSecret": "YOUR_BOT_APP_SECRET",
    "BotName": "TeamsCallCenterBot",
    "ServiceDnsName": "your-bot.azurewebsites.net",
    "ServiceCname": "your-bot.azurewebsites.net",
    "CertificateThumbprint": "YOUR_CERTIFICATE_THUMBPRINT",
    "InstancePublicPort": 443,
    "InstanceInternalPort": 8445,
    "CallSignalingPort": 9441
  },
  "Recording": {
    "Enabled": true,
    "RecordingPath": "C:\\Recordings\\TeamsCallCenter"
  }
}
```

- Dashboard API URL: `VITE_API_URL` env var (defaults to `https://localhost:7001`)

## Requirements

- .NET 8 SDK
- Node.js 18+
- Azure Bot registration with `Calls.AccessMedia.All` and `Calls.JoinGroupCall.All` permissions
- SSL certificate and public IP for Bot media streams (cannot run Bot fully locally)

## Port Configuration

| Component | Port | Protocol | Description |
|-----------|------|----------|-------------|
| API+Bot | 443 (or 7001 dev) | HTTPS | REST API + SignalR Hub + Bot webhooks |
| Dashboard | 5173 | HTTP | Svelte dev server |
| Media Internal | 8445 | HTTPS | Bot internal media port |
| Call Signaling | 9441 | TCP | Bot call signaling |
