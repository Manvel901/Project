﻿using AutoMapper;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Ocsp;
using static Diplom.Models.AppDbContext;

namespace Diplom.Services
{
    public class PenaltyesServices : IPenaltyService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;



        public PenaltyesServices(AppDbContext context, IMapper mapper, IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }
        public int CreatePenalty(PenaltyDto penaltyDto, int reservationId)
        {
            if (penaltyDto.Amount <= 0) throw new ArgumentException("Amount must be positive.");
            if (string.IsNullOrWhiteSpace(penaltyDto.BookTitle)) throw new ArgumentException("BookTitle required.");

            var reservationExists = _context.Reserv.Any(r => r.Id == reservationId);
            if (!reservationExists) throw new KeyNotFoundException("Reservation not found.");

            var entity = _mapper.Map<Penalties>(penaltyDto);
            if (entity.IssueDate == default) entity.IssueDate = DateTime.UtcNow;

            _context.Penalties.Add(entity);
            _context.SaveChanges();

            // Добавляем связь в явную join-таблицу
            _context.RservPenals.Add(new RservPenal { ReservationId = reservationId, PenaltyId = entity.Id });
            _context.SaveChanges();

            return entity.Id;
        }

        public bool PayPenalty(PenaltyDto penaltyDto)
        {
            using (_context)
            {
                if (penaltyDto.AmountPaid <= 0) throw new ArgumentException("Payment must be positive.");
                var penal = _context.Penalties.FirstOrDefault(x => x.Id == penaltyDto.Id || x.IsCancelled);
                if (penal == null) return false;

                var remaining = penal.Amount - penal.AmountPaid;
                var toApply = Math.Min(remaining, penaltyDto.AmountPaid);
                if (toApply <= 0) return false;

                penal.AmountPaid += toApply;
                if (penal.AmountPaid >= penal.Amount) penal.PaidAtUtc = (penaltyDto.PaidAtUtc?.ToUniversalTime() ?? DateTime.UtcNow);
                _context.SaveChanges();
                return true;
            }
        }

        public PenaltyDto? GetPenaltyById(int penaltyId)
        {
            using (_context)
            {
                var penalEntity = _context.Penalties.FirstOrDefault(x => x.Id == penaltyId);

                return _mapper.Map<PenaltyDto>(penalEntity);
            }
        }

      

        public IEnumerable<PenaltyDto> GetPenaltiesByBookTitle(string bookTitle)
        {
            using (_context)
            {
                var list = _context.Penalties
                   .Where(x => string.Equals(x.BookTitle, bookTitle, StringComparison.OrdinalIgnoreCase))
                   .OrderByDescending(x => x.IssueDate)
                   .ToList();

                return list.Select(x => _mapper.Map<PenaltyDto>(x));
            }
        }

        

        //public int ApplyOverduePenalties(DateTime asOfUtc)
        //{
        //    // Требует интеграции с сущностью reservation/loan. В заглушке — 0.
        //    return 0;
        //}

        //public bool CancelPenalty(PenaltyDto penaltyDto)
        //{
        //    using (_context)
        //    {
        //        var penal = _context.Penalties.FirstOrDefault(x => x.Id == penaltyDto.Id);
        //        if (penal == null) return false;
        //        if (penal.IsCancelled) return false;
        //        penal.IsCancelled = true;
        //        if (!string.IsNullOrWhiteSpace(penaltyDto.))
        //            penal.BookTitle = $"{penal.BookTitle} (Cancelled: {reason})";
        //        _context.SaveChanges();
        //        return true;
        //    }
        //}

      
    }
}

