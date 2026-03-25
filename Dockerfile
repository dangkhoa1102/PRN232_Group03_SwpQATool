# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Lab03_Assignment.sln", "."]
COPY ["BusinessLogicLayer/BusinessLogicLayer.csproj", "BusinessLogicLayer/"]
COPY ["DataAccessLayer/DataAccessLayer.csproj", "DataAccessLayer/"]
COPY ["SWP_Q&A_Tools_APIs/SWP_Q&A_Tools_APIs.csproj", "SWP_Q&A_Tools_APIs/"]
RUN dotnet restore "SWP_Q&A_Tools_APIs/SWP_Q&A_Tools_APIs.csproj"

COPY . .
WORKDIR /src/SWP_Q&A_Tools_APIs
RUN dotnet publish "SWP_Q&A_Tools_APIs.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SWP_Q&A_Tools_APIs.dll"]
