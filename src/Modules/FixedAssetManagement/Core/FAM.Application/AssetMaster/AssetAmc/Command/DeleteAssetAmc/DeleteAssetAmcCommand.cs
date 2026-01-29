using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Command.DeleteAssetAmc
{
    public class DeleteAssetAmcCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}