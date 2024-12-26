# Use a imagem base do .NET SDK 8.0 para compilar o aplicativo
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copie os arquivos do projeto e restaure as dependências
COPY ["WebSocketServer.csproj", "./"]
RUN dotnet restore "WebSocketServer.csproj"

# Copie o restante dos arquivos do projeto e compile o aplicativo
COPY . .
RUN dotnet publish "WebSocketServer.csproj" -c Release -o /app/publish

# Use a imagem base do .NET Runtime 8.0 para executar o aplicativo
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Exponha a porta que o aplicativo irá escutar
EXPOSE 5052

# Defina o comando de entrada para executar o aplicativo
ENTRYPOINT ["dotnet", "WebSocketServer.dll"]