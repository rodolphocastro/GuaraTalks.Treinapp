#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Treinapp.Reports.Portal/Treinapp.Reports.Portal.csproj", "src/Treinapp.Reports.Portal/"]
COPY ["src/Treinapp.Common/Treinapp.Common.csproj", "src/Treinapp.Common/"]
RUN dotnet restore "src/Treinapp.Reports.Portal/Treinapp.Reports.Portal.csproj"
COPY . .
WORKDIR "/src/src/Treinapp.Reports.Portal"
RUN dotnet build "Treinapp.Reports.Portal.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Treinapp.Reports.Portal.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Treinapp.Reports.Portal.dll"]
