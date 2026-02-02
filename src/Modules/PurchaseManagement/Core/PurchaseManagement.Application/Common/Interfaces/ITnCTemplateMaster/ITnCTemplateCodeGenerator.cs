using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster
{
    public interface ITnCTemplateCodeGenerator
    {
          Task<string> GenerateAsync(int templateTypeId, string templateName, CancellationToken ct = default);
    }
}