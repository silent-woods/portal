using FluentMigrator;
using App.Data.Extensions;
using App.Data.Migrations;
using Nop.Plugin.MultiFactorAuth.GoogleAuthenticator.Domains;

namespace Nop.Plugin.MultiFactorAuth.GoogleAuthenticator.Migrations
{
    [NopMigration("2020/07/30 12:00:00", "Nop.Plugin.MultiFactorAuth.GoogleAuthenticator schema", MigrationProcessType.Installation)]
    public class GoogleAuthenticatorSchemaMigration : AutoReversingMigration
    {
        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            Create.TableFor<GoogleAuthenticatorRecord>();
        }
    }
}
