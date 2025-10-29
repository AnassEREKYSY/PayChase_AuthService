# PayChase Auth Service

JWT-based authentication microservice for **PayChase**.  
**Stack:** .NET 8 that includes Swagger UI, role claims, refresh tokens, and Artillery load tests.

---

## Features

- Register / Login with BCrypt password hashing  
- Signed JWT access tokens + opaque refresh tokens (stored in MongoDB)  
- Role claims (`ClaimTypes.Role`) in the access token  
- `/api/Auth/me` protected with `Bearer` authentication  
- `/health` endpoint for monitoring  
- Load testing with Artillery (`load/` folder)



## Project Structure

/API
├── Controllers
│ └── AuthController.cs
├── Program.cs
├── appsettings.json
└── (other Web API configuration files)

/Core
├── Dtos
│ ├── LoginRequest.cs
│ ├── RegisterRequest.cs
│ └── ...
├── Entities
│ ├── User.cs
│ └── RefreshToken.cs
└── Responses
│ ├── MeResponse.cs
│ └── TokenResponse.cs

/Infrastructure
├── Security
│ ├── JwtProvider.cs
│ └── RedisTokenBlacklist.cs
├── Repositories
│ ├── UserRepository.cs
│ └── RefreshTokenRepository.cs
├── Services
│ └── AuthService.cs
├── ISecurity
│ └── IJwtProvider.cs
└── IRepositories
├── └── IUserRepository.cs
├── └── IRefreshTokenRepository.cs
└── └── IOrgRepository.cs

/load
├── artillery.yml
└── (load test reports and outputs)

Dockerfile
docker-compose.yml
PayChase.Auth.sln
README.md

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker Desktop](https://www.docker.com/) (for the dockerized Mongo Database)
- [Node.js 18+](https://nodejs.org/) (for Artillery load tests)


## Quick start 

1. Run the API 
```bash
cd API
dotnet restore
dotnet run
```

Then open:

- Swagger UI: http://localhost:5261
- Health Check: http://localhost:5261/health

2. Run the load Artillery tests 

```bash 
cd load
npx artillery run artillery.yml -o report.json
npx artillery report report.json
```

## Endpoints 

1. POST /api/Auth/register

Body 
```bash
{
  "orgId": "org-1",
  "email": "user@x.com",
  "password": "P@ssw0rd",
  "name": "User"
}
```
Response 
```bash
{
  "id": "...",
  "orgId": "org-1",
  "email": "user@x.com",
  "roles": ["user"],
  "status": "active"
}
```

2. POST /api/Auth/login

Body 
```bash
{
  "orgId": "org-1",
  "email": "user@x.com",
  "password": "P@ssw0rd"
}
```
Response 
```bash
{
  "accessToken": "<jwt>",
  "expiresAt": "2025-10-29T15:54:24Z",
  "refreshToken": "<opaque>",
  "refreshExpiresAt": "2025-11-05T15:54:24Z"
}
```

3. 2. POST /api/Auth/me

Header 
```bash
Authorization: Bearer <accessToken>
```
Response 
```bash
{
  "id": "...",
  "orgId": "org-1",
  "email": "user@x.com",
  "roles": ["user"]
}
```


