# Campus Event Management - Booking Service

This is the Booking Service microservice for the Campus Event Management system.

## Tech Stack
- ASP.NET Core Web API
- In-memory database or SQLite
- Azure Web App for hosting

## Features
- Create and manage event bookings
- Check event capacity (communicates with Event Service)
- View bookings by user or event

## API Endpoints

### POST /api/bookings
Create a new booking for an event:
- `eventId` - ID of the event to book

### GET /api/bookings
Get bookings for the current user (requires authentication)

### GET /api/bookings?eventId={eventId}
Get all bookings for a specific event (requires admin role)

### DELETE /api/bookings/{id}
Cancel a booking (requires authentication or admin role)

## Service Communication
This service communicates with the Event Service to:
- Validate event capacity before booking
- Update event registration count after booking

## Setup Instructions
1. Clone the repository
2. Open in Visual Studio or your preferred IDE
3. Configure the Event Service URL in appsettings.json
4. Build and run the project
5. The API will be available at `https://localhost:5003` or similar port

## Deployment
The application is deployed to Azure Web App. 