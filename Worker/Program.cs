using System;

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

            using (var comm = ClientCommunicator.OpenPipes(inPipeId, outPipeId))
            {
                Console.WriteLine("WORKER: Opened streams");

                var description = comm.Read<GreeterJobDescription>();

                var result = new GreeterJob(description).Run();

                comm.Write(result);
            }

            Console.WriteLine("WORKER: Done");
        }
    }
}
