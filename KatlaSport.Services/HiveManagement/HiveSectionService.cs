using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KatlaSport.DataAccess;
using KatlaSport.DataAccess.ProductStoreHive;
using DbHiveSection = KatlaSport.DataAccess.ProductStoreHive.StoreHiveSection;

namespace KatlaSport.Services.HiveManagement
{
    /// <summary>
    /// Represents a hive section service.
    /// </summary>
    public class HiveSectionService : IHiveSectionService
    {
        private readonly IProductStoreHiveContext _context;
        private readonly IUserContext _userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HiveSectionService"/> class with specified <see cref="IProductStoreHiveContext"/> and <see cref="IUserContext"/>.
        /// </summary>
        /// <param name="context">A <see cref="IProductStoreHiveContext"/>.</param>
        /// <param name="userContext">A <see cref="IUserContext"/>.</param>
        public HiveSectionService(IProductStoreHiveContext context, IUserContext userContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userContext = userContext ?? throw new ArgumentNullException();
        }

        /// <inheritdoc/>
        public async Task<List<HiveSectionListItem>> GetHiveSectionsAsync()
        {
            var dbHiveSections = await _context.Sections.OrderBy(s => s.Id).ToArrayAsync();
            var hiveSections = dbHiveSections.Select(s => Mapper.Map<HiveSectionListItem>(s)).ToList();
            return hiveSections;
        }

        /// <inheritdoc/>
        public async Task<HiveSection> GetHiveSectionAsync(int hiveSectionId)
        {
            var dbHiveSections = await _context.Sections.Where(s => s.Id == hiveSectionId).ToArrayAsync();
            if (dbHiveSections.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            return Mapper.Map<DbHiveSection, HiveSection>(dbHiveSections[0]);
        }

        /// <inheritdoc/>
        public async Task<List<HiveSectionListItem>> GetHiveSectionsAsync(int hiveId)
        {
            var dbHiveSections = await _context.Sections.Where(s => s.StoreHiveId == hiveId).OrderBy(s => s.Id).ToArrayAsync();
            var hiveSections = dbHiveSections.Select(s => Mapper.Map<HiveSectionListItem>(s)).ToList();
            return hiveSections;
        }

        /// <inheritdoc/>
        public async Task SetStatusAsync(int hiveSectionId, bool deletedStatus)
        {
            var dbHivesSections = await _context.Sections.Where(s => hiveSectionId == s.Id).ToArrayAsync();

            if (dbHivesSections.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            var dbHivesSection = dbHivesSections[0];
            if (dbHivesSection.IsDeleted != deletedStatus)
            {
                dbHivesSection.IsDeleted = deletedStatus;
                dbHivesSection.LastUpdated = DateTime.UtcNow;
                dbHivesSection.LastUpdatedBy = _userContext.UserId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<HiveSection> CreateHiveSectionAsync(UpdateHiveSectionRequest createRequest)
        {
            var dbHives = await _context.Sections.Where(h => h.Code == createRequest.Code).ToArrayAsync();
            if (dbHives.Length > 0)
            {
                throw new RequestedResourceHasConflictException("code");
            }

            var dbHiveSection = Mapper.Map<UpdateHiveSectionRequest, DbHiveSection>(createRequest);
            dbHiveSection.StoreHiveId = 1;
            dbHiveSection.CreatedBy = _userContext.UserId;
            dbHiveSection.LastUpdatedBy = _userContext.UserId;
            _context.Sections.Add(dbHiveSection);

            await _context.SaveChangesAsync();

            return Mapper.Map<HiveSection>(dbHiveSection);
        }

        public async Task<HiveSection> UpdateHiveSectionAsync(int hiveSectionId, UpdateHiveSectionRequest updateRequest)
        {
            var dbHivesSections = await _context.Sections.Where(p => p.Code == updateRequest.Code && p.Id != hiveSectionId).ToArrayAsync();
            if (dbHivesSections.Length > 0)
            {
                throw new RequestedResourceHasConflictException("code");
            }

            dbHivesSections = await _context.Sections.Where(p => p.Id == hiveSectionId).ToArrayAsync();
            if (dbHivesSections.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            var dbHiveSection = dbHivesSections[0];

            Mapper.Map(updateRequest, dbHiveSection);
            dbHiveSection.LastUpdatedBy = _userContext.UserId;

            await _context.SaveChangesAsync();

            return Mapper.Map<HiveSection>(dbHiveSection);
        }

        public async Task DeleteHiveSectionAsync(int hiveSectionId)
        {
            var dbHiveSections = await _context.Sections.Where(p => p.Id == hiveSectionId).ToArrayAsync();
            if (dbHiveSections.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            var dbHiveSection = dbHiveSections[0];
            if (dbHiveSection.IsDeleted == false)
            {
                throw new RequestedResourceHasConflictException();
            }

            _context.Sections.Remove(dbHiveSection);
            await _context.SaveChangesAsync();
        }
    }
}
