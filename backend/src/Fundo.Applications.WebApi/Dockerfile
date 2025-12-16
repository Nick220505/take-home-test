FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["src.sln", "./"]
COPY ["Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj", "Fundo.Applications.WebApi/"]
COPY ["Fundo.Services.Tests/Fundo.Services.Tests.csproj", "Fundo.Services.Tests/"]

RUN dotnet restore "Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj"

COPY . .
WORKDIR "/src/Fundo.Applications.WebApi"
RUN dotnet publish "Fundo.Applications.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Fundo.Applications.WebApi.dll"]
