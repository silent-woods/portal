using FluentMigrator;
using App.Core.Domain.Common;
using App.Core.Domain.Catalog;
using App.Core.Domain.Configuration;
using App.Core.Infrastructure;
using App.Data;
using App.Data.Migrations;
using App.Services.Configuration;

namespace App.Web.Framework.Migrations.UpgradeTo450
{
    [NopMigration("2021-04-23 00:00:00", "4.50.0", UpdateMigrationType.Settings, MigrationProcessType.Update)]
    public class SettingMigration : MigrationBase
    {
        /// <summary>Collect the UP migration expressions</summary>
        public override void Up()
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //do not use DI, because it produces exception on the installation process
            var settingRepository = EngineContext.Current.Resolve<IRepository<Setting>>();
            var settingService = EngineContext.Current.Resolve<ISettingService>();

            //miniprofiler settings are moved to appSettings
            settingRepository
                .Delete(setting => setting.Name == "storeinformationsettings.displayminiprofilerforadminonly" ||
                    setting.Name == "storeinformationsettings.displayminiprofilerinpublicstore");

            //#4363
            var commonSettings = settingService.LoadSetting<CommonSettings>();

            if (!settingService.SettingExists(commonSettings, settings => settings.ClearLogOlderThanDays))
            {
                commonSettings.ClearLogOlderThanDays = 0;
                settingService.SaveSetting(commonSettings, settings => settings.ClearLogOlderThanDays);
            }

            //#5551
            var catalogSettings = settingService.LoadSetting<CatalogSettings>();

            if (!settingService.SettingExists(catalogSettings, settings => settings.EnableSpecificationAttributeFiltering))
            {
                catalogSettings.EnableSpecificationAttributeFiltering = true;
                settingService.SaveSetting(catalogSettings, settings => settings.EnableSpecificationAttributeFiltering);
            }
        }

        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}