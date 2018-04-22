using LBAlgorithm;
using ServerAffinity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    class Utils
    {
        private const string PATH_NAME = @"E:\ProgrammingProjects\WIN\Load_Balancer\LoadBalancer\Assemblies";

        internal static List<string> GetAlgorithms(Type algorithm)
        {
            List<string> listAlgorithmNames = new List<string>();

            listAlgorithmNames = Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(t => algorithm.IsAssignableFrom(t))
                .Where(t => t != algorithm)
                .Select(t => t.Name)
                .ToList();

            return listAlgorithmNames;
        }

        internal static Dictionary<string, string> GetServerAffinitys(Type serverAffinity)
        {
            var files = Directory.GetFiles(PATH_NAME);

            Dictionary<string, string> names = new Dictionary<string, string>();

            foreach (var file in files)
            {
                var dll = Assembly.LoadFile(file);

                var typedll = dll.GetTypes()
                    .Where(t => t.GetInterfaces().Any(x => 
                        x.IsGenericType &&
                        x.GetGenericTypeDefinition() == serverAffinity
                    ))
                    .Select(t => t.FullName)
                    .ToList();

                typedll.ForEach(n => names.Add(n, file));
            }

            return names;
        }

        internal static Dictionary<string, string> GetAssemblyNamesForType(Type type)
        {
            var files = Directory.GetFiles(PATH_NAME);

            Dictionary<string, string> names = new Dictionary<string, string>();

            foreach (var file in files)
            {
                var dll = Assembly.LoadFile(file);

                var typedll = dll.GetTypes()
                    .Where(t => type.IsAssignableFrom(t))
                    .Where(t => t != type)
                    .Select(t => t.FullName)
                    .ToList();

                typedll.ForEach(n => names.Add(n, file));
            }

            return names;
        }
    }
}
