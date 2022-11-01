using Microsoft.Graph;

namespace LunchBot;

public partial class UserFinder
{
    private class UserMailComparer : IComparer<User>
    {
        public int Compare(User x, User y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (ReferenceEquals(null, y))
            {
                return 1;
            }

            if (ReferenceEquals(null, x))
            {
                return -1;
            }

            string xDomain = x.Mail.Split('@')[1];
            string yDomain = y.Mail.Split('@')[1];

            if (xDomain == yDomain)
            {
                return 0;
            }

            bool xIsRedEngine = xDomain.Contains("red-engine");
            bool yIsRedEngine = yDomain.Contains("red-engine");

            if (xIsRedEngine && yIsRedEngine)
            {
                return 0;
            }

            if (xIsRedEngine && !yIsRedEngine)
            {
                return 1;
            }

            return -1;
        }
    }
}