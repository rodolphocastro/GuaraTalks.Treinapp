#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Treinapp.Reports.API/Treinapp.Reports.API.csproj", "src/Treinapp.Reports.API/"]
COPY ["src/Treinapp.Common/Treinapp.Common.csproj", "src/Treinapp.Common/"]
RUN dotnet restore "src/Treinapp.Reports.API/Treinapp.Reports.API.csproj"
COPY . .
WORKDIR "/src/src/Treinapp.Reports.API"
RUN dotnet build "Treinapp.Reports.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Treinapp.Reports.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Treinapp.Reports.API.dll"]
