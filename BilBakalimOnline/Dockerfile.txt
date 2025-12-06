# Runtime imajı
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Railway genelde PORT environment variable'ı veriyor.
# Varsayılan olarak 8080 kullan.
ENV PORT=8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Build imajı
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasını kopyala ve restore et
COPY ["BilBakalimOnline.csproj", "./"]
RUN dotnet restore "./BilBakalimOnline.csproj"

# Tüm kodu kopyala ve publish et
COPY . .
RUN dotnet publish "BilBakalimOnline.csproj" -c Release -o /app/publish

# Final imaj
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BilBakalimOnline.dll"]
