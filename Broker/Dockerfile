# Ётап сборки
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

#  опируем .csproj и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

#  опируем весь исходный код и выполн€ем сборку проекта
COPY . ./
RUN dotnet publish -c Release -o out

# Ётап выполнени€: минимальный образ дл€ выполнени€ .NET приложени€
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /app

#  опируем собранное приложение из этапа сборки
COPY --from=build-env /app/out .

# ќткрываем порт дл€ MQTT-брокера (например, 1883)
EXPOSE 1883

# ”казываем команду дл€ запуска приложени€
ENTRYPOINT ["dotnet", "Broker.dll"]
 