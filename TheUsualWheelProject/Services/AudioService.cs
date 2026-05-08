using Plugin.Maui.Audio;
using System.IO;
using System.Threading.Tasks;

namespace TheUsualWheelProject.Services;

public class AudioService
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _bgmPlayer;
    private IAudioPlayer? _tickPlayer;
    private bool _isTickInitialized = false;
    
    public double BgmVolume { get; private set; } = 0.75;
    public double TickVolume { get; private set; } = 0.75;

    public AudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    private async Task<Stream> GetAudioStreamAsync(string fileName)
    {
        string localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        if (File.Exists(localPath))
        {
            return File.OpenRead(localPath);
        }

        return await FileSystem.OpenAppPackageFileAsync(fileName);
    }

    public async Task PlayBgmAsync(string fileName, bool loop = true)
    {
        StopBgm();

        Stream stream = await GetAudioStreamAsync(fileName);
        _bgmPlayer = _audioManager.CreatePlayer(stream);
        _bgmPlayer.Loop = loop;
        _bgmPlayer.Volume = BgmVolume;
        _bgmPlayer.Play();
    }

    public void StopBgm()
    {
        if (_bgmPlayer != null)
        {
            if (_bgmPlayer.IsPlaying)
            {
                _bgmPlayer.Stop();
            }
            _bgmPlayer.Dispose();
            _bgmPlayer = null;
        }
    }

    public async Task PlayTickAsync(string fileName = "tick.mp3")
    {
        if (!_isTickInitialized || _tickPlayer == null)
        {
            Stream stream = await GetAudioStreamAsync(fileName);
            _tickPlayer = _audioManager.CreatePlayer(stream);
            _tickPlayer.Volume = TickVolume;
            _isTickInitialized = true;
        }

        _tickPlayer.Volume = TickVolume;
        if (_tickPlayer.IsPlaying)
            _tickPlayer.Pause();

        _tickPlayer.Seek(0);
        _tickPlayer.Play();
    }

    public void SetBgmVolume(double volume)
    {
        BgmVolume = Math.Clamp(volume, 0.0, 1.0);
        if (_bgmPlayer != null) _bgmPlayer.Volume = BgmVolume;
    }

    public void SetTickVolume(double volume)
    {
        TickVolume = Math.Clamp(volume, 0.0, 1.0);
        if (_tickPlayer != null) _tickPlayer.Volume = TickVolume;
    }
}
