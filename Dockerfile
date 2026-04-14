# Sử dụng SDK .NET 8 để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy file .csproj và restore dependencies
COPY ["UC.eComm.Publish/UC.eComm.Publish.csproj", "UC.eComm.Publish/"]
RUN dotnet restore "UC.eComm.Publish/UC.eComm.Publish.csproj"

# Copy toàn bộ code còn lại
COPY . .

# Build và publish project
WORKDIR "/app/UC.eComm.Publish"
RUN dotnet publish "UC.eComm.Publish.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UC.eComm.Publish.dll"]
