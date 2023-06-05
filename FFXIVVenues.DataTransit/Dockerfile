FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FFXIVVenues.DataTransit/FFXIVVenues.DataTransit.csproj", "FFXIVVenues.DataTransit/"]
RUN dotnet restore "FFXIVVenues.DataTransit/FFXIVVenues.DataTransit.csproj"
COPY . .
WORKDIR "/src/FFXIVVenues.DataTransit"
RUN dotnet build "FFXIVVenues.DataTransit.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FFXIVVenues.DataTransit.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FFXIVVenues.DataTransit.dll"]
