# Weather Site

## Restoring client-side libraries (LibMan)

This project uses **LibMan (Library Manager)** to restore third-party client-side
JavaScript and CSS dependencies (e.g. jQuery, Bootstrap, AdminLTE, Select2).

The `wwwroot/lib` directory is **not committed** to the repository and must be
restored locally.

### Prerequisites
Ensure the LibMan CLI is installed:

```bash
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
```

Restore libraries

From the directory containing WeatherSite.csproj:
```bash
libman restore
```

This will download all required client-side dependencies into wwwroot/lib
based on the libman.json manifest.

Notes
- libman.json is located in the project root
- wwwroot/lib is generated and intentionally excluded from version control
- Run libman restore after cloning the repository or cleaning wwwroot/lib

---