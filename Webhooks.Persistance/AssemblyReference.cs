using System.Reflection;

namespace Webhooks.Persistance;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}