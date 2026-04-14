# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy file .csproj và restore dependencies
COPY ["UC.eComm.Publish.csproj", "./"]
RUN dotnet restore "UC.eComm.Publish.csproj"

# Copy toàn bộ source code còn lại
COPY . .

# Build và publish project
RUN dotnet publish "UC.eComm.Publish.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UC.eComm.Publish.dll"]
