using System.Threading.Tasks;

namespace JH.Applications
{
    public interface IFacebookService
    {
        Task<string> GetAccountAsync(string accessToken);
        Task PostOnWallAsync(string accessToken, string message);
    }

    public class FacebookService : IFacebookService
    {
        private readonly IFacebookClient _facebookClient;

        public FacebookService(IFacebookClient facebookClient)
        {
            _facebookClient = facebookClient;
        }

        public async Task<string> GetAccountAsync(string accessToken)
        {
            var result = await _facebookClient.GetAsync(Callback,
                accessToken, "223644934866041/feed", "");

            return result;
        }

        public async Task PostOnWallAsync(string accessToken, string message)
        {
            Task t = _facebookClient.PostAsync(accessToken, "me/feed", new { message });
            await t;
        }

        void Callback(string s)
        {

        }
 
    }
}