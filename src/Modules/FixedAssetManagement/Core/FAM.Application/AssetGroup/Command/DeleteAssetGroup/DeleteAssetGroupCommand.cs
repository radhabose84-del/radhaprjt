using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetGroup.Command.DeleteAssetGroup
{
    public class DeleteAssetGroupCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}