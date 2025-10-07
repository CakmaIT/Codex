using System.Linq;
using Ozge.Core.Services;
using Ozge.Data.Context;

namespace Ozge.App.Services;

public class ScreenSelectionService : IScreenSelectionService
{
    private readonly OzgeDbContext _dbContext;
    private int? _projectorIndex;

    public ScreenSelectionService(OzgeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public int? ProjectorScreenIndex => _projectorIndex;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var setting = _dbContext.Settings.FirstOrDefault(s => s.Key == "projectorScreenIndex");
        if (setting is not null && int.TryParse(setting.Value, out var index))
        {
            _projectorIndex = index;
        }
    }

    public async Task SetProjectorScreenIndexAsync(int index, CancellationToken cancellationToken = default)
    {
        _projectorIndex = index;
        var setting = _dbContext.Settings.FirstOrDefault(s => s.Key == "projectorScreenIndex");
        if (setting is null)
        {
            setting = new Core.Domain.Entities.SettingEntity
            {
                Id = Guid.NewGuid(),
                ClassId = Guid.Empty,
                Key = "projectorScreenIndex",
                Value = index.ToString()
            };
            _dbContext.Settings.Add(setting);
        }
        else
        {
            setting.Value = index.ToString();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
