using System.Runtime.CompilerServices;

// Las pruebas (EditMode en Unity y Core.Tests con dotnet) preparan estados concretos con setters internos.
[assembly: InternalsVisibleTo("SalYAlmas.Tests.EditMode")]
[assembly: InternalsVisibleTo("Core.Tests")]
