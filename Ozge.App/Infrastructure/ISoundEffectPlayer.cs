using System.Threading.Tasks;

namespace Ozge.App.Infrastructure;

public interface ISoundEffectPlayer
{
    Task PlayCorrectAsync();
    Task PlayIncorrectAsync();
    Task PlayCelebrationAsync();

    Task PreviewAsync(string? filePath);
}
