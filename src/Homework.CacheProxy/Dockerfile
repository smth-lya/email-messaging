FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Development
WORKDIR /src

COPY ["Homework.Api/Homework.Api.csproj", "Homework.Api/"]
RUN dotnet restore "Homework.Api/Homework.Api.csproj"

COPY . .
WORKDIR "/src/Homework.Api"
RUN dotnet build "Homework.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Development
RUN dotnet publish "Homework.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Homework.Api.dll"]