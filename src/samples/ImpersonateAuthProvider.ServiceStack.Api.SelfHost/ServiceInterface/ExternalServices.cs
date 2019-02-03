namespace ServiceStack.Api.SelfHost.ServiceInterface
{
    using ServiceStack;
    using ServiceModel;

    public class MyServices : Service
    {
        [RequiredRole("Manager")]
        [RequiredPermission("CanBuyStuff")]        
        public object Any(Hello request)
        {
            var session = this.GetSession();

            if (session.HasPermission( "CanSeeAllOrders", null))
            {
                return new HelloResponse
                {
                    Result = $"Whoooooaaaa! {request.Name}, you must be a big deal as you have the CanSeeAllOrders permission"
                };
            }
            else
            {
                return new HelloResponse
                {
                    Result = $"Hello, {request.Name} I'm in a separate Service Stack Instance!"
                };
            }
        }
    }
}