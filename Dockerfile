# Est�gio 1: Build da Aplica��o
# Usa a imagem oficial do SDK do .NET 8 para construir o projeto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia os arquivos do projeto (.csproj) e restaura as depend�ncias
COPY *.csproj .
RUN dotnet restore

# Copia todo o resto do c�digo fonte e publica a aplica��o
COPY . .
RUN dotnet publish -c Release -o out

# Est�gio 2: Imagem Final
# Usa uma imagem menor, apenas com o necess�rio para rodar a aplica��o
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copia os arquivos publicados do est�gio de build
COPY --from=build /app/out .

# Exp�e a porta que a aplica��o vai usar
EXPOSE 8080

# Define o comando para iniciar a aplica��o
ENTRYPOINT ["dotnet", "ControleDeContasMVC.dll"]