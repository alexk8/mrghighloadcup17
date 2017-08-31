#dotnet clean -c Release
dotnet build -c Release
dotnet publish -c Release
cp appsettings.json bin/Release/netcoreapp2.0/publish/
rm -r obj/Docker/publish 
mv bin/Release/netcoreapp2.0/publish obj/Docker/publish
docker build -t verkest .

