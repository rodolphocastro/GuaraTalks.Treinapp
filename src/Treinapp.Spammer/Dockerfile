#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Treinapp.Spammer/Treinapp.Spammer.csproj", "src/Treinapp.Spammer/"]
COPY ["src/Treinapp.Common/Treinapp.Common.csproj", "src/Treinapp.Common/"]
RUN dotnet restore "src/Treinapp.Spammer/Treinapp.Spammer.csproj"
COPY . .
WORKDIR "/src/src/Treinapp.Spammer"
RUN dotnet build "Treinapp.Spammer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Treinapp.Spammer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Treinapp.Spammer.dll"]