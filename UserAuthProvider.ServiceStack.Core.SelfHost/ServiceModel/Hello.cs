namespace UserAuthProvider.ServiceStack.Core.SelfHost.ServiceModel
{
    using global::ServiceStack;

    [Route("/hello")]
    [Route("/hello/{Name}")]
    public class Hello : IReturn<HelloResponse>
    {
        public string Name { get; set; }
    }

    public class HelloResponse
    {
        public string Result { get; set; }
    }
}
