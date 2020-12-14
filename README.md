# Fluxor.Extensions.StoreLogger

A library to visualize actions in the browser console.
A middleware for the [Fluxor library](https://github.com/mrpmorris/Fluxor).

See an example [here](https://sebfischer83.github.io/Fluxor.Extensions.StoreLogger/counter).

![develop](https://github.com/sebfischer83/Traccaradora/workflows/continuous/badge.svg)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Fluxor.Extensions.StoreLogger)

### Usage
```csharp
builder.Services.AddFluxor(options => options.ScanAssemblies(currentAssembly).AddStoreLoggerMiddleware(new StoreLoggerOptions()));
```
