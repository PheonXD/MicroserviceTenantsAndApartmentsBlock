using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using TestTask;
using TestTask.DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Extensions;
using System.Linq;
using System.Diagnostics;
using System.Configuration;
using Microsoft.EntityFrameworkCore.Internal;
using NLog.Targets.Wrappers;

namespace testTask.Services
{
    public class EdnpointService : ApartmentBlocksWebApi.ApartmentBlocksWebApiBase
    {

        private readonly Context _context;

        public EdnpointService(Context context)
        {
            _context = context;
        }

        public async override Task<Empty> AddApartmentBlock(AddApartmentBlock_request request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.ApartmentBlockInfo.City)) throw new RpcException(new Status(StatusCode.InvalidArgument, "City: String is null or empty"));
            if (string.IsNullOrEmpty(request.ApartmentBlockInfo.Street)) throw new RpcException(new Status(StatusCode.InvalidArgument, "Street: String is null or empty"));

            var ab = _context.ApartmentBlocks;
            if (ab.Any(abInfo =>
                abInfo.City == request.ApartmentBlockInfo.City &&
                abInfo.Street == request.ApartmentBlockInfo.Street &&
                abInfo.Number == request.ApartmentBlockInfo.Number)) throw new RpcException(new Status(StatusCode.AlreadyExists, "Apartment block with given info already exists"));
            await ab.AddAsync(new ApartmentBlocksEntity()
            {
                Street = request.ApartmentBlockInfo.Street,
                Number = request.ApartmentBlockInfo.Number,
                City = request.ApartmentBlockInfo.City,
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Unknown, $"Unhandled exception: {ex.Message}"));
            }

            return new Empty();
        }

        public async override Task<Empty> AddTenant(AddTenant_request request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.TenantInfo.FirstName)) throw new RpcException(new Status(StatusCode.InvalidArgument, "FirstName: String is null or empty"));
            if (string.IsNullOrEmpty(request.TenantInfo.LastName)) throw new RpcException(new Status(StatusCode.InvalidArgument, "LastName: String is null or empty"));

            var tenants = _context.Tenants;
            if (tenants.Any(abInfo =>
                abInfo.FirstName == request.TenantInfo.FirstName &&
                abInfo.LastName == request.TenantInfo.LastName)) throw new RpcException(new Status(StatusCode.AlreadyExists, "Apartment block with given info already exists"));
            await tenants.AddAsync(new TenantsEntity()
            {
                LastName = request.TenantInfo.LastName,
                FirstName = request.TenantInfo.FirstName,
                Age = request.TenantInfo.Age
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Unknown, $"Unhandled exception: {ex.Message}"));
            }

            return new Empty();
        }

        public async override Task<Empty> ChangeTenantApartmentBlock(ChangeTenantApartmentBlock_request request, ServerCallContext context)
        {
            var tenants = _context.Tenants.AsNoTracking();
            var apartmentBlocks = _context.ApartmentBlocks.AsNoTracking();
            var accommodation = _context.AccommodationInfo;

            var accommodationInfo = new AccommodationInfoEntity()
            {
                TenantId = (uint)request.TenantInfo.GetId(tenants),
                ABID = (uint)request.ApartmentBlockInfo.GetId(apartmentBlocks)
            };

            if (accommodationInfo.TenantId == 0 && accommodationInfo.ABID == 0) throw new RpcException(new Status(StatusCode.NotFound, "Tenant or ApartmentBlock not found"));

            await accommodation.AddAsync(accommodationInfo);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Unknown, $"Unhandled exception: {ex.Message}"));
            }

            return new Empty();
        }

        public async override Task<Empty> DeleteTenant(DeleteTenant_request request, ServerCallContext context)
        {
            var tenants = _context.Tenants;
            var accommodation = _context.AccommodationInfo;

            var tenantsEntity = new TenantsEntity() { Id = request.TenantInfo.GetId(tenants) };
            if (tenantsEntity.Id == 0) throw new RpcException(new Status(StatusCode.NotFound, "Tenant not found"));

            tenants.Remove(tenantsEntity);

            var acInfo = accommodation.Where(ac => ac.TenantId == tenantsEntity.Id);
            if (acInfo.Count() > 0) accommodation.RemoveRange(acInfo);
                 
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Unknown, $"Unhandled exception: {ex.Message}"));
            }

            return new Empty();
        }


        public async override Task<Empty> DeleteApartmentBlock(DeleteApartmentBlock_request request, ServerCallContext context)
        {
            var tenants = _context.Tenants.AsNoTracking();
            var accommodation = _context.AccommodationInfo;
            var apartmentBlocks = _context.ApartmentBlocks;

            var apartmentBlockToDel = new ApartmentBlocksEntity() { Id = request.ApartmentBlockToDelete.GetId(apartmentBlocks) };
            var apartmentBlockToMove = new ApartmentBlocksEntity() { Id = request.ApartmentBlockReplaceTenantsTo.GetId(apartmentBlocks) };
            if (apartmentBlockToDel.Id == 0) throw new RpcException(new Status(StatusCode.NotFound, "ApartmentBlock not found"));

            apartmentBlocks.Remove(apartmentBlockToDel);

            if(apartmentBlockToMove.Id != 0)
                await accommodation.Where(ac => ac.ABID == apartmentBlockToDel.Id).ForEachAsync(ac => ac.ABID = (uint)apartmentBlockToMove.Id);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Unknown, $"Unhandled exception: {ex.Message}"));
            }

            return new Empty();
        }

        public async override Task<GetABWith_response> GetABWith_MaxCountTenants(Empty request, ServerCallContext context)
        {
            var accommodation = _context.AccommodationInfo.AsNoTracking();
            var apartmentBlocks = _context.ApartmentBlocks.AsNoTracking();
            var blockId = accommodation.GroupBy(ac => ac.ABID).ToArray().OrderByDescending(g => g.Count()).FirstOrDefault()?.Key;

            var abInfo = apartmentBlocks.Where(ab => ab.Id == blockId).FirstOrDefault();
            if (abInfo == null) return null;

            return new GetABWith_response() { 
                ApartmentBlockInfo = new ApartmentBlock() {
                    City = abInfo.City,
                    Street = abInfo.Street,
                    Number = abInfo.Number,
                    Id = (uint)abInfo.Id
                }
            };
        }

        public override Task GetABWith_OldestTenant(Empty request, IServerStreamWriter<GetABWith_response> responseStream, ServerCallContext context)
        {
            var view = _context.View_TenToABV.AsNoTracking();
            var apartments = view.Where(v => v.Age == view.Max(v => v.Age));
            try
            {
                foreach (var ap in apartments)
                {
                    if (context.CancellationToken.IsCancellationRequested) break;
                    responseStream.WriteAsync(new GetABWith_response()
                    {
                        ApartmentBlockInfo = new ApartmentBlock
                        {
                            City = ap.City,
                            Street = ap.Street,
                            Number = ap.Number,
                            Id = (uint)ap.Id
                        }
                    });
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Operation was cancelled");
            }
            return Task.CompletedTask;
        }

        public async override Task<ABStreet_response> GetCountOfTenantsInABStreet(ABStreet_request request, ServerCallContext context)
        {
            var view = _context.View_TenToABV.AsNoTracking();
            var res = view.Where(v=> v.City == request.ApartmentBlockStreet.City &&
                                    v.Street == request.ApartmentBlockStreet.Street &&
                                    v.Number == request.ApartmentBlockStreet.Number);
            if (res == null) return null;
            return new ABStreet_response() { Result = res.Count().ToString() };
        }

        public async override Task<ABStreet_response> GetAvgAgeInABStreet(ABStreet_request request, ServerCallContext context)
        {
            var view = _context.View_TenToABV.AsNoTracking();
            var tenants = view.Where(v => v.City == request.ApartmentBlockStreet.City &&
                                    v.Street == request.ApartmentBlockStreet.Street &&
                                    v.Number == request.ApartmentBlockStreet.Number);
            var avgAge = tenants.Average(t => t.Age);
            return new ABStreet_response() { Result = avgAge.ToString() };
        }
    }
}

namespace Extensions
{
    public static class Extension
    {
        public static int GetId(this Tenant tenant, IQueryable<TenantsEntity> tenantsDb)
        {
            if (tenant.Id != 0) return (int)tenant.Id;
            else return tenantsDb.First(t => t.FirstName == tenant.FirstName &&
                t.LastName == tenant.LastName && t.Age == tenant.Age).Id;
        }

        public static int GetId(this ApartmentBlock apartmentBlock, IQueryable<ApartmentBlocksEntity> apartmentBlocksEntities)
        {
            if (apartmentBlock.Id != 0) return (int)apartmentBlock.Id;
            else return apartmentBlocksEntities.First(t => t.City == apartmentBlock.City &&
                t.Street == apartmentBlock.Street && t.Number == apartmentBlock.Number).Id;
        }
    }
}