FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/JobBoardApi/JobBoardApi.csproj ./JobBoardApi/
RUN dotnet restore ./JobBoardApi/JobBoardApi.csproj

COPY src/ ./
RUN dotnet publish ./JobBoardApi/JobBoardApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "JobBoardApi.dll"]
