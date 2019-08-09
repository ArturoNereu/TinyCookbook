using System;
using Bee.Core;
using Bee.Stevedore;
using NiceIO;

static class StevedoreUnityCecil
{
    public static NPath[] Paths => _paths.Value;
    
    static readonly Lazy<NPath[]> _paths = new Lazy<NPath[]>(() =>
    {
        var cecilArtifact = new StevedoreArtifact("unity-cecil");
        Backend.Current.Register(cecilArtifact);

        return new[]
        {
            cecilArtifact.Path.Combine("lib", "net40", "Unity.Cecil.dll"),
            cecilArtifact.Path.Combine("lib", "net40", "Unity.Cecil.Rocks.dll"),
            cecilArtifact.Path.Combine("lib", "net40", "Unity.Cecil.Mdb.dll"),
            cecilArtifact.Path.Combine("lib", "net40", "Unity.Cecil.Pdb.dll")
        };
    });
}
