namespace afs.jwt.abstractions
{
    public class JwtIssuerOptions
    {
        public int JwtExpirationMinutes { get; set; } = 2880; //60 * 48;
        public int JwtRefreshExpirationMinutes { get; set; } = 1440; //60 * 24;

        public virtual string JwtIssuer { get; set; }
        public virtual string JwtAudience { get; set;  }

        public virtual string MongoConnectionString { get; set;  } = "mongodb://localhost:27017/users";
    }
}