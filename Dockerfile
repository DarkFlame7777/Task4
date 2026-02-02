FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Task4.csproj", "./"]
RUN dotnet restore "Task4.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Task4.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/build .
ENV ASPNETCORE_URLS=http://0.0.0.0:3000
EXPOSE 3000
ENTRYPOINT ["dotnet", "Task4.dll"]
