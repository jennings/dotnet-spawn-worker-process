using System;
using System.IO;
using System.IO.Pipes;
using Newtonsoft.Json;

namespace Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                throw new Exception("Must provide 2 arguments");
            }

            var inPipeId = args[0];
            var outPipeId = args[1];

            using (var inPipe = new AnonymousPipeClientStream(PipeDirection.In, inPipeId))
            using (var outPipe = new AnonymousPipeClientStream(PipeDirection.Out, outPipeId))
            {
                Console.WriteLine("WORKER: Opened streams");

                var serializer = new JsonSerializer();
                GreeterJobDescription description;
                using (var reader = new StreamReader(inPipe))
                using (var json = new JsonTextReader(reader))
                {
                    description = serializer.Deserialize<GreeterJobDescription>(json);
                }

                var result = new GreeterJob(description).Run();

                using (var writer = new StreamWriter(outPipe))
                using (var json = new JsonTextWriter(writer))
                {
                    serializer.Serialize(json, result);
                    json.Flush();
                }
            }

            Console.WriteLine("WORKER: Done");
        }
    }
}
