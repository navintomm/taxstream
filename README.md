# TaxStream-AI

**Developer-Ready Backend** · .NET 10 Web API · Onion Architecture

## Stack
- Backend: .NET 10 Web API, C#
- ORM: Entity Framework Core + SQL Server
- AI: Azure OpenAI (interface-abstracted)
- Auth: ASP.NET Core JWT Bearer
- Tests: xUnit + Moq + FluentAssertions

## Solution Structure
```
src/
  TaxStream.Core/           → Domain entities, interfaces, exceptions
  TaxStream.Application/    → Services, DTOs, abstractions
  TaxStream.Infrastructure/ → EF Core, repositories, Azure OpenAI adapter, JWT
  TaxStream.WebAPI/         → Controllers, middleware, Program.cs
tests/
  TaxStream.Core.Tests/
  TaxStream.Application.Tests/
  TaxStream.WebAPI.Tests/
```

## Setup

### 1. Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server or LocalDB
- Azure OpenAI resource (or update `IAiProviderService` with another provider)

### 2. Configure secrets (never commit real values)
```bash
dotnet user-secrets set "Jwt:SigningKey" "<your-32+-char-secret>" --project src/TaxStream.WebAPI
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://<your-resource>.openai.azure.com/" --project src/TaxStream.WebAPI
dotnet user-secrets set "AzureOpenAI:ApiKey" "<your-api-key>" --project src/TaxStream.WebAPI
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>" --project src/TaxStream.WebAPI
```

### 3. Run migrations
```bash
dotnet ef migrations add InitialCreate --project src/TaxStream.Infrastructure --startup-project src/TaxStream.WebAPI
dotnet ef database update --project src/TaxStream.Infrastructure --startup-project src/TaxStream.WebAPI
```

### 4. Run the API
```bash
dotnet run --project src/TaxStream.WebAPI
```

Swagger UI: `https://localhost:<port>/swagger`

### 5. Run tests
```bash
dotnet test
```

## API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | /api/users/register | Public | Register a new account |
| POST | /api/users/login | Public | Login and get JWT token |
| GET | /api/taxrecords | Bearer | List your tax records |
| POST | /api/taxrecords | Bearer | Create a tax record |
| GET | /api/taxrecords/{id} | Bearer | Get a specific record |
| PUT | /api/taxrecords/{id} | Bearer | Update a record |
| DELETE | /api/taxrecords/{id} | Bearer | Delete a record |
| POST | /api/ai/chat | Bearer | Ask AI a tax question |
| GET | /api/ai/history | Bearer | Get your chat history |

## ⚠️ Important Notes
- **Tax jurisdiction**: The tax calculation is a placeholder. Confirm jurisdiction and implement real rules before production.
- **AI responses**: Informational only. Always verify with official sources.
- **Secrets**: Use environment variables or Azure Key Vault in production.
