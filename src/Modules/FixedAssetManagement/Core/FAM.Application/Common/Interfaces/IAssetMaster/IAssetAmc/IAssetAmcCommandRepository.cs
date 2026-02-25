using System.Linq.Expressions;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc
{
    public interface IAssetAmcCommandRepository
    {
          Task<int> CreateAsync(AssetAmc assetAmc);
          Task<int> UpdateAsync(int id,AssetAmc assetAmc);
          Task<int> DeleteAsync(int Id,AssetAmc assetAmc);
          Task<AssetAmc?> GetAlreadyAsync(Expression<Func<AssetAmc, bool>> predicate);
    }
}