# Multi-stage Dockerfile for Badminton_BE (ASP.NET Core 10)
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore first for layer caching
COPY Badminton_BE/Badminton_BE.csproj Badminton_BE/
RUN dotnet restore Badminton_BE/Badminton_BE.csproj

# copy everything and publish
COPY . .
RUN dotnet publish Badminton_BE/Badminton_BE.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Badminton_BE.dll"]
