using System.Reflection;
//  https://stackoverflow.com/a/949285
namespace VCG_Library
{
    public static class Reflectives
    {
        public static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }

        public static Type[] GetTypesInNamespace<T>(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal) && t.IsSubclassOf(typeof(T)))
                      .ToArray();
        }

        public static Type GetTypeInNameStace<T>(Assembly assembly, string name, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal) && t.Name == name && t.IsSubclassOf(typeof(T)))
                      .ToArray()[0];
        }

        public static Type GetCard(string name)
        {
            return
              Assembly.GetAssembly(typeof(VCG_Objects.Card)).GetTypes()
                      .Where(t => String.Equals(t.Namespace, "VCG_Objects.Cards", StringComparison.Ordinal) && t.Name == name && t.IsSubclassOf(typeof(VCG_Objects.Card))).FirstOrDefault();
        }

        public static Type[] GetCards()
        {
            return
              Assembly.GetAssembly(typeof(VCG_Objects.Card)).GetTypes()
                      .Where(t => String.Equals(t.Namespace, "VCG_Objects.Cards", StringComparison.Ordinal) && t.IsSubclassOf(typeof(VCG_Objects.Card))).ToArray();
        }
    }

}