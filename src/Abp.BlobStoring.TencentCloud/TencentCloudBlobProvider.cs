using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace Abp.BlobStoring.TencentCloud
{
    public class TencentCloudBlobProvider : BlobProviderBase, ITransientDependency
    {
        public override Task SaveAsync(BlobProviderSaveArgs args)
        {
            //TODO...
            throw new NotImplementedException();
        }

        public override Task<bool> DeleteAsync(BlobProviderDeleteArgs args)
        {
            //TODO...
            throw new NotImplementedException();
        }

        public override Task<bool> ExistsAsync(BlobProviderExistsArgs args)
        {
            //TODO...
            throw new NotImplementedException();
        }

        public override Task<Stream> GetOrNullAsync(BlobProviderGetArgs args)
        {
            //TODO...
            throw new NotImplementedException();
        }
    }
}
