#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Pondrop.Service.Product.Api/Pondrop.Service.Product.Api.csproj", "src/Pondrop.Service.Product.Api/"]
COPY ["src/Pondrop.Service.Product.Application/Pondrop.Service.Product.Application.csproj", "src/Pondrop.Service.Product.Application/"]
COPY ["src/Pondrop.Service.Product.Domain/Pondrop.Service.Product.Domain.csproj", "src/Pondrop.Service.Product.Domain/"]
COPY ["src/Pondrop.Service.Product.Infrastructure/Pondrop.Service.Product.Infrastructure.csproj", "src/Pondrop.Service.Product.Infrastructure/"]
RUN dotnet restore "src/Pondrop.Service.Product.Api/Pondrop.Service.Product.Api.csproj"
COPY . .
WORKDIR "/src/src/Pondrop.Service.Product.Api"
RUN dotnet build "Pondrop.Service.Product.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pondrop.Service.Product.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pondrop.Service.Product.Api.dll"]