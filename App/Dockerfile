FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

EXPOSE 80

# copy csproj and restore as distinct layers
COPY *.props .
COPY *.sln .

# copy over all the projects from host to image
#Base
COPY Base.BLL.Contracts/*.csproj ./Base.BLL.Contracts/
COPY Base.BLL/*.csproj ./Base.BLL/
COPY Base.DAL.Contracts/*.csproj ./Base.DAL.Contracts/
COPY Base.DAL/*.csproj ./Base.DAL/
COPY Base.Domain.Contracts/*.csproj ./Base.Domain.Contracts/
COPY Base.Domain/*.csproj ./Base.Domain/
COPY Base.Mapper.Contracts/*.csproj ./Base.Mapper.Contracts/
COPY Base.Mapper/*.csproj ./Base.Mapper/

#App
COPY Public.DTO/*.csproj ./Public.DTO/
COPY App.BLL.Contracts/*.csproj ./App.BLL.Contracts/
COPY App.BLL/*.csproj ./App.BLL/
COPY App.DAL.Contracts/*.csproj ./App.DAL.Contracts/
COPY App.DAL/*.csproj ./App.DAL/
COPY App.Domain/*.csproj ./App.Domain/
COPY Public.DTO/*.csproj ./Public.DTO/
COPY Helpers/*.csproj ./Helpers/
COPY WebApp/*.csproj ./WebApp/
COPY ApplicationTests/*.csproj ./ApplicationTests/

RUN dotnet restore

# copy everything else (source files) and build app

#Base
COPY Base.BLL.Contracts/ ./Base.BLL.Contracts/
COPY Base.BLL/ ./Base.BLL/
COPY Base.DAL.Contracts/ ./Base.DAL.Contracts/
COPY Base.DAL/ ./Base.DAL/
COPY Base.Domain.Contracts/ ./Base.Domain.Contracts/
COPY Base.Domain/ ./Base.Domain/
COPY Base.Mapper.Contracts/ ./Base.Mapper.Contracts/
COPY Base.Mapper/ ./Base.Mapper/

#App
COPY Public.DTO/ ./Public.DTO/
COPY App.BLL.Contracts/ ./App.BLL.Contracts/
COPY App.BLL/ ./App.BLL/
COPY App.DAL.Contracts/ ./App.DAL.Contracts/
COPY App.DAL/ ./App.DAL/
COPY App.Domain/ ./App.Domain/
COPY Helpers/ ./Helpers/
COPY WebApp/ ./WebApp/
COPY ApplicationTests/ ./ApplicationTests/

WORKDIR /src/WebApp
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
EXPOSE 80

COPY --from=build /src/WebApp/out ./

ENTRYPOINT ["dotnet", "WebApp.dll"]