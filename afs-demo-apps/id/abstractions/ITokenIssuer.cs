using System.Threading.Tasks;

namespace afs.jwt.abstractions
{
    public interface ITokenIssuer
    {
        Task<string> GenerateToken(string username, string password, SigType sigType = SigType.Symmetric);
    }
}