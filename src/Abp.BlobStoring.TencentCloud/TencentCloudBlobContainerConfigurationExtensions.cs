﻿using System;
using Volo.Abp.BlobStoring;

namespace Abp.BlobStoring.TencentCloud
{
    public static class TencentCloudBlobContainerConfigurationExtensions
    {

        public static BlobContainerConfiguration UseTencentCloudBlobProvider(
            this BlobContainerConfiguration containerConfiguration,
            Action<TencentCloudBlobProviderConfiguration> configureAction)
        {
            containerConfiguration.ProviderType = typeof(TencentCloudBlobProvider);

            configureAction.Invoke(
                new TencentCloudBlobProviderConfiguration(containerConfiguration)
            );

            return containerConfiguration;
        }

        public static TencentCloudBlobProviderConfiguration GetTencentCloudBlobProviderConfiguration(
            this BlobContainerConfiguration containerConfiguration)
        {
            return new TencentCloudBlobProviderConfiguration(containerConfiguration);
        }
    }
}
