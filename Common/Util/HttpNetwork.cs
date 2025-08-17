namespace EggLink.DanhengServer.Util;

public static class HttpNetwork
{
    public static async ValueTask<(int, string?)> SendGetRequest(string url)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return ((int)response.StatusCode, content);
        }
        catch (Exception ex)
        {
            return (500, ex.Message);
        }
    }
}