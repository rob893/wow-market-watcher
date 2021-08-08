using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Hangfire.JobsLogger;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Services;

using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.BackgroundJobs
{
    public sealed class PullRealmDataBackgroundJob
    {
        private readonly IBlizzardService blizzardService;

        private readonly DataContext dbContext;

        private readonly IMapper mapper;

        private readonly ICorrelationIdService correlationIdService;

        private readonly ILogger<PullRealmDataBackgroundJob> logger;

        public PullRealmDataBackgroundJob(
            IBlizzardService blizzardService,
            DataContext dbContext,
            IMapper mapper,
            ICorrelationIdService correlationIdService,
            ILogger<PullRealmDataBackgroundJob> logger)
        {
            this.blizzardService = blizzardService ?? throw new ArgumentNullException(nameof(blizzardService));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string CorrelationId => this.correlationIdService.CorrelationId;

        public async Task PullRealmData(PerformContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var sourceName = GetSourceName();
            var hangfireJobId = context.BackgroundJob.Id;
            this.correlationIdService.CorrelationId = $"{hangfireJobId}-{Guid.NewGuid()}";

            // Can't use tags yet. Issue with Pomelo ef core MySQL connector
            // context.AddTags(nameof(PullRealmDataBackgroundJob));

            var metadata = new Dictionary<string, object> { { LogMetadataFields.BackgroundJobName, nameof(PullRealmDataBackgroundJob) } };

            this.logger.LogInformation(hangfireJobId, sourceName, this.CorrelationId, $"{nameof(PullRealmDataBackgroundJob)} started.", metadata);

            try
            {
                var totalEntriesUpdated = 0;

                var currentConnectedRealms = (await this.dbContext.ConnectedRealms.ToListAsync()).ToDictionary(x => x.Id);
                var currentRealms = (await this.dbContext.Realms.ToListAsync()).ToDictionary(r => r.Id);

                var blizzardRealms = await this.blizzardService.GetAllConnectedRealmsAsync();

                this.logger.LogDebug(hangfireJobId, sourceName, this.CorrelationId, $"Starting to process connected realms.", metadata);
                foreach (var connectedRealm in blizzardRealms)
                {
                    try
                    {
                        if (!currentConnectedRealms.ContainsKey(connectedRealm.Id))
                        {
                            this.logger.LogInformation(hangfireJobId, sourceName, this.CorrelationId, $"Connected realm {connectedRealm.Id} not in database. Adding it.", metadata);
                            this.dbContext.ConnectedRealms.Add(this.mapper.Map<ConnectedRealm>(connectedRealm));
                            totalEntriesUpdated += await this.dbContext.SaveChangesAsync();
                            continue;
                        }

                        var currentConnectedRealm = currentConnectedRealms[connectedRealm.Id];
                        var pulledRealms = connectedRealm.Realms.ToDictionary(r => r.Id);
                        var currentRealmsForConnectedRealm = currentConnectedRealm.Realms.ToDictionary(r => r.Id);

                        if (connectedRealm.Population.Name.EnUS != currentConnectedRealm.Population)
                        {
                            if (string.IsNullOrWhiteSpace(connectedRealm.Population.Name.EnUS))
                            {
                                this.logger.LogWarning(
                                    hangfireJobId,
                                    sourceName,
                                    this.CorrelationId,
                                    $"Population for connected realm {connectedRealm.Id} of '{connectedRealm.Population.Name.EnUS}' pulled from Blizzard is null or white space. Skipping.",
                                    metadata);
                            }
                            else
                            {
                                this.logger.LogInformation(
                                    hangfireJobId,
                                    sourceName,
                                    this.CorrelationId,
                                    $"Population for connected realm {connectedRealm.Id} has changed from {currentConnectedRealm.Population} to {connectedRealm.Population.Name.EnUS}. Updating it.",
                                    metadata);
                                currentConnectedRealm.Population = connectedRealm.Population.Name.EnUS;
                                totalEntriesUpdated += await this.dbContext.SaveChangesAsync();
                            }
                        }

                        foreach (var realmId in currentRealmsForConnectedRealm.Keys)
                        {
                            if (!pulledRealms.ContainsKey(realmId))
                            {
                                this.logger.LogInformation(
                                    hangfireJobId,
                                    sourceName,
                                    this.CorrelationId,
                                    $"Realm {realmId} in database for connected realm {connectedRealm.Id} but not in Blizzard response. Removing it.",
                                    metadata);
                                this.dbContext.Realms.Remove(currentRealmsForConnectedRealm[realmId]);
                                totalEntriesUpdated += await this.dbContext.SaveChangesAsync();
                                currentRealms.Remove(realmId);
                            }
                        }

                        foreach (var realmId in pulledRealms.Keys)
                        {
                            if (!currentRealmsForConnectedRealm.ContainsKey(realmId))
                            {
                                this.logger.LogInformation(
                                    hangfireJobId,
                                    sourceName,
                                    this.CorrelationId,
                                    $"Realm {realmId} not in database for connected realm {connectedRealm.Id}. Adding it.",
                                    metadata);

                                if (currentRealms.ContainsKey(realmId))
                                {
                                    var realm = currentRealms[realmId];
                                    realm.ConnectedRealm = currentConnectedRealm;
                                    realm.ConnectedRealmId = currentConnectedRealm.Id;
                                    totalEntriesUpdated += await this.dbContext.SaveChangesAsync();
                                }
                                else
                                {
                                    var newRealm = this.mapper.Map<Realm>(pulledRealms[realmId]);
                                    newRealm.ConnectedRealm = currentConnectedRealm;
                                    newRealm.ConnectedRealmId = currentConnectedRealm.Id;
                                    this.dbContext.Realms.Add(newRealm);
                                    totalEntriesUpdated += await this.dbContext.SaveChangesAsync();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(hangfireJobId, sourceName, this.CorrelationId, $"Unable to process connected realm {connectedRealm.Id}. Reason: {ex}", metadata);
                    }
                }
                this.logger.LogDebug(hangfireJobId, sourceName, this.CorrelationId, $"Processing connected realms complete.", metadata);

                this.logger.LogInformation(hangfireJobId, sourceName, this.CorrelationId, $"{nameof(PullRealmDataBackgroundJob)} complete. {totalEntriesUpdated} entries updated.", metadata);
            }
            catch (OperationCanceledException ex)
            {
                this.logger.LogWarning(hangfireJobId, sourceName, this.CorrelationId, $"{nameof(PullRealmDataBackgroundJob)} canceled. Reason: {ex}", metadata);
            }
            catch (Exception ex)
            {
                this.logger.LogError(hangfireJobId, sourceName, this.CorrelationId, $"{nameof(PullRealmDataBackgroundJob)} failed. Reason: {ex}", metadata);
                throw new BackgroundJobClientException(ex.Message, ex);
            }
        }
    }
}