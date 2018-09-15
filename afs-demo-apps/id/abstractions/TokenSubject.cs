namespace afs.jwt.abstractions
{
    public class TokenSubject
    {
        public string Id { get; set; }
        public string Username { get; set; }

        public static TokenSubject Create(string username)
            => new TokenSubject { Id = System.Guid.NewGuid().ToString(), Username = username };
    }
}