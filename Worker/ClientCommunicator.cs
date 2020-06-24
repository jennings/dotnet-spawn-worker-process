using System;
using System.IO;
using System.IO.Pipes;
using Newtonsoft.Json;

namespace Worker
{
    class ClientCommunicator : IDisposable
    {
        static readonly JsonSerializer _serializer = new JsonSerializer();

        readonly AnonymousPipeClientStream _in;
        readonly AnonymousPipeClientStream _out;

        public ClientCommunicator(AnonymousPipeClientStream inStream, AnonymousPipeClientStream outStream)
        {
            _in = inStream;
            _out = outStream;
        }

        public static ClientCommunicator OpenPipes(string inPipeId, string outPipeId)
        {
            var inStream = new AnonymousPipeClientStream(PipeDirection.In, inPipeId);
            var outStream = new AnonymousPipeClientStream(PipeDirection.Out, outPipeId);
            return new ClientCommunicator(inStream, outStream);
        }

        public void Dispose()
        {
            _in.Dispose();
            _out.Dispose();
        }

        public T Read<T>()
        {
            using (var reader = new StreamReader(_in))
            using (var json = new JsonTextReader(reader))
            {
                return _serializer.Deserialize<T>(json);
            }
        }

        public void Write(object obj)
        {
            using (var writer = new StreamWriter(_out))
            using (var json = new JsonTextWriter(writer))
            {
                _serializer.Serialize(json, obj);
                json.Flush();
            }
        }
    }
}
