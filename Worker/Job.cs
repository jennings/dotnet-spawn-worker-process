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

    public class Greeter
    {
        private readonly GreeterJobDescription _description;

        public Greeter(GreeterJobDescription description)
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
