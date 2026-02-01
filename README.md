# ShoppingCart - Backend

## 1. Install .NET SDK
- This project runs on .NET 8.0.401
- Download link: https://dotnet.microsoft.com/en-us/download/dotnet/8.0

## 2. Check the necessary tooling
~~~bash
dotnet --version # Check whether the correct dotnet version was installed
# Install dotnet-ef tooling for database migrations
dotnet tool install --global dotnet-ef
# OR
dotnet tool update --global dotnet-ef
dotnet-ef --version # Check whether the Entity Framework Core .NET Command-line Tools has been installed properly
~~~

## 3. Create an initial migration
~~~bash
dotnet ef migrations add InitialCreate --project App.DAL --startup-project WebApp
# Picks a project, where dbcontext is located, also specifies the startup project
~~~

## 4. Apply Migration and Update the Database
#### 4.1 Run the database container from the docker-compose.yml
#### 4.2 Run this script
~~~bash
dotnet ef database update --project App.DAL --startup-project WebApp
~~~
#### 4.3 Check whether the migration has been correctly applied. Connect to the database using the connection string from appsettings.json inside of WebApp

## 5. Run the application
~~~bash
docker-compose up --build
~~~


