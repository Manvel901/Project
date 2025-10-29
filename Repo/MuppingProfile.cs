using AutoMapper;
using Diplom.Models;
using Diplom.Models.dto;

namespace Diplom.Repo
{
    public class MuppingProfile:Profile
    {
        public MuppingProfile()
        {
            CreateMap<Book, BookDto>(MemberList.Destination).ReverseMap();
            CreateMap<Autors, AutorDto>(MemberList.Destination).ReverseMap();
            CreateMap<User, UserDto>(MemberList.Destination).ReverseMap();
            CreateMap<Genres, GenresDto>(MemberList.Destination).ReverseMap();
            CreateMap<Reservation, ReservationDto>(MemberList.Destination).ReverseMap();
            CreateMap<Penalties, PenaltyDto>(MemberList.Destination).ReverseMap();
            CreateMap<RservPenal, ReservPenalDto>(MemberList.Destination).ReverseMap();
            CreateMap<EmailEntity, EmailDto>(MemberList.Destination).ReverseMap();
        }
    }
}
