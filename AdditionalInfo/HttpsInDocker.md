# How to enable https for docker containers

For .net project that runs in docker that is set up on linux machine

### 1. Create a configuration file for OpenSSL

Create a file (somewhere in the project directory) named `openssl.cnf` with the following content to include both DNS names:

```
[ req ]
default_bits       = 2048
distinguished_name = req_distinguished_name
req_extensions     = req_ext
x509_extensions    = v3_req
prompt             = no

[ req_distinguished_name ]
C = US
ST = CA
L = SomeCity
O = MyCompany
OU = MyDivision
CN = localhost

[ req_ext ]
subjectAltName = @alt_names

[ v3_req ]
subjectAltName = @alt_names

[ alt_names ]
DNS.1 = localhost
DNS.2 = oauthserver
```

This configuration sets up the necessary fields for the certificate and specifies both `localhost` and `oauthserver` (should be the same as container name) as valid DNS names for the certificate.

### 2. Generate the certificate and private key

Use OpenSSL to generate the certificate and private key with the configuration file.

In the directory where `openssl.cnf` file is placed, run the command:

```
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout localhost.key -out localhost.crt -config openssl.cnf
```

### 3. Convert to PKCS#12 format (.pfx)

Convert the certificate and key into a `.pfx` file, which is used by Kestrel

In the directory where `localhost.key` and `localhost.crt` files are placed, run the command:

```
openssl pkcs12 -export -out localhost.pfx -inkey localhost.key -in localhost.crt -password pass:zaq1@WSX
```

Replace `zaq1@WSX` with your actual password for the `.pfx` file.

### 4. Setup listening for port

Copy `localhost.pfx` file to `OAuthServer` (same directory for `.csproj`)
also copy it to CitiesService/CitiesService.Api/cert/localhost.pfx

In `Program.cs` file of `OAuthServer`, use this code:

```
// Configure Kestrel to use HTTPS
builder.WebHost.ConfigureKestrel(serverOptions =>
{
  // you can use any port, 443 is an example port (default port for https)
  serverOptions.Listen(IPAddress.Any, 443, listenOptions =>
	{
    //if file is located in root directory
		listenOptions.UseHttps("localhost.pfx", "zaq1@WSX"); //use your .pfx password

    //if file is located in 'cert' directory
    //listenOptions.UseHttps("cert/localhost.pfx", "zaq1@WSX");
	});

  // Configure Kestrel to use HTTP on port 80
  serverOptions.Listen(IPAddress.Any, 80);
});
```

---

In `.csproj` include:

If in root directory

```
<ItemGroup>
  <None Update="localhost.pfx">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

If in `cert` directory

```
  <ItemGroup>
    <Folder Include="cert\" />
    <None Update="cert\localhost.pfx">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
```

### 5. Configure `Dockerfile`

```
# Use the appropriate ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set Current Director to '/app'
WORKDIR /app

# Copy the compiled app to the container
COPY deploy .

# Copy the .pfx certificate into the container
COPY localhost.pfx /https/
# If in 'cert' directory:
# COPY cert/localhost.pfx /https/

# Set environment variables
ENV ASPNETCORE_URLS=https://+:443;http://+:80
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="zaq1@WSX"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.pfx

# Set the entry point to run your app
ENTRYPOINT ["dotnet", "OAuthServer.dll"]
```

### 5. Configure `docker-compose.yml`

```
version: '3.8'

services:
  oauthserver:
    image: oauthserver
    container_name: oauthserver
    hostname: oauthserver
    restart: unless-stopped
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_Kestrel__Certificates__Default__Password=zaq1@WSX
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.pfx
    ports:
      - 8079:443
      - 8078:80
    volumes:
      - ./Authorization/OAuthServer/localhost.pfx:/https/localhost.pfx  # Mount the .pfx file into the container
    networks:
      - default
      - overlaynetwork
networks:
  default:
    driver: bridge
  overlaynetwork:
    name: overlaynetwork
    external: true
```

---

For reference look up `OAuthServer` or `CitiesService.Api` projects

---

Remember to forward appropriate ports (add to `iptables`)
