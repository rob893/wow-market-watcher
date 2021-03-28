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
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Services;
using WoWMarketWatcher.Common.Extensions;
using static WoWMarketWatcher.Common.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.BackgroundJobs
{
    public class PullRealmDataBackgroundJob
    {
        private readonly IBlizzardService blizzardService;
        private readonly IConnectedRealmRepository realmRepository;
        private readonly IMapper mapper;
        private readonly ILogger<PullRealmDataBackgroundJob> logger;

        public PullRealmDataBackgroundJob(IBlizzardService blizzardService, IConnectedRealmRepository realmRepository, IMapper mapper, ILogger<PullRealmDataBackgroundJob> logger)
        {
            this.blizzardService = blizzardService ?? throw new ArgumentNullException(nameof(blizzardService));
            this.realmRepository = realmRepository ?? throw new ArgumentNullException(nameof(realmRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PullRealmData(PerformContext context)
        {
            var sourceName = GetSourceName();
            var hangfireJobId = context.BackgroundJob.Id;
            var correlationId = $"{hangfireJobId}-{Guid.NewGuid()}";

            // Can't use tags yet. Issue with Pomelo ef core MySQL connector
            // context.AddTags(nameof(PullRealmDataBackgroundJob));

            var metadata = new Dictionary<string, object> { { LogMetadataFields.BackgroundJobName, nameof(PullRealmDataBackgroundJob) } };

            this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"{nameof(PullRealmDataBackgroundJob)} started.", metadata);

            try
            {
                var currentRealms = (await this.realmRepository.EntitySet().Include(connectedRealm => connectedRealm.Realms).ToListAsync()).ToDictionary(x => x.Id);

                var blizzardRealms = await this.blizzardService.GetAllConnectedRealmsAsync(correlationId);


                foreach (var connectedRealm in blizzardRealms)
                {
                    if (!currentRealms.ContainsKey(connectedRealm.Id))
                    {
                        this.realmRepository.Add(this.mapper.Map<ConnectedRealm>(connectedRealm));
                        continue;
                    }

                    var currentConnectedRealm = currentRealms[connectedRealm.Id];
                    var pulledRealms = connectedRealm.Realms.ToDictionary(r => r.Id);
                    var currentRealmsForConnectedRealm = currentConnectedRealm.Realms.ToDictionary(r => r.Id);

                    foreach (var realmId in pulledRealms.Keys)
                    {
                        if (!currentRealmsForConnectedRealm.ContainsKey(realmId))
                        {
                            currentConnectedRealm.Realms.Add(this.mapper.Map<Realm>(pulledRealms[realmId]));
                        }
                    }

                    foreach (var realmId in currentRealmsForConnectedRealm.Keys)
                    {
                        if (!pulledRealms.ContainsKey(realmId))
                        {
                            currentConnectedRealm.Realms.Remove(currentRealmsForConnectedRealm[realmId]);
                        }
                    }
                }

                var entriesUpdated = await this.realmRepository.SaveChangesAsync();

                this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"{nameof(PullRealmDataBackgroundJob)} complete. {entriesUpdated} entries updated.", metadata);
            }
            catch (OperationCanceledException ex)
            {
                this.logger.LogWarning(hangfireJobId, sourceName, correlationId, $"{nameof(PullRealmDataBackgroundJob)} canceled. Reason: {ex}", metadata);
            }
            catch (Exception ex)
            {
                this.logger.LogError(hangfireJobId, sourceName, correlationId, $"{nameof(PullRealmDataBackgroundJob)} failed. Reason: {ex}", metadata);
                throw new BackgroundJobClientException(ex.Message, ex);
            }
        }
    }
}