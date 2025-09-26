
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/ToDoApi/ToDoApi.csproj src/ToDoApi/
RUN dotnet restore src/ToDoApi/ToDoApi.csproj
COPY src/ToDoApi/ src/ToDoApi/
RUN dotnet publish src/ToDoApi/ToDoApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "ToDoApi.dll"]
