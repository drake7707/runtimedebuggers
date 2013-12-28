using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace RunTimeDebuggers.AssemblyExplorer
{
    public static class ILGeneratorHelper
    {

        public static void SetField(this ILGenerator ilGen, string field, object value)
        {
            ilGen.GetType().GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                           .SetValue(ilGen, value);
        }

        public static T GetField<T>(this ILGenerator ilGen, string field)
        {
            return (T)ilGen.GetType().GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                           .GetValue(ilGen);
        }

    }
}
