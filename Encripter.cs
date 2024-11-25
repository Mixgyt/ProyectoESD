using System.Security.Cryptography;
using System.Text;

namespace Proyecto_Final_Estructura_De_Datos;

public class Encripter
{
    public static string EnconderSHA256(string input)
    {
        SHA256 newSHA = SHA256.Create();
        string result = "";
        var hash = newSHA.ComputeHash(Encoding.ASCII.GetBytes(input));
        foreach(var item in hash)
        {
            result += item.ToString("x2");
        }
        return result;
    }
}