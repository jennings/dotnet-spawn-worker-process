using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Worker;

namespace Coordinator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("COORDINATOR: Started");

            var workerAssemblyLocation = args.Length > 0 ? args[0] : GetWorkerAssemblyLocation();

            Console.WriteLine("COORDINATOR: Worker assembly: {0}", workerAssemblyLocation);

            using (var outPipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable))
            using (var inPipe = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable))
            {
                var outPipeId = outPipe.GetClientHandleAsString();
                var inPipeId = inPipe.GetClientHandleAsString();

                Process process;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    process = LaunchAssemblyOnWindows(workerAssemblyLocation, outPipeId, inPipeId);
                }
                else
                {
                    process = LaunchAssemblyOnNonWindows(workerAssemblyLocation, outPipeId, inPipeId);
                }

                outPipe.DisposeLocalCopyOfClientHandle();
                inPipe.DisposeLocalCopyOfClientHandle();

                var serializer = new JsonSerializer();

                using (var writer = new StreamWriter(outPipe))
                using (var json = new JsonTextWriter(writer))
                {
                    serializer.Serialize(json, new GreeterJobDescription
                    {
                        Name = "Ada"
                    });
                    json.Flush();
                }

                var success = process.WaitForExit(5000);
                if (!success)
                {
                    Console.WriteLine("COORDINATOR: Worker did not exit in time");
                    Environment.ExitCode = 1;
                    return;
                }

                GreeterResult result;
                using (var reader = new StreamReader(inPipe))
                using (var json = new JsonTextReader(reader))
                {
                    result = serializer.Deserialize<GreeterResult>(json);
                }

                Console.WriteLine("COORDINATOR: Received greeting: {0}", result.Greeting);
            }

            Console.WriteLine("COORDINATOR: Done");
        }

        static string GetWorkerAssemblyLocation()
        {
            var location = typeof(GreeterJobDescription).Assembly.Location;
            return Path.ChangeExtension(location, "exe");
        }

        static Process LaunchAssemblyOnWindows(string assembly, params string[] args)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = assembly,
                Arguments = string.Join(" ", args),
            });
        }

        static Process LaunchAssemblyOnNonWindows(string assembly, params string[] args)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = string.Join(" ", new[] { assembly }.Concat(args)),
            });
        }
    }
}
