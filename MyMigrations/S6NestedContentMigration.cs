using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uSync.Migrations.Core.Composing;
using uSync.Migrations.Core.Configuration.Models;
using uSync.Migrations.Migrators.Optional;
using static uSync.BackOffice.uSyncConstants;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace MyMigrations
{
    // S6 apparently we can't target custom properties or editors without building an entire MIGRATION PLAN >_<
    internal class S6NestedContentMigration : ISyncMigrationPlan
    {

        private readonly SyncMigrationHandlerCollection _handlers;

        public S6NestedContentMigration(SyncMigrationHandlerCollection handlers)
         => _handlers = handlers;

        public int Order => 100;

        public string Name => "S6 Nested Content Migration Plan";

        public string Icon => "icon-brick color-blue";

        public string Description => "Because usync.Migrations is a pain to customize for even simple things like targeting a custom property editor.";

        public MigrationOptions Options => new()
        {
            SourceVersion = 8,
            Handlers = _handlers.SelectGroup(8, string.Empty),
            PreferredMigrators = new Dictionary<string, string>
            {
                // S6 These did not work as [SyncMigrator("S6AccountingAddressEditor")] annotations on the existing NestedToBlockListMIgrator so we have to write all this to associate them instead
                // usync.Migrations is dropping these somehow too...it has stupid assembly logic that processes these and seems to filter them out somewhere
                { "S6AccountingAddressEditor", nameof(NestedToBlockListMigrator) },
                { "S6AccountingOrderLineItemsEditor", nameof(NestedToBlockListMigrator) },
                /* Handle default NestedContent/Grid properties too (taken from BlockMigrationPlan.cs) in case we miss any or get their aliases slightly wrong
                 * ...but will these merge nicely with our custom migrator definitions below? (ie. will discount_rewards be processed or skipped?) */
                { UmbConstants.PropertyEditors.Aliases.NestedContent, "NestedToBlockListMigrator" },
                { UmbConstants.PropertyEditors.Aliases.Grid, "GridToBlockGridMigrator" }
            },
            // S6 giving up and just trying to target explicit properties instead like a caveman (maybe we can at least target by composition alias to reduce total number of entries?):
            PropertyMigrators = new Dictionary<string, string>
            {            
                // OR target a property on a specific content type only:
                /* S6 we also need to READD all properties that usync.Migrations was handling natively b/c this entire plan replaces that (ie customer stuff didn't change) >_< 
                 * ... must be a better way to merge them 
                 * After discussion, we don't want to trigger any UpdateDate changes on Accounting nodes (customers,orders,invoices,etc.) by importing 
                 * more usync files than we absolutely need. SO NestedContent-to-BlockList conversion will likely be limited to Discount nodes (remaining in Umbraco)
                 * or at the very least we need to migration all accounting data to the new accounting system BEFORE this usync.Migrations import is run.
                 * That also means we can probably use the default Migrator which naturally handles discounts and just scope the import to those nodes.
                 */
                { "s6CustomerGroup_billingAddresses", nameof(NestedToBlockListMigrator) }, // REQUIRED...native migrator does not identify this as NestedContent
                { "s6CustomerGroup_shippingAddresses", nameof(NestedToBlockListMigrator) }, // REQUIRED...native migrator does not identify this as NestedContent
                { "s6CustomerGroup_domains", nameof(NestedToBlockListMigrator) },
                { "S6Order_billingAddresses", nameof(NestedToBlockListMigrator) }, // REQUIRED...native migrator does not identify this as NestedContent
                { "S6Order_shippingAddresses", nameof(NestedToBlockListMigrator) }, // REQUIRED...native migrator does not identify this as NestedContent
                { "S6Order_lineItems", nameof(NestedToBlockListMigrator) },  // REQUIRED...native migrator does not identify this as NestedContent
                { "S6Order_layout", nameof(GridToBlockListMigrator) }, // Likely optional, native migrator can probably see this
                { "S6InvoicePage_billingAddresses", nameof(NestedToBlockListMigrator) }, // REQUIRED...native migrator does not identify this as NestedContent
                { "S6InvoicePage_shippingAddresses", nameof(NestedToBlockListMigrator) }, // REQUIRED...native migrator does not identify this as NestedContent
                { "S6InvoicePage_lineItems", nameof(NestedToBlockListMigrator) }, // REQUIRED...native migrator does not identify this as NestedContent
                { "S6InvoicePage_layout", nameof(GridToBlockListMigrator) },
                { "s6Refund_billingAddresses", nameof(NestedToBlockListMigrator) },
                { "s6Refund_shippingAddresses", nameof(NestedToBlockListMigrator) },
                { "s6Refund_lineItems", nameof(NestedToBlockListMigrator) }, // REQUIRED...native migrator does not identify this as NestedContent
                { "s6Refund_layout", nameof(GridToBlockListMigrator) },
                { "s6Shipment_address", nameof(NestedToBlockListMigrator) }, // REQUIRED...native migrator does not identify this as NestedContent
                { "s6Shipment_lineItems", nameof(NestedToBlockListMigrator) }, // REQUIRED...native migrator does not identify this as NestedContent
                { "s6Shipment_layout", nameof(GridToBlockListMigrator) },
                // S6 could leave discount properties OUT to ensure that PreferredMigrators definitions from above actually function along with our specific PropertyMigrators
                { "s6Discount_rewards", nameof(NestedToBlockListMigrator) },
                { "s6Discount_rules", nameof(NestedToBlockListMigrator) },
                { "s6DataStoreBundle_bundleItems", nameof(NestedToBlockListMigrator) },
                { "s6CompositionOpenGraph_ogType", nameof(NestedToBlockListMigrator) }, // Hope compositions can target multiples, otherwise we'll need entries for each inherited property
            }
        };
    }
}
