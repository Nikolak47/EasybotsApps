using System.Security;

namespace ExceptionlessApp
{
    internal class ExceptionlessBotInfo
    {
        public string ProjectName { get; set; }
        public string ApiKey { get; set; }
        public SecureString ApiKeyAsSecureString;

        public ExceptionlessBotInfo(string projectName, string apiKey)
        {
            this.ProjectName = projectName;
            this.ApiKey = apiKey;
            this.ApiKeyAsSecureString = Encryption.ToSecureString(apiKey);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            ExceptionlessBotInfo botInfo = obj as ExceptionlessBotInfo;
            if (ReferenceEquals(null, botInfo))
                return false;

            if (ReferenceEquals(this, botInfo))
                return true;

            return (botInfo != null) && ((this.ProjectName == botInfo.ProjectName) || (this.ApiKey == botInfo.ApiKey));
        }

        public override int GetHashCode()
        {
            return (this.ProjectName + this.ApiKey).GetHashCode();
        }

        public override string ToString()
        {
            return this.ProjectName;
        }

        public static bool operator ==(ExceptionlessBotInfo a, ExceptionlessBotInfo b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(ExceptionlessBotInfo a, ExceptionlessBotInfo b)
        {
            return !Equals(a, b);
        }
    }
}