# ���� ������
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# �������� .csproj � ��������������� �����������
COPY *.csproj ./
RUN dotnet restore

# �������� ���� �������� ��� � ��������� ������ �������
COPY . ./
RUN dotnet publish -c Release -o out

# ���� ����������: ����������� ����� ��� ���������� .NET ����������
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /app

# �������� ��������� ���������� �� ����� ������
COPY --from=build-env /app/out .

# ��������� ���� ��� MQTT-������� (��������, 1883)
EXPOSE 1883

# ��������� ������� ��� ������� ����������
ENTRYPOINT ["dotnet", "Broker.dll"]
 