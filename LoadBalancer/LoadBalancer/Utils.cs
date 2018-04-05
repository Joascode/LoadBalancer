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
        internal static List<string> GetAlgorithms(Type algorithm)
        {
            //Type algorithm = typeof(ILBAlgorithm);
            List<string> listAlgorithmNames = new List<string>();

            listAlgorithmNames = Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(t => algorithm.IsAssignableFrom(t))
                .Where(t => t != algorithm)
                .Select(t => t.Name)
                .ToList();

            return listAlgorithmNames;
        }

        internal static List<string> GetServerAffinitys(Type serverAffinity)
        {
            //Type algorithm = typeof(IServerAffinity<string, string>);
            /*List<string> listAlgorithmNames = new List<string>();

            listAlgorithmNames = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => serverAffinity.IsAssignableFrom(t))
                .Where(t => t != serverAffinity)
                .Select(t => t.Name)
                .ToList();

            return listAlgorithmNames;*/
            string pathname = @"E:\ProgrammingProjects\WIN\Load_Balancer\LoadBalancer\Assemblies";
            //string path = Path.GetDirectoryName(pathname);
            var files = Directory.GetFiles(pathname);

            List<string> names = new List<string>();

            foreach (var file in files)
            {
                //Console.WriteLine(file);
                var dll = Assembly.LoadFile(file); //load valid assembly into the main AppDomain
                                                   //do something
                var typedll = dll.GetTypes()
                    .Where(t => t.GetInterfaces().Any(x => 
                        x.IsGenericType &&
                        x.GetGenericTypeDefinition() == serverAffinity
                    ))
                    .Select(t => t.Name)
                    .ToList();

                typedll.ForEach(n => Console.WriteLine(n));

                typedll.ForEach(n => names.Add(n));
            }

            // REMOVAL OF BACKTICKS cmbBoxCallFlowStage.Items.Add(t.Name.Substring(0, t.Name.Length - 2));

            /*return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => type.IsAssignableFrom(t))
                .Where(t => t != type)
                .Select(t => t.Name)
                .ToList();*/
            return names;
        }

        internal static List<string> GetAssemblyNamesForTypes(Type type)
        {
            string pathname = @"E:\ProgrammingProjects\WIN\Load_Balancer\Assemblies";
            //string path = Path.GetDirectoryName(pathname);
            var files = Directory.GetFiles(pathname);

            List<string> names = new List<string>();

            foreach (var file in files)
            {
                Console.WriteLine(file);
                var dll = Assembly.LoadFile(file); //load valid assembly into the main AppDomain
                                                   //do something
                var typedll = dll.GetTypes()
                    .Where(t => type.IsAssignableFrom(t))
                    .Where(t => t != type)
                    .Select(t => t.Name)
                    .ToList();

                typedll.ForEach(n => Console.WriteLine(n));

                typedll.ForEach(n => names.Add(n));
            }

            /*return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => type.IsAssignableFrom(t))
                .Where(t => t != type)
                .Select(t => t.Name)
                .ToList();*/
            return names;
        }
    }
}
