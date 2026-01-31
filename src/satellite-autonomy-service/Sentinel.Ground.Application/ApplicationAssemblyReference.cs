using System.Reflection;

namespace Sentinel.Ground.Application;

public class ApplicationAssemblyReference
{
    public static Assembly Assembly => typeof(ApplicationAssemblyReference).Assembly;
}
