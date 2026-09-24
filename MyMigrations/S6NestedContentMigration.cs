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
                // uysnc.Migrations is dropping these somehow too...it has stupid assembly logic that processes these and seems to filter them out somewhere
                //{ "S6AccountingAddressEditor", nameof(NestedToBlockListMigrator) },
                //{ "S6AccountingOrderLineItemsEditor", nameof(NestedToBlockListMigrator) },
                // Handle default NestedContent/Grid properties too (taken from BlockMigrationPlan.cs) in case we miss any or get their aliases slightly wrong
                { UmbConstants.PropertyEditors.Aliases.NestedContent, "NestedToBlockListMigrator" },
                { UmbConstants.PropertyEditors.Aliases.Grid, "GridToBlockGridMigrator" }
            },
            // S6 giving up and just trying to target explicit properties instead like a caveman (maybe we can at least target by composition alias to reduce total number of entries?):
            PropertyMigrators = new Dictionary<string, string>
            {            
                // OR target a property on a specific content type only:
                /* S6 we also need to READD all properties that usync.Migrations was handling natively b/c this entire plan replaces that (ie customer stuff didn't change) >_< 
                 * ... must be a better way to merge them */
                { "s6CustomerGroup_billingAddresses", nameof(NestedToBlockListMigrator) },
                { "s6CustomerGroup_shippingAddresses", nameof(NestedToBlockListMigrator) },
                { "s6CustomerGroup_domains", nameof(NestedToBlockListMigrator) },
                { "S6Order_billingAddresses", nameof(NestedToBlockListMigrator) },
                { "S6Order_shippingAddresses", nameof(NestedToBlockListMigrator) },
                { "S6Order_lineItems", nameof(NestedToBlockListMigrator) },
                { "S6Order_layout", nameof(GridToBlockListMigrator) },
                { "S6InvoicePage_billingAddresses", nameof(NestedToBlockListMigrator) },
                { "S6InvoicePage_shippingAddresses", nameof(NestedToBlockListMigrator) },
                { "S6InvoicePage_lineItems", nameof(NestedToBlockListMigrator) },
                { "S6InvoicePage_layout", nameof(GridToBlockListMigrator) },
                { "s6Refund_billingAddresses", nameof(NestedToBlockListMigrator) },
                { "s6Refund_shippingAddresses", nameof(NestedToBlockListMigrator) },
                { "s6Refund_lineItems", nameof(NestedToBlockListMigrator) },
                { "s6Refund_layout", nameof(GridToBlockListMigrator) },
                { "s6Shipment_address", nameof(NestedToBlockListMigrator) },                
                { "s6Shipment_lineItems", nameof(NestedToBlockListMigrator) },
                { "s6Shipment_layout", nameof(GridToBlockListMigrator) },
            }
        };
    }
}
