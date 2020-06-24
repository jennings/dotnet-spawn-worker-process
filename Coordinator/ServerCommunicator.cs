using System;
using System.IO;
using System.IO.Pipes;
using Newtonsoft.Json;

namespace Coordinator
{
    class ServerCommunicator : IDisposable
    {
        static readonly JsonSerializer _serializer = new JsonSerializer();

        readonly AnonymousPipeServerStream _out;
        readonly AnonymousPipeServerStream _in;

        public ServerCommunicator()
        {
            _out = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            _in = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
        }

        public void Dispose()
        {
            _out.Dispose();
            _in.Dispose();
        }

        public (string, string) GetClientHandleStrings()
        {
            return (_out.GetClientHandleAsString(), _in.GetClientHandleAsString());
        }

        public void DisposeLocalClientHandles()
        {
            _out.DisposeLocalCopyOfClientHandle();
            _in.DisposeLocalCopyOfClientHandle();
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
