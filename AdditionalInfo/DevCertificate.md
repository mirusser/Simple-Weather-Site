# Dev certificate

If there is a warning that looks like this:
```
The ASP.NET Core developer certificate is not trusted. 
For information about trusting the ASP.NET Core developer certificate, 
see https://aka.ms/aspnet/https-trust-dev-cert
```

on Arch Linux it can be resolved by using these commands:

Create/trust the dev cert:
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

Ensure your system trust store is set up (Arch Linux):
```bash
sudo pacman -S --needed ca-certificates
sudo update-ca-trust
```

---

Alternatively you can just use http on dev environment:

```csharp
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
```


