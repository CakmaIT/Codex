using System;
using System.Linq;
using System.Reflection;

namespace Ozge.Assets;

public static class AssetCatalog
{
    private static Assembly Assembly => typeof(AssetCatalog).Assembly;

    public static Stream? OpenAssetStream(string relativePath)
    {
        var resourceName = Assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(relativePath.Replace("\\", ".").Replace("/", "."), StringComparison.OrdinalIgnoreCase));

        return resourceName is null ? null : Assembly.GetManifestResourceStream(resourceName);
    }
}
