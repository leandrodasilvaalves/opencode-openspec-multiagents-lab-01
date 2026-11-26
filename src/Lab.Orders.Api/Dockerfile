FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Lab.Orders.slnx", "./"]
COPY ["src/Lab.Orders.Domain/Lab.Orders.Domain.csproj", "src/Lab.Orders.Domain/"]
COPY ["src/Lab.Orders.Application/Lab.Orders.Application.csproj", "src/Lab.Orders.Application/"]
COPY ["src/Lab.Orders.Infrastructure/Lab.Orders.Infrastructure.csproj", "src/Lab.Orders.Infrastructure/"]
COPY ["src/Lab.Orders.Api/Lab.Orders.Api.csproj", "src/Lab.Orders.Api/"]
RUN dotnet restore "src/Lab.Orders.Api/Lab.Orders.Api.csproj"

COPY . .
WORKDIR "/src/src/Lab.Orders.Api"
RUN dotnet build "Lab.Orders.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lab.Orders.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
EXPOSE 5000
WORKDIR /app
USER app
COPY --chown=app:app --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lab.Orders.Api.dll"]
