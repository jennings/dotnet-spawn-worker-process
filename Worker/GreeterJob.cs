namespace Worker
{
    public class GreeterJobDescription
    {
        public string Name { get; set; }
    }

    public class GreeterResult
    {
        public string Greeting { get; set; }
    }

    public class GreeterJob
    {
        private readonly GreeterJobDescription _description;

        public GreeterJob(GreeterJobDescription description)
        {
            _description = description;
        }

        public GreeterResult Run()
        {
            return new GreeterResult
            {
                Greeting = $"Hello, {_description.Name}!",
            };
        }
    }
}
