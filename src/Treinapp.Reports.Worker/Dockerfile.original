#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Treinapp.Reports.Worker/Treinapp.Reports.Worker.csproj", "src/Treinapp.Reports.Worker/"]
RUN dotnet restore "src/Treinapp.Reports.Worker/Treinapp.Reports.Worker.csproj"
COPY . .
WORKDIR "/src/src/Treinapp.Reports.Worker"
RUN dotnet build "Treinapp.Reports.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Treinapp.Reports.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Treinapp.Reports.Worker.dll"]