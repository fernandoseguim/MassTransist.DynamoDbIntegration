using System;
using System.Globalization;
using System.Text.RegularExpressions;
using AutoMapper;
using Newtonsoft.Json;
using Orchestrator.Service.Contracts.Events;
using Orchestrator.Service.Observers;

namespace Orchestrator.Service.Mappers
{
    public class AuditProfile : Profile
    {
        public AuditProfile()
        {
            CreateMap<FundTransferWasReceived, IAuditEvent>()
                .ForMember(a => a.AggregateId, b => b.MapFrom(x => $"TRANSACTION_ID_{x.AuthenticationCode}"))
                .ForMember(a => a.CorrelationId, b => b.MapFrom(x => x.CorrelationId))
                .ForMember(a => a.Category, b => b.MapFrom(x => ""))
                .ForMember(a => a.Name, b => b.MapFrom(x => FormatTypeName(x.GetType())))
                .ForMember(a => a.CompanyKey, b => b.MapFrom(x => x.Company.ToUpper()))
                .ForMember(a => a.CreatedAt, b => b.MapFrom(x => x.CreatedAt))
                .ForMember(a => a.BankAccount, b => b.MapFrom(x => x.BankAccount))
                .ForMember(a => a.BankBranch, b => b.MapFrom(x => x.BankBranch))
                .ForMember(a => a.DocumentNumber, b => b.MapFrom(x => x.Document))
                .ForMember(a => a.Status, b => b.MapFrom(x => EventStatus.ACTIVATED))
                .ForMember(a => a.Type, b => b.MapFrom(x => EventType.INFO))
                .ForMember(a => a.Timestamp, b => b.MapFrom(x => DateTime.UtcNow))
                .ForMember(a => a.Description, b => b.MapFrom(x => x.Description))
                .ForMember(a => a.Data, b => b.MapFrom(x => JsonConvert.SerializeObject(x)))
                .ForMember(a => a.Amount, b => b.MapFrom(x => x.Amount))
                .ForMember(a => a.UpdatedAt, b => b.Ignore())
                .ForMember(a => a.CardId, b => b.Ignore())
                .ForMember(a => a.Label, b => b.Ignore());
        }
        
        private static string FormatTypeName(Type type)
        {
            var typeName = type.Name;

            if(typeName.StartsWith("I")) { typeName = typeName.Substring(1, typeName.Length - 1); }

            var splitted = Regex.Replace(typeName, "([A-Z])", " $1", RegexOptions.Compiled)
                    .Trim()
                    .Split(' ');

            var entityName = string.Join("_", splitted);

            return entityName.ToUpper(CultureInfo.InvariantCulture);
        }
    }
}
