namespace Auth.Extension
{
    public class EndpointOptions
    {
        internal List<Type> types = new List<Type>();

        public IReadOnlyCollection<Type> Types { get { return types; } }

        public List<string>? GetPrefixes { get; set; } = new List<string>();

        public List<string>? PostPrefixes { get; set; } = new List<string>();

        public List<string>? PutPrefixes { get; set; } = new List<string>();

        public List<string>? DeletePrefixes { get; set; } = new List<string>();

        public List<string> ServiceNamePost { get; set; } = new List<string>();
    }
}
