FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Sweet_Silvia/Sweet_Silvia.sln Sweet_Silvia/
COPY Sweet_Silvia/Sweet_Silvia_Domain/Sweet_Silvia_Domain.csproj Sweet_Silvia/Sweet_Silvia_Domain/
COPY Sweet_Silvia/Sweet_Silvia_Application/Sweet_Silvia_Application.csproj Sweet_Silvia/Sweet_Silvia_Application/
COPY Sweet_Silvia/Sweet_Silvia_Infrastructure/Sweet_Silvia_Infrastructure.csproj Sweet_Silvia/Sweet_Silvia_Infrastructure/
COPY Sweet_Silvia/Sweet_Silvia_Api/Sweet_Silvia_Api.csproj Sweet_Silvia/Sweet_Silvia_Api/

RUN dotnet restore Sweet_Silvia/Sweet_Silvia_Api/Sweet_Silvia_Api.csproj

COPY Sweet_Silvia/ Sweet_Silvia/
RUN dotnet publish Sweet_Silvia/Sweet_Silvia_Api/Sweet_Silvia_Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl libgssapi-krb5-2 \
    && curl --fail --silent --show-error \
        https://truststore.pki.rds.amazonaws.com/global/global-bundle.pem \
        --output /app/rds-global-bundle.pem \
    && apt-get purge -y --auto-remove curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Sweet_Silvia_Api.dll"]
