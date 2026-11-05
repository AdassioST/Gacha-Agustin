using SalYAlmas.Core.Meta;

namespace SalYAlmas.Tests
{
    /// <summary>Datos reales de la Fase 2 para las pruebas (se cargan una vez).</summary>
    public static class Phase2Kit
    {
        private static MetaDatabase _meta;

        public static MetaDataSources Sources() => MetaDataSources.FromDirectory(TestKit.DataDir());

        public static MetaDatabase Meta() => _meta ?? (_meta = MetaLoader.Load(Sources(), TestKit.RealDb()));
    }
}
