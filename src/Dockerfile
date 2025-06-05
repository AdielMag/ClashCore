#####################################################################
# Build everything once (layer-cache optimized, solution 1)
#####################################################################

# Use matching .NET SDK version (you are on 8.0 now)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy full source code (after restore)
COPY src/ ./

# Restore dependencies
RUN dotnet restore ./MyApp.Server.Services/MyApp.Server.Services.csproj
RUN dotnet restore ./MyApp.Server.GameHub/MyApp.Server.GameHub.csproj



# Publish both projects
RUN dotnet publish ./MyApp.Server.Services/MyApp.Server.Services.csproj \
       -c Release -o /artifacts/services --no-restore \
 && dotnet publish ./MyApp.Server.GameHub/MyApp.Server.GameHub.csproj \
       -c Release -o /artifacts/gamehub  --no-restore


#####################################################################
# Runtime image – Services
#####################################################################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS services
WORKDIR /app
COPY --from=build /artifacts/services .
ENV ASPNETCORE_ENVIRONMENT=Development \
    DISABLE_HTTPS=true
ENTRYPOINT ["dotnet", "MyApp.Server.Services.dll"]


#####################################################################
# Runtime image – GameHub
#####################################################################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS gamehub
WORKDIR /app
COPY --from=build /artifacts/gamehub .
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "MyApp.Server.GameHub.dll"]
