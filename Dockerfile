# Estágio 1: Build da Aplicação
# Usa a imagem oficial do SDK do .NET 8 para construir o projeto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia os arquivos do projeto (.csproj) e restaura as dependências
COPY *.csproj .
RUN dotnet restore

# Copia todo o resto do código fonte e publica a aplicação
COPY . .
RUN dotnet publish -c Release -o out

# Estágio 2: Imagem Final
# Usa uma imagem menor, apenas com o necessário para rodar a aplicação
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copia os arquivos publicados do estágio de build
COPY --from=build /app/out .

# Expõe a porta que a aplicação vai usar
EXPOSE 8080

# Define o comando para iniciar a aplicação
ENTRYPOINT ["dotnet", "ControleDeContasMVC.dll"]