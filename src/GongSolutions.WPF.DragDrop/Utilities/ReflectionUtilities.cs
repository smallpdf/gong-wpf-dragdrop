using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;


namespace GongSolutions.Wpf.DragDrop.Utilities
{
    public static class ReflectionUtilities   // Smallpdf
    {

        public static void Clone(object origin, object destination, List<string> propNamesToIgnore = null)
        {
            var properties = origin.GetType().GetRuntimeProperties();
            var destProperties = destination.GetType().GetRuntimeProperties();
            foreach (var property in properties)
            {
                if (propNamesToIgnore != null && propNamesToIgnore.Contains(property.Name))
                    continue;

                var destProperty = destProperties.Where(x => x.Name == property.Name).FirstOrDefault();
                if (destProperty != null)
                {
                    var value = property.GetValue(origin);
                    try
                    {
                        destProperty.SetValue(destination, value);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error copying property: " + ex.Message);
                    }
                }
            }
        }

    }
}
