namespace afs.jwt.abstractions
{
    public interface ITokenValidator
    {
        bool IsTokenValid(string tokenString);
        string GetTokenJti(string tokenString);
    }
}