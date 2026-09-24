using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Migrators;
using uSync.Migrations.Core.Migrators.Models;
using uSync.Migrations.Migrators.Core;
//using UmbConstants = Umbraco.Cms.Core.Constants;

namespace MyMigrations.S6NestedContentMigrator
{
    // S6 THIS IS NOT BEING USED...OR AT LEAST ISN'T FUNCTIONAL...SEE S6NestedContentMigration.cs instead

    // S6 We just want to instruct usync.Migrations that certain properties are in fact NestedContent despite having custom editor definitions in App_Plugins package.manifest
    [SyncMigrator("Umbraco.NestedContent")] // Other options available too
    [SyncMigratorVersion(8)]
    internal class S6NestedContentMigrator : NestedContentMigrator //: SyncPropertyMigratorBase
    {
        private readonly ILogger<S6NestedContentMigrator> log;
        private const string TargetContentTypeAlias = "s6NestedContentAddressDetails"; // TODO S6 if this works we'll need to add entries for ALL nested content property aliases that required conversion

        public S6NestedContentMigrator(ILogger<S6NestedContentMigrator> log)
        {
            this.log = log;
        }

        public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
        {
            if (string.IsNullOrWhiteSpace(contentProperty.Value))
                return string.Empty;

            var rowValues = JsonConvert.DeserializeObject<List<NestedContentRowValue>>(contentProperty.Value);
            if (rowValues == null || !rowValues.Any())
                return contentProperty.Value;

            foreach (var row in rowValues)
            {
                // Only process rows that match our target content type
                if (!string.Equals(row.ContentTypeAlias, TargetContentTypeAlias, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (row.Id == Guid.Empty)
                    row.Id = Guid.NewGuid();

                // Apply our custom converter logic to each raw property value
                foreach (var property in row.RawPropertyValues)
                {
                    var editorAlias = context.ContentTypes.GetEditorAliasByTypeAndProperty(
                        row.ContentTypeAlias, property.Key);
                    if (editorAlias == null) continue;

                    try
                    {
                        // Look up the migrator for this specific property editor
                        var migrator = context.Migrators.TryGetMigrator(editorAlias.OriginalEditorAlias);
                        if (migrator != null)
                        {
                            row.RawPropertyValues[property.Key] = migrator.GetContentValue(
                                new SyncMigrationContentProperty(
                                    row.ContentTypeAlias,
                                    property.Key,
                                    row.ContentTypeAlias,
                                    property.Value?.ToString() ?? string.Empty),
                                context);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning(
                            "Error converting property {Property} on {ContentType}: {Error}",
                            property.Key, row.ContentTypeAlias, ex.Message);
                    }
                }
            }

            return JsonConvert.SerializeObject(rowValues, Formatting.Indented);
        }
    }
}
