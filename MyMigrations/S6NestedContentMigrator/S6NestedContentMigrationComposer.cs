using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using uSync.Migrations.Core.Composing;
using uSync.Migrations.Migrators.Core;

namespace MyMigrations.S6NestedContentMigrator
{
    [ComposeAfter(typeof(SyncMigrationsComposer))]
    public class S6NestedContentMigrationComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            /* S6 trying to implement our own composer to handle nested content properties that 
             * are implemented with customer property editors that usync.Migrations doesn't automatically
             * detect/process during conversions.
             */
            // S6 temp disable this so we can look at core NestedContentMigrator and see if/why it isn't seeing certain properties as NestedContent
            // But we don't want to REPLACE NestedContentMigrator...we want to use it for custom properties like S6AccountingAddressEditor that UM isn't detecting as NestedContent
            //builder.SyncPropertyMigrators().Replace<NestedContentMigrator, S6NestedContentMigrator>();
        }
    }
}
