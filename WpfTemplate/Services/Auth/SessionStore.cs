using System.IO;
using System.Text.Json;
using WpfTemplate.Models;
using WpfTemplate.Services.Abstractions;

namespace WpfTemplate.Services.Auth;

public sealed class SessionStore : ISessionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    private readonly string _tokenFile;
    private readonly string _userFile;
    private string? _memoryToken;
    private UserInfo? _user;

    public SessionStore()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WpfTemplate");
        Directory.CreateDirectory(dir);
        _tokenFile = Path.Combine(dir, "session.token");
        _userFile = Path.Combine(dir, "session.user.json");
        _memoryToken = File.Exists(_tokenFile) ? File.ReadAllText(_tokenFile).Trim() : null;
        if (File.Exists(_userFile))
        {
            try
            {
                _user = JsonSerializer.Deserialize<UserInfo>(File.ReadAllText(_userFile), JsonOptions);
            }
            catch
            {
                _user = null;
            }
        }
    }

    public string? Token => _memoryToken;

    public UserInfo? User => _user;

    public event EventHandler? SessionCleared;

    public void SaveToken(string token, bool persist)
    {
        _memoryToken = token;
        if (persist)
        {
            File.WriteAllText(_tokenFile, token);
        }
        else if (File.Exists(_tokenFile))
        {
            File.Delete(_tokenFile);
        }
    }

    public void SaveUser(UserInfo user)
    {
        _user = user;
        File.WriteAllText(_userFile, JsonSerializer.Serialize(user, JsonOptions));
    }

    public void Clear(bool notify = true)
    {
        _memoryToken = null;
        _user = null;
        if (System.IO.File.Exists(_tokenFile))
        {
            System.IO.File.Delete(_tokenFile);
        }

        if (System.IO.File.Exists(_userFile))
        {
            System.IO.File.Delete(_userFile);
        }

        if (notify)
        {
            SessionCleared?.Invoke(this, EventArgs.Empty);
        }
    }
}
